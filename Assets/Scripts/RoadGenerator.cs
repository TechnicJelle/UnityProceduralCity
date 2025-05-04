#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadGenerator : MonoBehaviour
{
#if UNITY_EDITOR

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

	[SerializeField]
	public List<Collider> collidersToBridgeOver = new();

	[SerializeField]
	public List<Collider> collidersToAvoid = new();


	// Spreading
	[SerializeField]
	public float middleSpawnFactor = 0.25f;

	[SerializeField]
	public int initialStartPoints = 4;


	// Stepping
	[SerializeField]
	public float stepDistance = 50.0f;

	[SerializeField]
	public float stepDistanceForBridges = 100.0f;

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


	// Buildings Generation
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


	// Buildings Object
	[SerializeField]
	public Material? buildingsMaterial;


	// Roofs Object
	[SerializeField]
	public Material? roofsMaterial;

	[SerializeField]
	public List<GameObject> roofPrefabs = new();


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

	public const float ANTI_Z = 0.1f;
	private const string BUILDINGS_OBJECT_NAME = "Buildings";
	private const string BUILDINGS_ROOFS_OBJECT_NAME = "BuildingsRoofs";

	private const string ROADS_MESH_CACHE_PATH = "Assets/MeshCaches/Roads.mesh";
	private const string BUILDINGS_MESH_CACHE_PATH = "Assets/MeshCaches/Buildings.mesh";
	private const string BUILDINGS_ROOFS_MESH_CACHE_PATH = "Assets/MeshCaches/BuildingsRoofs.mesh";

	public readonly List<Point> Points = new();
	public readonly List<Road> Roads = new();
	private readonly List<BuildingBox> _buildingBoxes = new();

	private Random? _rng;

#endregion

	public bool HasPoints() => Points.Count > 0;

	public bool HasRoads() => Roads.Count > 0;

	public bool HasRoadMesh() => GetComponent<MeshFilter>().sharedMesh != null;

	public bool HasBuildings() => _buildingBoxes.Count > 0;

	public bool HasBuildingsObject() => transform.Find(BUILDINGS_OBJECT_NAME) != null;

	public bool HasRoofsObject() => transform.Find(BUILDINGS_ROOFS_OBJECT_NAME) != null;

	public void ResetRng() => _rng = null;

	public float RandomRange(float minNumber, float maxNumber)
	{
		_rng ??= automaticSeed ? new Random() : new Random(seed);
		return (float)_rng.NextDouble() * (maxNumber - minNumber) + minNumber;
	}

	public int RandomRange(int minNumber, int maxNumber)
	{
		_rng ??= automaticSeed ? new Random() : new Random(seed);
		return _rng.Next(minNumber, maxNumber);
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
		//We also draw the gizmos a bit higher, to avoid z-fighting with the road meshes
		Matrix4x4 translationMatrix = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, -ANTI_Z));
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
				try
				{
					BuildingBox buildingBox = _buildingBoxes[i];
					buildingBox.Render();
				}
				catch(ArgumentOutOfRangeException)
				{
					//we don't mind if the rendering is wrong/behind for a moment
				}
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
			float x;
			float y;
			do
			{
				x = RandomRange(width * (0.5f - middleSpawnFactor), width * (0.5f + middleSpawnFactor));
				y = RandomRange(height * (0.5f - middleSpawnFactor), height * (0.5f + middleSpawnFactor));
			} while(IsPlaceToAvoid(new Vector2(x, y)) || IsPlaceToBridgeOver(new Vector2(x, y)));
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

	public bool IsPlaceToAvoid(Vector2 newPos)
	{
		Vector3 origin = new Vector3(newPos.x, 100, newPos.y) - new Vector3(width * 0.5f, 0, height * 0.5f);
		Vector3 direction = Vector3.down;
		if (Physics.Raycast(origin, direction, out RaycastHit hit))
		{
			if (collidersToAvoid.Contains(hit.collider))
			{
				return true;
			}
		}

		return false;
	}

	public bool IsPlaceToBridgeOver(Vector2 newPos)
	{
		Vector3 origin = new Vector3(newPos.x, 10, newPos.y) - new Vector3(width * 0.5f, 0, height * 0.5f);
		Vector3 direction = Vector3.down;
		if (Physics.Raycast(origin, direction, out RaycastHit hit))
		{
			if (collidersToBridgeOver.Contains(hit.collider))
			{
				return true;
			}
		}

		return false;
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

		AssetDatabase.DeleteAsset(ROADS_MESH_CACHE_PATH);
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

		AssetDatabase.CreateAsset(combinedMesh, ROADS_MESH_CACHE_PATH);

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(ROADS_MESH_CACHE_PATH);
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

				//do not build buildings along a bride over the river...
				if (road.Bridge)
					continue;

				GenerateBuildingAlongRoad(road, -1.001f);
				GenerateBuildingAlongRoad(road, 1.001f);
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

		//check if the building box is intersecting a place to avoid
		bool overlapsAnyPlaceToAvoid = potentialNewBuildingBox.IntersectsAPlaceToAvoid(this);
		if (overlapsAnyPlaceToAvoid) return;

		//check if the building box is intersecting a place to bridge over
		bool overlapsAnyPlaceToBridgeOver = potentialNewBuildingBox.IntersectsAPlaceToBridgeOver(this);
		if (overlapsAnyPlaceToBridgeOver) return;

		//check if the building box is intersecting a road
		bool overlapsAnyRoad = Roads.Any(otherRoad => potentialNewBuildingBox.CheckOverlap(otherRoad));
		if (overlapsAnyRoad) return;

		//check if the building box intersects with any other building boxes
		// bool overlapsAnyOtherBuildingBox = _buildingBoxes.Any(buildingBox => buildingBox.CheckOverlap(potentialNewBuildingBox));
		// if (overlapsAnyOtherBuildingBox) return;

		_buildingBoxes.Add(potentialNewBuildingBox);
	}

	public void ClearBuildingsObject()
	{
		Transform[] children = transform.GetComponentsInChildren<Transform>();
		foreach(Transform child in children)
		{
			if (child.name == BUILDINGS_OBJECT_NAME)
			{
				DestroyImmediate(child.gameObject);
			}
		}
		AssetDatabase.DeleteAsset(BUILDINGS_MESH_CACHE_PATH);
	}

	/// <remarks>cannot be called on a separate thread, due to using Unity APIs</remarks>
	public void GenerateBuildingsObject()
	{
		GameObject buildingsObject = CreateChild(BUILDINGS_OBJECT_NAME);

		//The points are stored offset, so we move the models by half width and height
		Matrix4x4 halfOff = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, 0));
		//create the mesh
		CombineInstance[] combine = new CombineInstance[_buildingBoxes.Count];
		for(int i = 0; i < combine.Length; i++)
		{
			BuildingBox buildingBox = _buildingBoxes[i];
			combine[i].mesh = buildingBox.ToMesh();
			combine[i].transform = halfOff;
		}

		Mesh combinedMesh = new() {name = "Buildings"};
		combinedMesh.CombineMeshes(combine);
		combinedMesh.RecalculateNormals();
		combinedMesh.RecalculateBounds();
		combinedMesh.Optimize();

		AssetDatabase.CreateAsset(combinedMesh, BUILDINGS_MESH_CACHE_PATH);

		MeshFilter meshFilter = buildingsObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(BUILDINGS_MESH_CACHE_PATH);

		MeshRenderer meshRenderer = buildingsObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = buildingsMaterial;
	}

	public void ClearRoofsObject()
	{
		Transform[] children = transform.GetComponentsInChildren<Transform>();
		foreach(Transform child in children)
		{
			if (child.name == BUILDINGS_ROOFS_OBJECT_NAME)
			{
				DestroyImmediate(child.gameObject);
			}
		}
		AssetDatabase.DeleteAsset(BUILDINGS_ROOFS_MESH_CACHE_PATH);
	}

	public void GenerateRoofsObject()
	{
		GameObject roofsObject = CreateChild(BUILDINGS_ROOFS_OBJECT_NAME);
		//The points are stored offset, so we move the models by half width and height
		Matrix4x4 halfOff = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, 0));

		//TODO: Do not do this.
		//It is 88 megabytes of raw mesh data.
		//Just accept the few extra GameObjects, and instance the prefabs normally.
		//The RoofMatrix should still work for the prefabs, too.

		//create the mesh
		CombineInstance[] combine = new CombineInstance[_buildingBoxes.Count];
		for(int i = 0; i < combine.Length; i++)
		{
			GameObject roofPrefab = roofPrefabs[RandomRange(0, roofPrefabs.Count)];
			Mesh roofMesh = roofPrefab.GetComponent<MeshFilter>().sharedMesh;
			combine[i].mesh = roofMesh;

			BuildingBox buildingBox = _buildingBoxes[i];
			combine[i].transform = halfOff * buildingBox.GetRoofMatrix();
		}
		Mesh combinedMesh = new()
		{
			name = "BuildingsRoofs",
			indexFormat = IndexFormat.UInt32,
		};
		combinedMesh.CombineMeshes(combine);
		combinedMesh.RecalculateNormals();
		combinedMesh.RecalculateBounds();
		combinedMesh.Optimize();

		AssetDatabase.CreateAsset(combinedMesh, BUILDINGS_ROOFS_MESH_CACHE_PATH);

		MeshFilter meshFilter = roofsObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(BUILDINGS_ROOFS_MESH_CACHE_PATH);

		MeshRenderer meshRenderer = roofsObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = roofsMaterial;
	}

	private GameObject CreateChild(string childName, Transform? parent = null)
	{
		GameObject childObject = new(childName);
		childObject.transform.SetParent(parent == null ? transform : parent);
		childObject.transform.localPosition = Vector3.zero;
		childObject.transform.localRotation = Quaternion.identity;
		childObject.transform.localScale = Vector3.one;
		childObject.isStatic = true;
		return childObject;
	}

	#endif //UNITY_EDITOR
}
