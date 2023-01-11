using Sandbox;
using Sandbox.UI;

namespace Platformer.Core;

public partial class Actor : Panel
{
	private bool _initialized;

	private struct ActorRestoreData
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public Actor Ground;
		public Vector2 GroundNormal;
	}

	private ActorRestoreData _simulate = new();

	public Vector2 Position = Vector2.Zero;
	public Vector2 Velocity = Vector2.Zero;
	protected Actor Ground;
	protected Vector2 GroundNormal;

	protected bool GravityAffected = true;

	protected float Restitution = 0.5f;

	private float _mass;
	private float _inverseMass;

	protected float Mass
	{
		get => _mass;
		set
		{
			_mass = value;
			_inverseMass = value == 0 ? 0 : 1 / _mass;
		}
	}

	public bool IsGrounded
	{
		get => Ground != null;
		set => Ground = value ? Ground : null;
	}

	public Actor()
	{
		Mass = 15; // whatever
		Log.Info( $"New actor {this}, of type {GetType().Name}" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !_initialized )
		{
			// freaky check for Panel readiness
			if ( Box.Rect is { Width: 0, Height: 0 } ) return;

			if ( Position == Vector2.Zero )
				Position = Box.Rect.Position * ScaleFromScreen;

			CreateCollider();

			SaveSimulateData();

			_initialized = true;

			return;
		}

		InternalFrameSimulate();

		Style.Left = Length.Pixels( Position.x );
		Style.Top = Length.Pixels( Position.y );
	}

	protected void SaveSimulateData()
	{
		_simulate.Position = Position;
		_simulate.Velocity = Velocity;
		_simulate.Ground = Ground;
		_simulate.GroundNormal = GroundNormal;
	}

	protected void RestoreSimulateData()
	{
		Position = _simulate.Position;
		Velocity = _simulate.Velocity;
		Ground = _simulate.Ground;
		GroundNormal = _simulate.GroundNormal;
	}

	/// <summary>
	/// Process player movement in provided amount of steps
	/// </summary>
	/// <param name="steps">Steps</param>
	private void ProcessMovement( int steps )
	{
		// steps aren't toooooo important for the way we use collision now (they were before!)
		// but we'll keep them in anyways. just in case...
		var velocityPerStep = Velocity / steps;
		for ( var iteration = 0; iteration < steps; iteration++ )
		{
			var goal = Position + velocityPerStep * Time.Delta;

			// Check if we can move to the goal
			foreach ( var actor in Static.CurrentMap.Actors )
			{
				if ( actor == this ) continue;

				if ( actor.Collider == null ) continue;

				var result = Intersect( actor, goal );
				if ( result == null ) continue;

				ComputeCollision( actor, result.Value );
				actor.ComputeCollision( this, -result.Value );
			}

			Position += velocityPerStep * Time.Delta;
		}
	}

	/// <summary>
	/// Called by the <see cref="Map"/>
	/// </summary>
	public void ProcessSimulate()
	{
		if ( !_initialized ) return;
		RestoreSimulateData();
		CreateCollider(); // this isn't great - ideally the collider should only update when needed but whatever
		Simulate();
		IsGrounded = false;
		if ( !IsStaticObject ) ProcessMovement( 3 );
		SaveSimulateData();
	}

	private void InternalFrameSimulate()
	{
		FrameSimulate();
		if ( !IsStaticObject ) ProcessMovement( 1 );
	}

	protected virtual void CreateCollider() => UsePanelAsCollider();

	protected virtual void ComputeCollision( Actor actor, Vector2 displacement )
	{
		var displacementNormal = displacement.Normal;

		if ( float.IsNaN( displacementNormal.x ) || float.IsNaN( displacementNormal.y ) )
			return;

		// Check if we should use this actor as the ground
		// abs(displacementNormal.y) == 1 means we've hit a fully vertical wall, 0 means fully horizontal
		if ( float.Abs( displacementNormal.y ) > 0.5f )
		{
			Ground = actor;
			GroundNormal = displacement.Normal;
		}

		// Adapted from https://gamedevelopment.tutsplus.com/tutorials/how-to-create-a-custom-2d-physics-engine-the-basics-and-impulse-resolution--gamedev-6331
		var relativeVelocity = Velocity - actor.Velocity;
		var velAlongNormal = Vector2.Dot( relativeVelocity, displacementNormal );
		if ( velAlongNormal > 0 ) return;

		// Calc impulse scalar
		var scalar = -(1 + float.Min( Restitution, actor.Restitution )) * velAlongNormal;
		scalar /= actor._inverseMass + _inverseMass;

		// Scale for incline
		var inclineMultiplier = float.Abs( Vector2.Dot( displacementNormal, Vector2.Up ) );
		if ( inclineMultiplier != 1.0f )
			inclineMultiplier *= 0.044f; // this seems to be a nice constant for this!

		// Apply impulse to actors
		actor.Velocity -= inclineMultiplier * (actor._inverseMass * (scalar * displacementNormal));
		Velocity += inclineMultiplier * (_inverseMass * (scalar * displacementNormal));

		// Correction
		const float slop = 0.01f; // usually 0.01 to 0.1
		var magnitude = float.Max( displacement.Length - slop, 0.0f );
		var correction = displacementNormal * (0.6f * (magnitude / (actor._inverseMass + _inverseMass)));
		actor.Position -= correction * actor._inverseMass;
		Position += correction * _inverseMass;
	}

	protected virtual void Simulate()
	{
		if ( IsStaticObject || Velocity.Length < 0.3f ) Velocity = Vector2.Zero;

		Velocity *= 0.99f;

		if ( IsGrounded ) Velocity.x *= 0.97f;

		if ( GravityAffected ) Velocity.y += 15.7f;
	}

	/// <summary>
	/// Called when rendering the actor
	/// </summary>
	protected virtual void FrameSimulate() { }
}
