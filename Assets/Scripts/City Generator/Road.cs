#nullable enable
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Road
{
#if UNITY_EDITOR
	private const int PLACE_CHECKING_SAMPLES = 10;

	private readonly RoadGenerator _roadGenerator;

	public readonly Point P1;
	public readonly Point P2;

	public readonly bool Bridge = false;

	private readonly BoundingPolygon _polygon;
	public BoundingPolygon Polygon => _polygon;

	public Road(RoadGenerator roadGenerator, Point p1, Point p2)
	{
		_roadGenerator = roadGenerator;

		P1 = p1;
		P2 = p2;

		//check along multiple positions along the road whether we are a bridge
		for(int i = 0; i < PLACE_CHECKING_SAMPLES; i++)
		{
			float t = i / (float)PLACE_CHECKING_SAMPLES;
			Vector2 pos = Vector2.Lerp(P1.Pos, P2.Pos, t);
			if (_roadGenerator.IsPlaceToBridgeOver(pos))
			{
				Bridge = true;
				break;
			}
		}

		Vector2 ray = P2.Pos - P1.Pos;
		Vector2 direction = ray.normalized;
		Vector2 perpendicular = new(-direction.y, direction.x);
		List<Vector2> corners = new()
		{
			P1.Pos - perpendicular * _roadGenerator.meshRadius,
			P1.Pos + perpendicular * _roadGenerator.meshRadius,
			P2.Pos + perpendicular * _roadGenerator.meshRadius,
			P2.Pos - perpendicular * _roadGenerator.meshRadius,
		};
		_polygon = new BoundingPolygon(corners);
	}

	public float GetMagnitude() => Vector2.Distance(P1.Pos, P2.Pos);

	public Vector2 GetMiddlePos() => (P1.Pos + P2.Pos) / 2;

	public Vector2 GetDirection() => (P2.Pos - P1.Pos).normalized;

	///checks along multiple positions along the road whether we're intersecting a place to avoid
	public bool IntersectsAPlaceToAvoid()
	{
		for(int i = 0; i < PLACE_CHECKING_SAMPLES; i++)
		{
			float t = i / (float)PLACE_CHECKING_SAMPLES;
			Vector2 pos = Vector2.Lerp(P1.Pos, P2.Pos, t);
			if (_roadGenerator.IsPlaceToAvoid(pos))
			{
				return true;
			}
		}
		return false;
	}

	public Vector2? Intersect(Road other)
	{
		return Intersect3(P1.Pos, P2.Pos, other.P1.Pos, other.P2.Pos);
	}

	public void Render()
	{
		Gizmos.color = Bridge
			? Color.grey
			: (GetMagnitude() < _roadGenerator.minRoadLengthForBuilding
				? Color.red
				: Color.yellow).WithAlpha(0.3f);
		Gizmos.DrawLine(P1.Pos, P2.Pos);
	}

	public Mesh ToMesh()
	{
		const float epsilon = 0.01f;
		return new Mesh
		{
			vertices = new Vector3[]
			{
				new(_polygon.Corners[0].x, _polygon.Corners[0].y, epsilon * 0),
				new(_polygon.Corners[1].x, _polygon.Corners[1].y, epsilon * 1),
				new(_polygon.Corners[2].x, _polygon.Corners[2].y, epsilon * 2),
				new(_polygon.Corners[3].x, _polygon.Corners[3].y, epsilon * 3),
			},
			triangles = new[]
			{
				0, 1, 2,
				0, 2, 3,
			},
			uv = new Vector2[]
			{
				new(0, 0),
				new(1, 0),
				new(1, GetMagnitude() * _roadGenerator.textureStretching),
				new(0, GetMagnitude() * _roadGenerator.textureStretching),
			},
		};
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
#endif //UNITY_EDITOR
}
