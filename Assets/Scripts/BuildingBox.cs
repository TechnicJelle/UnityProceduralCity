#nullable enable
using System.Collections.Generic;
using UnityEngine;

internal class BuildingBox
{
	public readonly Vector2 Pos;
	public readonly Vector2 Surface;
	public readonly float Height;
	public readonly Quaternion Rotation;

	public BuildingBox(Vector2 pos, Vector2 surface, float height, Quaternion rotation)
	{
		Pos = pos;
		Surface = surface;
		Height = height;
		Rotation = rotation;
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
			Surface.x * 0.5f * Vector2.right,
			Surface.y * 0.5f * Vector2.up,
			-Surface.x * 0.5f * Vector2.right,
			-Surface.y * 0.5f * Vector2.up,
		};
		for (int i = 0; i < vertices.Count; i++)
		{
			vertices[i] = Rotation * vertices[i];
			vertices[i] += Pos;
		}

		return new BoundingPolygon(Pos, vertices);
	}
}
