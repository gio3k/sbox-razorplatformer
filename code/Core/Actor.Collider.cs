using System;
using System.Collections.Generic;
using System.Linq;

namespace Platformer.Core;

public partial class Actor
{
	public Polygon Collider { get; set; }

	/// <summary>
	/// Use the CSS / Sandbox panel as the collider poly
	/// </summary>
	protected void UsePanelAsCollider()
	{
		if ( !GlobalMatrix.HasValue )
		{
			Collider = new Polygon( new List<Vector2>()
			{
				Box.Rect.TopLeft, Box.Rect.TopRight, Box.Rect.BottomRight, Box.Rect.BottomLeft
			} );
		}
		else
		{
			var matrix = GlobalMatrix.Value.Inverted;
			Collider = new Polygon( new List<Vector2>()
			{
				matrix.Transform( Box.Rect.TopLeft ),
				matrix.Transform( Box.Rect.TopRight ),
				matrix.Transform( Box.Rect.BottomRight ),
				matrix.Transform( Box.Rect.BottomLeft )
			} );
		}

		// Collider poly shouldn't have anything to do with position
		Collider.Subtract( Box.Rect.Position );

		// Multiply by ScaleFromScreen so points are in "UI" coordinates
		Collider.Multiply( ScaleFromScreen );
	}

	/// <summary>
	/// Use a circle as the collider poly
	/// </summary>
	/// <param name="sides">Sides</param>
	/// <param name="radius">Radius</param>
	protected void UseCircleAsCollider( int sides, float radius )
	{
		var delta = 2 * float.Pi / sides;
		var points = new List<Vector2>();
		for ( var i = 0; i < sides; i++ )
		{
			points.Add( new Vector2(
				radius + radius * float.Cos( i * delta ),
				radius + radius * float.Sin( i * delta )
			) );
		}

		Collider = new Polygon( points );
	}

	private bool IntersectBounds( Actor other, Vector2 position )
	{
		var oneMin = Collider.GetMins( position );
		var oneMax = Collider.GetMaxs( position );
		var twoMin = other.Collider.GetMins( other.Position );
		var twoMax = other.Collider.GetMaxs( other.Position );
		return !(twoMin.x > oneMax.x ||
		         twoMax.x < oneMin.x ||
		         twoMin.y > oneMax.y ||
		         twoMax.y < oneMin.y);
	}

	public bool IntersectBounds( Actor other ) => IntersectBounds( other, Position );

	private Vector2? Intersect( Actor other, Vector2 position )
	{
		/*
		 * This code (and most of the collision code) is hard to understand and just not that great.
		 * If you want to use this demo to make a game - rewrite this stuff (maybe just use AABB)
		 */

		bool TestSide( Vector2 side, ref Vector2 normal, ref float magnitude )
		{
			var axis = new Vector2( -side.y, side.x ).Normal;

			var one = Collider.Project( axis, position );
			var two = other.Collider.Project( axis, other.Position );

			if ( one.y < two.x || two.y < one.x ) return true;

			var overlap = MathF.Min( two.y - one.x, one.y - two.x );

			if ( overlap <= 0.0f ) return true;

			if ( overlap < magnitude )
			{
				normal = axis;
				magnitude = overlap;
			}

			return false;
		}

		// Test AABB collision first -> 
		if ( !IntersectBounds( other, position ) )
			return null;

		// Test SAT -> 
		Vector2 oneNormal = Vector2.Zero, twoNormal = Vector2.Zero;
		float oneMagnitude = float.MaxValue, twoMagnitude = float.MaxValue;

		if ( Collider.Sides.Any( side => TestSide( side, ref oneNormal, ref oneMagnitude ) ) )
			return null;

		if ( other.Collider.Sides.Any( side => TestSide( side, ref twoNormal, ref twoMagnitude ) ) )
			return null;

		var result = oneMagnitude < twoMagnitude ? oneNormal * oneMagnitude : -twoNormal * twoMagnitude;
		var direction = other.Collider.Center( other.Position ) - Collider.Center( Position );
		return Vector2.Dot( direction, result ) < 0 ? result : -result;
	}

	public Vector2? Intersect( Actor other ) =>
		Intersect( other, Position );
}
