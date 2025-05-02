#nullable enable
using System.Collections.Generic;
using UnityEngine;

internal class BuildingBox
{
	private readonly Vector2 _pos;
	private readonly Vector2 _surface;
	private readonly float _height;
	private readonly Quaternion _rotation;

	public BuildingBox(Vector2 pos, Vector2 surface, float height, Quaternion rotation)
	{
		_pos = pos;
		_surface = surface;
		_height = height;
		_rotation = rotation;
	}

	/// Checks if this box overlaps with another box,
	/// taking rotation into account, but not the height.
	/// So it's a 2D check ONLY.
	public bool CheckOverlap(BuildingBox other)
	{
		BoundingPolygon thisPolygon = ToBoundingPolygon();
		BoundingPolygon otherPolygon = other.ToBoundingPolygon();

		return BoundingPolygon.Collides(thisPolygon, otherPolygon);
	}

	public bool CheckOverlap(Road other)
	{
		BoundingPolygon thisPolygon = ToBoundingPolygon();
		BoundingPolygon otherPolygon = other.ToBoundingPolygon();

		return BoundingPolygon.Collides(thisPolygon, otherPolygon);
	}

	private BoundingPolygon ToBoundingPolygon()
	{
		List<Vector2> vertices = new()
		{
			_surface.x * 0.5f * Vector2.right,
			_surface.y * 0.5f * Vector2.up,
			-_surface.x * 0.5f * Vector2.right,
			-_surface.y * 0.5f * Vector2.up,
		};
		for (int i = 0; i < vertices.Count; i++)
		{
			vertices[i] = _rotation * vertices[i];
			vertices[i] += _pos;
		}

		return new BoundingPolygon(_pos, vertices);
	}
	public void Render()
	{
		Gizmos.color = Color.green;
		Matrix4x4 pushMatrix = Gizmos.matrix;
		Gizmos.matrix *= Matrix4x4.TRS(_pos, _rotation, Vector3.one);
		//x&y zero because the matrix already contains the position
		//z is half the height, to put the bottom of the box on the ground
		Gizmos.DrawCube(new Vector3(0, 0, _height / -2), new Vector3(_surface.x, _surface.y, _height));
		Gizmos.matrix = pushMatrix;
	}
}
