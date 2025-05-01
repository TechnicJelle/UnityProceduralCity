#nullable enable
using Unity.VisualScripting;
using UnityEngine;

public class Road
{
	public readonly Point P1;
	public readonly Point P2;

	public Road(Point p1, Point p2)
	{
		P1 = p1;
		P2 = p2;
	}

	public Vector2? Intersect(Road other)
	{
		return Intersect3(P1.Pos, P2.Pos, other.P1.Pos, other.P2.Pos);
	}

	public void Render()
	{
		Gizmos.color = Color.yellow.WithAlpha(0.3f);
		Gizmos.DrawLine(P1.Pos, P2.Pos);
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
}
