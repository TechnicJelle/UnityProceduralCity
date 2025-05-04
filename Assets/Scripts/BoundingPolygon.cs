#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Source: https://textbooks.cs.ksu.edu/cis580/04-collisions/04-separating-axis-theorem/
//  Licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International Licence.

/// <summary>
/// A struct representing a convex bounding polygon
/// </summary>
public struct BoundingPolygon
{
	/// <summary>
	/// The corners of the bounding polygon,
	/// in relation to (0,0)
	/// </summary>
	public List<Vector2> Corners;

	/// <summary>
	/// The normals of each corner of this bounding polygon
	/// </summary>
	public Vector2[] Normals;

	/// <summary>
	/// Constructs a new arbitrary convex bounding polygon
	/// </summary>
	/// <remarks>
	/// In order to be used with Separating Axis Theorem,
	/// the bounding polygon MUST be convex.
	/// </remarks>
	/// <param name="corners">The corners of the polygon</param>
	public BoundingPolygon(List<Vector2> corners)
	{
		// Store the center and corners
		Corners = corners;
		// Determine the normal vectors for the sides of the shape
		// We can use a hashset to avoid duplicating normals
		HashSet<Vector2> normals = new();
		// Calculate the first edge by subtracting the first from the last corner
		Vector2 edge = Corners[^1] - Corners[0];
		// Then determine a perpendicular vector
		Vector2 perpendicular = new(edge.y, -edge.x);
		// Then normalize
		perpendicular.Normalize();
		// Add the normal to the list
		normals.Add(perpendicular);
		// Repeat for the remaining edges
		for(int i = 1; i < Corners.Count; i++)
		{
			edge = Corners[i] - Corners[i - 1];
			perpendicular = new Vector2(edge.y, -edge.x);
			perpendicular.Normalize();
			normals.Add(perpendicular);
		}
		// Store the normals
		Normals = normals.ToArray();
	}

	public void Render()
	{
		for(int i = 0; i < Corners.Count; i++)
		{
			Vector2 start = Corners[i];
			Vector2 end = Corners[(i + 1) % Corners.Count];
			Gizmos.DrawLine(start, end);
		}
	}

	/// <summary>
	/// Detects a collision between two convex polygons
	/// using the Separating Axis Theorem
	/// </summary>
	/// <param name="p1">the first polygon</param>
	/// <param name="p2">the second polygon</param>
	/// <returns>true when colliding, false otherwise</returns>
	public static bool Collides(BoundingPolygon p1, BoundingPolygon p2)
	{
		// Check the first polygon's normals
		foreach(Vector2 normal in p1.Normals)
		{
			// Determine the minimum and maximum projection
			// for both polygons
			MinMax mm1 = FindMaxMinProjection(p1, normal);
			MinMax mm2 = FindMaxMinProjection(p2, normal);
			// Test for separation (as soon as we find a separating axis,
			// we know there is no possibility of collision, so we can
			// exit early)
			if (mm1.Max < mm2.Min || mm2.Max < mm1.Min) return false;
		}
		// Repeat for the second polygon's normals
		foreach(Vector2 normal in p2.Normals)
		{
			// Determine the minimum and maximum projection
			// for both polygons
			MinMax mm1 = FindMaxMinProjection(p1, normal);
			MinMax mm2 = FindMaxMinProjection(p2, normal);
			// Test for separation (as soon as we find a separating axis,
			// we know there is no possibility of collision, so we can
			// exit early)
			if (mm1.Max < mm2.Min || mm2.Max < mm1.Min) return false;
		}
		// If we reach this point, no separating axis was found
		// and the two polygons are colliding
		return true;
	}

	/// <summary>
	/// An object representing minimum and maximum bounds
	/// </summary>
	private struct MinMax
	{
		/// <summary>
		/// The minimum bound
		/// </summary>
		public float Min;

		/// <summary>
		/// The maximum bound
		/// </summary>
		public float Max;

		/// <summary>
		/// Constructs a new MinMax pair
		/// </summary>
		public MinMax(float min, float max)
		{
			Min = min;
			Max = max;
		}
	}

	private static MinMax FindMaxMinProjection(BoundingPolygon poly, Vector2 axis)
	{
		float projection = Vector2.Dot(poly.Corners[0], axis);
		float max = projection;
		float min = projection;
		for(int i = 1; i < poly.Corners.Count; i++)
		{
			projection = Vector2.Dot(poly.Corners[i], axis);
			max = max > projection ? max : projection;
			min = min < projection ? min : projection;
		}
		return new MinMax(min, max);
	}
}
