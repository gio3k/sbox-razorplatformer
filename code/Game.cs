using Platformer.Core;
using Platformer.Maps;

namespace Sandbox;

public class PlatformerGame : GameManager
{
	public PlatformerGame()
	{
		if ( Game.IsServer ) return;

		Static.SetMap<Stage00>();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Static.CurrentMap?.Simulate();
	}
}
