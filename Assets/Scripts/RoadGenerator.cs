using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoadGenerator : MonoBehaviour
{

#region Constants

	[SerializeField]
	public float width = 1000.0f;

	[SerializeField]
	public float height = 1000.0f;

	[SerializeField] [Range(0.0f, 0.5f)]
	private float middleSpawnFactor = 0.5f;

	[SerializeField]
	private int initialStartPoints = 4;

	[SerializeField]
	public float stepSize = 50.0f;

	[SerializeField] [Range(0.0f, Mathf.PI)]
	public float maxRotationAmountRadians = 0.4f;

	[SerializeField] [Range(0.0f, 1.0f)]
	public float newRoadChance = 0.7f;

	[SerializeField]
	private GameObject roadPrefab;

#endregion

#region Private Fields

	public readonly List<Point> Points = new();
	private bool _completed = false;

#endregion

	private void Start()
	{
		for(int i = 0; i < initialStartPoints; i++)
		{
			float x = Random.Range(width * (0.5f - middleSpawnFactor), width * (0.5f + middleSpawnFactor));
			float y = Random.Range(height * (0.5f - middleSpawnFactor), height * (0.5f + middleSpawnFactor));
			Points.Add(new Point(this, x, y));
		}
	}

	private void OnDrawGizmos()
	{
		//Apply the road generator transform to the gizmos
		Gizmos.matrix = transform.localToWorldMatrix;

		//Draw the middle spawn area
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(transform.position, new Vector3(width * 2 * middleSpawnFactor, height * 2 * middleSpawnFactor, 0));

		//Draw the bounding box
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));

		//The points are stored offset, so we move the drawing by half width and height
		Matrix4x4 translationMatrix = Matrix4x4.Translate(new Vector3(-width * 0.5f, -height * 0.5f, 0));
		Gizmos.matrix *= translationMatrix;

		// Draw the points
		foreach(Point p in Points)
		{
			p.Render();
		}

		// Reset the matrix to identity to avoid affecting other gizmos
		Gizmos.matrix = Matrix4x4.identity;
	}

	private void Update()
	{
		// if(Input.GetMouseButtonDown(0))
		{
			TakeAStep();
		}
	}

	private void TakeAStep()
	{
		if (!_completed)
		{
			for(int i = 0; i < Points.Count; i++)
			{
				Point p = Points[i];
				if (p.Head)
				{
					Points.AddRange(p.Step());
				}
			}

			_completed = CheckDone();
		}
	}

	private void FinishUp()
	{
		//make directional links double-sided
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

	private bool CheckDone()
	{
		foreach(Point p in Points)
		{
			if (p.Head)
			{
				return false;
			}
		}

		FinishUp();

		return true;
	}
}
