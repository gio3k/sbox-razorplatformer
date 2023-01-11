namespace Platformer.Core;

public partial class Actor
{
	protected bool IsStaticObject;

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		switch ( name )
		{
			case "static":
				Mass = 0;
				Restitution = 0;
				IsStaticObject = true;
				break;
		}
	}
}
