#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class BuildingBox
{
#if UNITY_EDITOR
	private readonly Vector2 _pos;
	private readonly Vector2 _surface;
	private readonly float _height;
	private readonly Quaternion _rotation;

	private readonly BoundingPolygon _polygon;

	public BuildingBox(Vector2 pos, Vector2 surface, float height, Quaternion rotation)
	{
		_pos = pos;
		_surface = surface;
		_height = height;
		_rotation = rotation;

		List<Vector2> corners = new()
		{
			new Vector2(surface.x, surface.y) * 0.5f,
			new Vector2(surface.x, -surface.y) * 0.5f,
			new Vector2(-surface.x, -surface.y) * 0.5f,
			new Vector2(-surface.x, surface.y) * 0.5f,
		};
		for(int i = 0; i < corners.Count; i++)
		{
			corners[i] = _rotation * corners[i];
			corners[i] += _pos;
		}

		_polygon = new BoundingPolygon(corners);
	}

	public Matrix4x4 GetRoofMatrix() => Matrix4x4.TRS(
		new Vector3(_pos.x, _pos.y, -_height),
		_rotation,
		new Vector3(_surface.x, _surface.y, -_height) * 50f //TODO: Figure out why this is needed, and specifically why 50???
	);

	public bool IntersectsAPlaceToAvoid(RoadGenerator roadGenerator)
	{
		foreach(Vector2 corner in _polygon.Corners)
		{
			if (roadGenerator.IsPlaceToAvoid(corner))
			{
				return true;
			}
		}
		return false;
	}

	public bool IntersectsAPlaceToBridgeOver(RoadGenerator roadGenerator)
	{
		foreach(Vector2 corner in _polygon.Corners)
		{
			if (roadGenerator.IsPlaceToBridgeOver(corner))
			{
				return true;
			}
		}
		return false;
	}

	/// Checks if this box overlaps with another box,
	/// taking rotation into account, but not the height.
	/// So it's a 2D check ONLY.
	public bool CheckOverlap(BuildingBox other)
	{
		return BoundingPolygon.Collides(_polygon, other._polygon);
	}

	public bool CheckOverlap(Road other)
	{
		return BoundingPolygon.Collides(_polygon, other.Polygon);
	}

	public void Render()
	{
		Gizmos.color = Color.green;
		Matrix4x4 pushMatrix = Gizmos.matrix;
		Gizmos.matrix *= Matrix4x4.TRS(_pos, _rotation, Vector3.one);
		//x&y zero because the matrix already contains the position
		//z is half the height, to put the bottom of the box on the ground
		//minus a bit to avoid z-fighting with the building meshes
		Gizmos.DrawCube(
			new Vector3(0, 0, _height / -2 + RoadGenerator.ANTI_Z),
			new Vector3(_surface.x - RoadGenerator.ANTI_Z, _surface.y - RoadGenerator.ANTI_Z, _height - RoadGenerator.ANTI_Z)
		);
		Gizmos.matrix = pushMatrix;
	}

	public Mesh ToMesh()
	{
		return new Mesh
		{
			vertices = new Vector3[]
			{
				//bottom plane
				new(_polygon.Corners[0].x, _polygon.Corners[0].y, 0),
				new(_polygon.Corners[1].x, _polygon.Corners[1].y, 0),
				new(_polygon.Corners[2].x, _polygon.Corners[2].y, 0),
				new(_polygon.Corners[3].x, _polygon.Corners[3].y, 0),
				//top plane
				new(_polygon.Corners[0].x, _polygon.Corners[0].y, -_height),
				new(_polygon.Corners[1].x, _polygon.Corners[1].y, -_height),
				new(_polygon.Corners[2].x, _polygon.Corners[2].y, -_height),
				new(_polygon.Corners[3].x, _polygon.Corners[3].y, -_height),
			},
			triangles = new[]
			{
				//north plane
				0, 1, 5,
				0, 5, 4,
				//south plane
				2, 3, 7,
				2, 7, 6,
				//east plane
				1, 2, 6,
				1, 6, 5,
				//west plane
				3, 0, 4,
				3, 4, 7,
			},
			uv = new Vector2[]
			{
				//bottom plane
				new(0, 0),
				new(1, 0),
				new(0, 0),
				new(1, 0),
				//top plane
				new(0, 1),
				new(1, 1),
				new(0, 1),
				new(1, 1),
			},
		};
	}
#endif //UNITY_EDITOR
}
