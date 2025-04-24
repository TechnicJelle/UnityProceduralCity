#nullable enable
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Point
{
	private readonly RoadGenerator _roadGenerator;

	public Vector2 Pos;
	private readonly Vector2 _dir;
	public bool Head = true;
	public readonly List<Point> Connections = new();
	private float _splitChance; //0..1
	private readonly bool _startingPoint = false;

	public Point(RoadGenerator roadGenerator, float x, float y)
	{
		_roadGenerator = roadGenerator;

		Pos = new Vector2(x, y);
		float angle = _roadGenerator.RandomRange(0, Mathf.PI * 2);
		_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		_splitChance = 0.0f;
		_startingPoint = true;
	}

	private Point(RoadGenerator roadGenerator, Vector2 newPos,
		Vector2 newDir, Point previous)
	{
		_roadGenerator = roadGenerator;

		Pos = newPos;
		_dir = newDir;
		if (Pos.x < 0.0f || Pos.x > _roadGenerator.width ||
		    Pos.y < 0.0f || Pos.y > _roadGenerator.height) Head = false;
		Connections.Add(previous);
		_splitChance = previous._splitChance + roadGenerator.newRoadChance;
	}

	public float CalculateStraightness()
	{
		//compare all connections to eachother, and compare the angle between them
		float minStraightness = float.MaxValue;
		int connectionCount = Connections.Count;
		for(int i = 0; i < connectionCount; i++)
		{
			for(int j = i + 1; j < connectionCount; j++)
			{
				float angle = Vector2.Angle(Connections[i].Pos - Pos, Connections[j].Pos - Pos);
				if (angle < minStraightness) minStraightness = angle;
			}
		}

		if (minStraightness > 360) minStraightness = 180;

		return minStraightness;
	}

	public void Render(int pointsIndex)
	{
		//Colour
		if (_startingPoint)
		{
			Gizmos.color = Color.grey;
		}
		else
		{
			Gizmos.color = Head ? Color.red : Color.yellow;
		}

		//Circle
		Gizmos.color = Gizmos.color.WithAlpha(0.3f);
		Gizmos.DrawSphere(Pos, _startingPoint ? 0.2f : 0.1f);

		//Index label
		if (_roadGenerator is {showPointsIndex: true, showStraightness: false})
		{
			Matrix4x4 pushMatrix = Handles.matrix;
			Handles.matrix = Gizmos.matrix;
			GUIStyle style = new() {normal = {textColor = Color.white}};
			Handles.Label(Pos, "i:" + pointsIndex, style);
			Handles.matrix = pushMatrix;
		}

		//Straightness label
		if (_roadGenerator is {showStraightness: true, showPointsIndex: false})
		{
			Matrix4x4 pushMatrix = Handles.matrix;
			Handles.matrix = Gizmos.matrix;
			float straightness = CalculateStraightness();
			GUIStyle style = new() {normal = {textColor = straightness < _roadGenerator.acceptableStraightsMin || straightness > _roadGenerator.acceptableStraightsMax ? Color.red : Color.white}};
			Handles.Label(Pos, "Straightness: " + straightness, style);
			Handles.matrix = pushMatrix;
		}

		//Both labels
		if (_roadGenerator is {showPointsIndex: true, showStraightness: true})
		{
			Matrix4x4 pushMatrix = Handles.matrix;
			Handles.matrix = Gizmos.matrix;
			float straightness = CalculateStraightness();
			GUIStyle style = new() {normal = {textColor = straightness < _roadGenerator.acceptableStraightsMin || straightness > _roadGenerator.acceptableStraightsMax ? Color.red : Color.white}};
			Handles.Label(Pos, "i:" + pointsIndex + " Straightness: " + straightness, style);
			Handles.matrix = pushMatrix;
		}

		//Arrows
		int connectionCount = Connections.Count;
		for(int i = 0; i < connectionCount; i++)
		{
			Point c = Connections[i];

			Gizmos.color = Gizmos.color.WithAlpha(1.0f);
			Arrow(Pos, c.Pos, _roadGenerator.arrowHeadSize, _roadGenerator.arrowHeadAngle * Mathf.Deg2Rad);
		}
	}

	private bool CheckIntersectWithAnyConnections(Vector2 currentPos, Vector2 newPos)
	{
		foreach(Point c in Connections)
		{
			if (Intersect3(c.Pos, Pos, currentPos, newPos) != null)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<Point> Step()
	{
		Head = false;
		List<Point> newPoints = new();

		//always take a next steppy
		Point? alwaysNewPoint = GenerateNewSteppy(_dir);
		if (alwaysNewPoint == null) return newPoints;

		//we did not connect to an already existing point
		newPoints.Add(alwaysNewPoint);

		//maybe we can split off?
		if (_roadGenerator.RandomRange(0, 1) > _splitChance) return newPoints;

		Vector2 newDir = _roadGenerator.RandomRange(0, 1) < 0.5f
			? Vector2Rotate(_dir, Mathf.PI / 2f)
			: Vector2Rotate(_dir, -Mathf.PI / 2f);
		Point? maybeNewPoint = GenerateNewSteppy(newDir);

		if (maybeNewPoint == null) return newPoints;

		maybeNewPoint._splitChance = 0;
		alwaysNewPoint._splitChance = 0;
		newPoints.Add(maybeNewPoint);

		return newPoints;
	}

	/// Returns null when not made a new steppy, but connected to another steppy instead
	private Point? GenerateNewSteppy(Vector2 stepDir)
	{
		Vector2 newPos = Pos + stepDir * _roadGenerator.stepSize;
		Point? intersectionPoint = null;
		foreach(Point? p in _roadGenerator.Points)
		{
			if (p == this) continue;
			if (p.CheckIntersectWithAnyConnections(Pos, newPos))
			{
				intersectionPoint = p;
				break;
			}
		}
		if (intersectionPoint == null)
		{
			Vector2 newDir = Vector2Rotate(stepDir,
				_roadGenerator.RandomRange(-_roadGenerator.maxRotationAmountRadians,
					_roadGenerator.maxRotationAmountRadians));
			return new Point(_roadGenerator, newPos, newDir, this);
		}

		List<Point> potentialConnectionPoints = new();
		potentialConnectionPoints.AddRange(intersectionPoint.Connections);
		potentialConnectionPoints.Add(intersectionPoint);
		Point? closestPoint = null;
		float smallestDistance = float.MaxValue;
		//find the closest point to this point
		foreach(Point pp in potentialConnectionPoints)
		{
			float dist = Vector2.Distance(Pos, pp.Pos);
			if (dist >= smallestDistance) continue;
			smallestDistance = dist;
			closestPoint = pp;
		}
		closestPoint?.Connections.Add(this);

		return null;

	}

	private static Vector2? Intersect3(
		Vector2 lineR1Start, Vector2 lineR1End,
		Vector2 lineR2Start, Vector2 lineR2End)
	{
		//Adapted from https://stackoverflow.com/a/1968345/8109619
		float p0X = lineR1Start.x;
		float p0Y = lineR1Start.y;
		float p1X = lineR1End.x;
		float p1Y = lineR1End.y;

		float p2X = lineR2Start.x;
		float p2Y = lineR2Start.y;
		float p3X = lineR2End.x;
		float p3Y = lineR2End.y;

		float s1X = p1X - p0X;
		float s1Y = p1Y - p0Y;
		float s2X = p3X - p2X;
		float s2Y = p3Y - p2Y;

		float h = -s2X * s1Y + s1X * s2Y;
		float s = (-s1Y * (p0X - p2X) + s1X * (p0Y - p2Y)) / h;
		float t = (s2X * (p0Y - p2Y) - s2Y * (p0X - p2X)) / h;

		if (s is < 0 or > 1 || t is < 0 or > 1) return null; // No collision

		// Collision detected
		float iX = p0X + t * s1X;
		float iY = p0Y + t * s1Y;
		return new Vector2(iX, iY);
	}

	private static void Arrow(Vector2 start, Vector2 end, float headSize, float headAngle)
	{
		Vector2 ray = end - start;
		end = start + ray / 2;
		Gizmos.DrawLine(start, end);
		Matrix4x4 pushMatrix = Gizmos.matrix;
		Gizmos.matrix *= Matrix4x4.Translate(new Vector2(end.x, end.y));
		float angle = Mathf.Atan2(start.x - end.x, end.y - start.y);
		Gizmos.matrix *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg));
		Vector2 headSizeVector = new(0, -headSize);
		Gizmos.DrawLine(Vector2.zero, Vector2Rotate(headSizeVector, -headAngle));
		Gizmos.DrawLine(Vector2.zero, Vector2Rotate(headSizeVector, headAngle));
		Gizmos.matrix = pushMatrix;
	}

	/// <summary>
	/// Copy the vector and rotate it by the given angle (in radians)
	/// </summary>
	/// <returns>The new vector</returns>
	private static Vector2 Vector2Rotate(Vector2 vec, float radians)
	{
		float temp = vec.x;
		vec.x = vec.x * Mathf.Cos(radians) - vec.y * Mathf.Sin(radians);
		vec.y = temp * Mathf.Sin(radians) + vec.y * Mathf.Cos(radians);
		return vec;
	}
}
