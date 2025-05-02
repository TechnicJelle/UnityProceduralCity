#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadGenerator : MonoBehaviour
{

#region Options

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
	public float middleSpawnFactor = 0.25f;

	[SerializeField]
	public int initialStartPoints = 4;


	// Stepping
	[SerializeField]
	public float stepDistance = 50.0f;

	[SerializeField]
	public float maxRotationAmountRadians = 0.4f;

	[SerializeField]
	public float newRoadChance = 0.7f;


	// Merging
	[SerializeField]
	public float mergeDistance = 3.0f;


	// Mesh
	[SerializeField]
	public float meshRadius = 2.0f;

	[SerializeField]
	public float textureStretching = 1.0f;


	// Buildings
	[SerializeField]
	public float buildingAlongRoadChance = 1.0f;

	[SerializeField]
	public float minRoadLengthForBuilding = 20.0f;

	[SerializeField]
	public float buildingLengthFactorMin = 0.6f;
	[SerializeField]
	public float buildingLengthFactorMax = 0.9f;

	[SerializeField]
	public float buildingWidthFactorMin = 0.1f;
	[SerializeField]
	public float buildingWidthFactorMax = 0.5f;

	[SerializeField]
	public float buildingHeightFactorMin = 0.1f;
	[SerializeField]
	public float buildingHeightFactorMax = 0.3f;


	// Debug Drawing
	[SerializeField]
	public bool showPointsSphere = true;

	[SerializeField]
	public float sphereSizeDefault = 4.0f;

	[SerializeField]
	public float sphereSizeIncrease = 5.0f;

	[SerializeField]
	public bool showPointsIndex = false;

	[SerializeField]
	public bool showRoadLines = true;

	[SerializeField]
	public bool showBuildingBoxes = true;

#endregion

#region Fields

	public readonly List<Point> Points = new();
	public readonly List<Road> Roads = new();
	private readonly List<BuildingBox> _buildingBoxes = new();

	private Random? _rng;

#endregion

	public bool HasPoints() => Points.Count > 0;

	public bool HasRoads() => Roads.Count > 0;

	public bool HasBuildings() => _buildingBoxes.Count > 0;

	public bool HasMesh() => GetComponent<MeshFilter>().sharedMesh != null;

	public void ResetRng() => _rng = null;

	public float RandomRange(float minNumber, float maxNumber)
	{
		_rng ??= automaticSeed ? new Random() : new Random(seed);
		return (float)_rng.NextDouble() * (maxNumber - minNumber) + minNumber;
	}

	private void OnDrawGizmosSelected()
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
		Matrix4x4 translationMatrix = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, -0.1f));
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

		// Draw the roads
		if (showRoadLines)
		{
			int roadCount = Roads.Count;
			for(int i = 0; i < roadCount; i++)
			{
				try
				{
					Road r = Roads[i];
					r.Render();
				}
				catch(ArgumentOutOfRangeException)
				{
					//we don't mind if the rendering is wrong/behind for a moment
				}
			}
		}

		// Draw the buildings
		if (showBuildingBoxes)
		{
			int buildingBoxesCount = _buildingBoxes.Count;
			for(int i = 0; i < buildingBoxesCount; i++)
			{
				BuildingBox buildingBox = _buildingBoxes[i];
				Gizmos.color = Color.green;
				Matrix4x4 pushMatrix = Gizmos.matrix;
				Gizmos.matrix *= Matrix4x4.TRS(buildingBox.Pos, buildingBox.Rotation, Vector3.one);
				//x&y zero because the matrix already contains the position
				//z is half the height, to put the bottom of the box on the ground
				Gizmos.DrawCube(new Vector3(0, 0, buildingBox.Height / -2), new Vector3(buildingBox.Surface.x, buildingBox.Surface.y, buildingBox.Height));
				Gizmos.matrix = pushMatrix;
			}
		}

		// Reset the matrix to identity to avoid affecting other gizmos
		Gizmos.matrix = Matrix4x4.identity;
	}

	public void ClearRoads()
	{
		ResetRng();
		Points.Clear();
		Roads.Clear();
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
			p.Step();
		}
	}

	private bool CheckDoneStepping()
	{
		//is there any head? that means it's still going
		foreach(Point p in Points)
		{
			if (p.Head)
			{
				return false;
			}
		}

		return true;
	}

	public void MergeByDistance()
	{
		throw new NotImplementedException();
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
				Road r = p.Connections[j];
				if (Points.Contains(r.P1) && Points.Contains(r.P2)) continue;

				Debug.LogError($"Point {i} has a connection to a point that is not in the list: {j}");
				allGood = false;
			}
		}
		//check if all roads are in the points list
		for(int i = 0; i < Roads.Count; i++)
		{
			Road r = Roads[i];
			if (Points.Contains(r.P1) && Points.Contains(r.P2)) continue;

			Debug.LogError($"Road {i} has a connection to a point that is not in the list");
			allGood = false;
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

	public void ClearRoadMesh()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.sharedMesh.Clear();
		meshFilter.sharedMesh = null;
	}

	/// <remarks>cannot be called on a separate thread, due to using Unity APIs</remarks>
	public void GenerateRoadMesh()
	{
		CombineInstance[] combine = new CombineInstance[Roads.Count];
		for(int i = 0; i < combine.Length; i++)
		{
			Road road = Roads[i];
			combine[i].mesh = road.ToMesh();
			//The points are stored offset, so we move the models by half width and height
			combine[i].transform = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, 0));
		}

		Mesh combinedMesh = new() {name = "RoadMesh"};
		combinedMesh.CombineMeshes(combine);
		combinedMesh.RecalculateNormals();
		combinedMesh.RecalculateBounds();
		combinedMesh.Optimize();
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.sharedMesh = combinedMesh;
	}

	public void ClearBuildings()
	{
		_buildingBoxes.Clear();
	}

	public void GenerateBuildingsAlongRoads()
	{
		foreach(Road road in Roads)
		{
			if (RandomRange(0.0f, 1.0f) < buildingAlongRoadChance)
			{
				//check if the road is long enough to place a building
				if (road.GetMagnitude() < minRoadLengthForBuilding)
					continue;

				GenerateBuildingAlongRoad(road, -1.1f);
				GenerateBuildingAlongRoad(road, 1.1f);
			}
		}
	}

	private void GenerateBuildingAlongRoad(Road road, float side)
	{
		//building size (it's relative to its road's size)
		float buildingWidth = road.GetMagnitude() * RandomRange(buildingWidthFactorMin, buildingWidthFactorMax);
		float buildingLength = road.GetMagnitude() * RandomRange(buildingLengthFactorMin, buildingLengthFactorMax);
		float buildingHeight = road.GetMagnitude() * RandomRange(buildingHeightFactorMin, buildingHeightFactorMax);

		//position relative to the middle of the road, but offset by the road mesh radius
		Vector2 roadDirection = road.GetDirection();
		Vector2 perpendicular = new(-roadDirection.y, roadDirection.x);
		Vector2 offset = perpendicular * (meshRadius + buildingWidth / 2f);
		Vector2 pos = road.GetMiddlePos() + offset * side;

		//inputs
		Vector2 surface = new(buildingWidth, buildingLength);
		Quaternion rotation = Quaternion.FromToRotation(Vector3.up, road.GetDirection());
		BuildingBox potentialNewBuildingBox = new(pos, surface, buildingHeight, rotation);

		//check if the building box is intersecting a road
		bool overlapsAnyRoad = Roads.Any(otherRoad => potentialNewBuildingBox.CheckOverlap(otherRoad));

		//check if the building box intersects with any other building boxes
		// bool overlapsAnyOtherBuildingBox = _buildingBoxes.Any(buildingBox => buildingBox.CheckOverlap(potentialNewBuildingBox));
		if (!overlapsAnyRoad)
		{
			_buildingBoxes.Add(potentialNewBuildingBox);
		}
	}
}
