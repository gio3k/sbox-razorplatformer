using System.Collections.Generic;

namespace Platformer.Core;

public class Polygon
{
	/// <summary>
	/// Points / vertices
	/// </summary>
	public readonly Vector2[] Points;

	/// <summary>
	/// Sides / edges
	/// </summary>
	public readonly List<Vector2> Sides = new();

	private float MeanX { get; set; }
	private float MeanY { get; set; }

	public float MinX { get; private set; }
	public float MinY { get; private set; }
	public float MaxX { get; private set; }
	public float MaxY { get; private set; }

	public Polygon( List<Vector2> points )
	{
		Points = points.ToArray();

		Recalculate();
	}

	private void Recalculate()
	{
		MinX = float.MaxValue;
		MaxX = float.MinValue;
		MinY = float.MaxValue;
		MaxY = float.MinValue;

		Sides.Clear();

		MeanX = 0;
		MeanY = 0;

		for ( var index = 0; index < Points.Length; index++ )
		{
			var point = Points[index];

			if ( point.x < MinX ) MinX = point.x;
			if ( point.x > MaxX ) MaxX = point.x;
			if ( point.y < MinY ) MinY = point.y;
			if ( point.y > MaxY ) MaxY = point.y;

			MeanX += point.x;
			MeanY += point.y;

			var next = index + 1 < Points.Length ? Points[index + 1] : Points[0];
			Sides.Add( new Vector2( point.y - next.y, next.x - point.x ) );
		}

		MeanX /= Points.Length;
		MeanY /= Points.Length;
	}

	/// <summary>
	/// Project polygon onto provided axis
	/// </summary>
	/// <param name="axis">Axis</param>
	/// <param name="offset">Offset to add to each point</param>
	/// <returns>Min and max</returns>
	public Vector2 Project( Vector2 axis, Vector2 offset )
	{
		var min = float.MaxValue;
		var max = float.MinValue;
		foreach ( var point in Points )
		{
			var dp = Vector2.Dot( point + offset, axis );
			min = float.Min( min, dp );
			max = float.Max( max, dp );
		}

		return new Vector2( min, max );
	}

	/// <summary>
	/// Get mins
	/// </summary>
	/// <param name="offset">Offset to add to end product</param>
	/// <returns>Mins</returns>
	public Vector2 GetMins( Vector2 offset )
	{
		return new Vector2( MinX, MinY ) + offset;
	}

	/// <summary>
	/// Get maxs
	/// </summary>
	/// <param name="offset">Offset to add to end product</param>
	/// <returns>Maxs</returns>
	public Vector2 GetMaxs( Vector2 offset )
	{
		return new Vector2( MaxX, MaxY ) + offset;
	}

	/// <summary>
	/// Get center of polygon through arithmetic mean of points
	/// </summary>
	/// <param name="offset">Offset to add to end product</param>
	/// <returns>Center</returns>
	public Vector2 Center( Vector2 offset ) => new Vector2( MeanX, MeanY ) + offset;

	/// <summary>
	/// Add value to all points
	/// </summary>
	/// <param name="value">Value to add</param>
	public void Add( Vector2 value )
	{
		for ( var index = 0; index < Points.Length; index++ ) Points[index] += value;

		Recalculate();
	}

	/// <summary>
	/// Subtract value from all points
	/// </summary>
	/// <param name="value">Value to subtract</param>
	public void Subtract( Vector2 value )
	{
		for ( var index = 0; index < Points.Length; index++ ) Points[index] -= value;

		Recalculate();
	}

	/// <summary>
	/// Multiply all points by value
	/// </summary>
	/// <param name="value">Value to multiply</param>
	public void Multiply( Vector2 value )
	{
		for ( var index = 0; index < Points.Length; index++ ) Points[index] *= value;

		Recalculate();
	}

	/// <summary>
	/// Multiply all points by value
	/// </summary>
	/// <param name="value">Value to multiply</param>
	public void Multiply( float value )
	{
		for ( var index = 0; index < Points.Length; index++ ) Points[index] *= value;

		Recalculate();
	}
}
