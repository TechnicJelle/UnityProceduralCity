#nullable enable
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Point
{
	private readonly RoadGenerator _roadGenerator;

	public Vector2 Pos;
	public readonly List<Road> Connections = new();

	private readonly bool _start = false;
	public bool Head { get; private set; } = true;
	private readonly Vector2 _dir;
	private float _splitChance;


	///for initial points
	public Point(RoadGenerator roadGenerator, float x, float y)
	{
		_roadGenerator = roadGenerator;

		Pos = new Vector2(x, y);
		float angle = _roadGenerator.RandomRange(0, Mathf.PI * 2);
		_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		_splitChance = 0;
		_start = true;
	}

	///for a split road; we do not care about dir
	private Point(RoadGenerator roadGenerator, Vector2 newPos)
	{
		_roadGenerator = roadGenerator;

		Pos = newPos;
		Head = false;
	}

	///for stepped roads
	private Point(RoadGenerator roadGenerator, Vector2 newPos, Vector2 prevDir, float prevSplitChance)
	{
		_roadGenerator = roadGenerator;

		Pos = newPos;

		//rotate a bit
		Vector2 newDir = Vector2Rotate(prevDir, _roadGenerator.RandomRange(
			-_roadGenerator.maxRotationAmountRadians, _roadGenerator.maxRotationAmountRadians));
		_dir = newDir;

		//stop if out of bounds
		if (Pos.x < 0 || Pos.x > _roadGenerator.width || Pos.y < 0 || Pos.y > _roadGenerator.height)
		{
			Head = false;
		}

		//take over split chance of prev, and increase it a bit
		//this makes it so roads are more likely to split off, the longer they have existed/haven't split
		_splitChance = prevSplitChance + _roadGenerator.newRoadChance;
	}

	public void Render(int pointsIndex)
	{
		if (_roadGenerator.showPointsSphere)
		{
			if (_start)
			{
				Gizmos.color = Color.grey;
			}
			else
			{
				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (Head)
				{
					Gizmos.color = Color.red;
				}
				else
				{
					Gizmos.color = Color.green;
				}
			}

			Gizmos.color = Gizmos.color.WithAlpha(0.3f);
			Gizmos.DrawSphere(Pos, _roadGenerator.sphereSizeDefault + _splitChance * _roadGenerator.sphereSizeIncrease);
		}

		//Index label
		if (_roadGenerator is {showPointsIndex: true})
		{
			Matrix4x4 pushMatrix = Handles.matrix;
			Handles.matrix = Gizmos.matrix;
			GUIStyle style = new() {normal = {textColor = Color.white}};
			Handles.Label(Pos, "i:" + pointsIndex, style);
			Handles.matrix = pushMatrix;
		}
	}

	public void Step()
	{
		if (!Head) return;

		//shall we split?
		if (_roadGenerator.RandomRange(0.0f, 1.0f) < _splitChance)
		{
			_splitChance = 0;
			Vector2 newDir = _roadGenerator.RandomRange(0.0f, 1.0f) < 0.5f
				? Vector2Rotate(_dir, Mathf.PI / 2f)
				: Vector2Rotate(_dir, -Mathf.PI / 2f);
			StepDir(newDir);
		}

		StepDir(_dir);
	}

	private struct IntersectionData
	{
		public readonly Vector2 Pos;
		public readonly int RoadIndex;

		public IntersectionData(Vector2 pos, int roadIndex)
		{
			Pos = pos;
			RoadIndex = roadIndex;
		}
	}

	private void StepDir(Vector2 stepDirection)
	{
		//take a step in the direction that we're going
		Vector2 newPos = Pos + stepDirection * _roadGenerator.stepDistance;
		//create tentative new point at that new position
		Point newPoint = new(_roadGenerator, newPos, stepDirection, _splitChance);
		//create tentative new road with that new point
		Road newRoad = new(_roadGenerator, this, newPoint);

		//check whether the new road intersects with any existing roads
		List<IntersectionData> intersections = new();
		for(int i = 0; i < _roadGenerator.Roads.Count; i++)
		{
			Road otherRoad = _roadGenerator.Roads[i];

			//do not test against own road(s), because there is intersection due to the overlap caused by sharing the same point
			if (Connections.Contains(otherRoad)) continue;

			Vector2? intersectionPosition = newRoad.Intersect(otherRoad);
			if (intersectionPosition != null)
			{
				//we record all intersected roads, in case this step intersects with multiple
				//this is rare, but it _can_ happen
				intersections.Add(new IntersectionData(intersectionPosition.Value, i));
			}
		}

		if (intersections.Count == 0)
		{
			//we did not intersect with any existing road,
			//so all is good to save this next step permanently

			//add the new road to this point's actual connections...
			Connections.Add(newRoad);
			//...and add the new road to the new point's connections
			newPoint.Connections.Add(newRoad);

			//register the new point and road globally
			_roadGenerator.Points.Add(newPoint);
			_roadGenerator.Roads.Add(newRoad);
		}
		else
		{
			//new road intersects with one or more already-existing roads!
			//that means that this road cannot continue onwards.
			//instead, we split find the closest road we intersected with,
			//split it, and connect this point to that split point!


			//go through all intersections and find the closest one
			int closestRoadIndex = -1;
			float closestRoadDistance = float.MaxValue;
			for(int i = 0; i < intersections.Count; i++)
			{
				IntersectionData id = intersections[i];
				float dist = Vector2.Distance(Pos, id.Pos);
				if (dist <= closestRoadDistance)
				{
					closestRoadDistance = dist;
					closestRoadIndex = i;
				}
			}

			//get data from the closest intersection point, to do the splitting with.
			Vector2 intersectionPosition = intersections[closestRoadIndex].Pos;
			int roadIntersectedWithIndex = intersections[closestRoadIndex].RoadIndex;

			// == cleanup first ==

			//get the road we intersected with
			Road roadIntersectedWith = _roadGenerator.Roads[roadIntersectedWithIndex];
			//get the points of the road we just intersected with
			Point p1 = roadIntersectedWith.P1;
			Point p2 = roadIntersectedWith.P2;

			//because it will be replaced by two new roads,
			//remove the road from the three places it's stored:
			{
				//remove the road from those connections
				p1.Connections.Remove(roadIntersectedWith);
				p2.Connections.Remove(roadIntersectedWith);
				//from the global roads
				_roadGenerator.Roads.RemoveAt(roadIntersectedWithIndex);
			}

			//create new point at the intersection point
			Point intersectionPoint = new(_roadGenerator, intersectionPosition);
			//and add it to the global points list
			_roadGenerator.Points.Add(intersectionPoint);

			//create the two new roads!
			Road r1 = new(_roadGenerator, p1, intersectionPoint);
			Road r2 = new(_roadGenerator, intersectionPoint, p2);

			//add r1 to the three places it should be stored:
			{
				//the points
				p1.Connections.Add(r1);
				intersectionPoint.Connections.Add(r1);
				//the global
				_roadGenerator.Roads.Add(r1);
			}
			//add r2 to the three places it should be stored:
			{
				//the points
				p2.Connections.Add(r2);
				intersectionPoint.Connections.Add(r2);
				//the global
				_roadGenerator.Roads.Add(r2);
			}

			//now the original road has been fully properly split!

			//finally, we can create one last road to connect this point to the intersection point
			Road r3 = new(_roadGenerator, this, intersectionPoint);
			//add r3 to the three places it should be stored:
			{
				//the points
				Connections.Add(r3);
				intersectionPoint.Connections.Add(r3);
				//the global
				_roadGenerator.Roads.Add(r3);
			}
		}

		//whether we set a next step, or connected to an existing road,
		//this point is done and will not step again.
		Head = false;
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
