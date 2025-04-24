using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RoadGenerator : MonoBehaviour
{

#region Constants

	// Global
	[SerializeField]
	public bool automaticSeed = true;

	[SerializeField]
	public int seed = 0;

	// Generation
	[SerializeField]
	public float width = 1000.0f;

	[SerializeField]
	public float height = 1000.0f;

	// Spreading
	[SerializeField]
	public float middleSpawnFactor = 0.5f;

	[SerializeField]
	public int initialStartPoints = 4;

	// Stepping
	[SerializeField]
	public float stepSize = 50.0f;

	[SerializeField]
	public float maxRotationAmountRadians = 0.4f;

	[SerializeField]
	public float newRoadChance = 0.7f;

	// Merging
	[SerializeField]
	public float mergeDistance = 0.5f;

	[SerializeField]
	public float acceptableStraightsMin = 20.0f;

	[SerializeField]
	public float acceptableStraightsMax = 300.0f;

	// Debug Drawing
	[SerializeField]
	public float arrowHeadSize = 0.2f;

	[SerializeField]
	public float arrowHeadAngle = 45.0f;

	[SerializeField]
	public bool showPointsIndex = false;

	[SerializeField]
	public bool showStraightness = false;

	// Prefab
	[SerializeField]
	public GameObject roadPrefab;

#endregion

#region Fields

	public readonly List<Point> Points = new();

	[CanBeNull]
	private Random _rng;

#endregion

	public bool HasPoints() => Points.Count > 0;

	public void ResetRng() => _rng = null;

	public float RandomRange(float minNumber, float maxNumber)
	{
		_rng ??= automaticSeed ? new Random() : new Random(seed);
		return (float)_rng.NextDouble() * (maxNumber - minNumber) + minNumber;
	}

	private void OnDrawGizmos()
	{
		//Apply the road generator transform to the gizmos
		Gizmos.matrix = transform.localToWorldMatrix;

		//Draw the middle spawn area
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(width * 2 * middleSpawnFactor, height * 2 * middleSpawnFactor, 0));

		//Draw the bounding box
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 0));

		//The points are stored offset, so we move the drawing by half width and height
		Matrix4x4 translationMatrix = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, 0));
		Gizmos.matrix *= translationMatrix;

		// Draw the points
		int pointCount = Points.Count;
		for(int i = 0; i < pointCount; i++)
		{
			try
			{
				Point p = Points[i];
				p.Render(i);
			}
			catch(ArgumentOutOfRangeException)
			{
				//we don't mind if the rendering is wrong/behind for a moment
			}
		}

		// Reset the matrix to identity to avoid affecting other gizmos
		Gizmos.matrix = Matrix4x4.identity;
	}

	public void ClearRoads()
	{
		ResetRng();
		Points.Clear();
	}

	public void SpreadStartingPoints()
	{
		for(int i = 0; i < initialStartPoints; i++)
		{
			float x = RandomRange(width * (0.5f - middleSpawnFactor), width * (0.5f + middleSpawnFactor));
			float y = RandomRange(height * (0.5f - middleSpawnFactor), height * (0.5f + middleSpawnFactor));
			Points.Add(new Point(this, x, y));
		}
	}

	public void DoStepping()
	{
		while(!CheckDoneStepping())
		{
			TakeAStep();
		}
	}

	private void TakeAStep()
	{
		int pointsCount = Points.Count;
		for(int i = 0; i < pointsCount; i++)
		{
			Point p = Points[i];
			if (p.Head)
			{
				Points.AddRange(p.Step());
			}
		}
	}

	private bool CheckDoneStepping()
	{
		foreach(Point p in Points)
		{
			if (p.Head)
			{
				return false;
			}
		}

		return true;
	}

	///make directional links double-sided
	public void DoubleLink()
	{
		for(int i = 0; i < Points.Count - 1; i++)
		{
			for(int j = i + 1; j < Points.Count; j++)
			{
				Point p1 = Points[i];
				Point p2 = Points[j];
				if (p1.Connections.Contains(p2))
				{
					if (!p2.Connections.Contains(p1))
					{
						p2.Connections.Add(p1);
					}
				}
				if (p2.Connections.Contains(p1))
				{
					if (!p1.Connections.Contains(p2))
					{
						p1.Connections.Add(p2);
					}
				}
			}
		}
	}

	/// <param name="index">the index in the Points list to take out</param>
	private void TakeOutPoint(int index)
	{
		Point removedPoint = Points[index];
		for(int i = removedPoint.Connections.Count - 1; i >= 0; i--)
		{
			Point connection = removedPoint.Connections[i];
			connection.Connections.Remove(removedPoint);
			//add all of the removedConnections' other connections to the current point's connections
			for(int j = connection.Connections.Count - 1; j >= 0; j--)
			{
				Point connectionOfConnection = connection.Connections[j];
				if (connectionOfConnection != removedPoint && !removedPoint.Connections.Contains(connectionOfConnection))
				{
					removedPoint.Connections.Add(connectionOfConnection);
				}
			}
		}

		Points.RemoveAt(index);
		Debug.Log("Took out point " + index);
	}

	public void MergeByDistance()
	{
		for(int i = 0; i < Points.Count - 1; i++)
		{
			for(int j = i + 1; j < Points.Count; j++)
			{
				Point p1 = Points[i];
				Point p2 = Points[j];
				float distance = Vector2.Distance(p1.Pos, p2.Pos);
				if (distance < mergeDistance)
				{
					//remove the point with the worst straightness
					float straightness1 = p1.CalculateStraightness();
					float straightness2 = p2.CalculateStraightness();
					TakeOutPoint(straightness1 < straightness2 ? i : j);
				}
			}
		}
	}

	public void MergeUnacceptableStraights()
	{
		for(int i = Points.Count - 1; i >= 0; i--)
		{
			Point p = Points[i];
			float straightness = p.CalculateStraightness();
			bool shouldBeRemoved = straightness < acceptableStraightsMin || straightness > acceptableStraightsMax;
			if (shouldBeRemoved)
			{
				TakeOutPoint(i);
			}
		}
	}

	///check if all points' connections are actually in the points list
	public void VerifyConnections()
	{
		bool allGood = true;
		for(int i = 0; i < Points.Count; i++)
		{
			Point p = Points[i];
			for(int j = 0; j < p.Connections.Count; j++)
			{
				Point c = p.Connections[j];
				if (Points.Contains(c)) continue;

				Debug.LogError($"Point {i} has a connection to a point that is not in the list: {j}");
				allGood = false;
			}
		}
		if (allGood)
		{
			Debug.Log("All connections are valid");
		}
		else
		{
			Debug.LogError("Some connections are invalid");
		}
	}
}
