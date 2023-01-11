using Sandbox;

namespace Platformer.Core;

public static class Static
{
	public static Map CurrentMap { get; private set; }

	/// <summary>
	/// Set current map
	/// </summary>
	/// <typeparam name="T">New map type</typeparam>
	public static void SetMap<T>() where T : Map, new()
	{
		CurrentMap?.Delete( false );
		CurrentMap = null;

		CurrentMap = new T();

		Game.RootPanel = CurrentMap;
	}

	/// <summary>
	/// Set current map by type description
	/// </summary>
	/// <param name="typeDescription">Type description</param>
	private static void SetMap( TypeDescription typeDescription )
	{
		if ( typeDescription == null ) return;

		if ( !typeDescription.TargetType.IsSubclassOf( typeof(Map) ) )
		{
			Log.Error( $"{typeDescription.Name} is not a subclass of Map!" );
			return;
		}

		CurrentMap?.Delete( false );
		CurrentMap = null;

		CurrentMap = typeDescription.Create<Map>();

		Game.RootPanel = CurrentMap;
	}

	/// <summary>
	/// Set current map by name
	/// </summary>
	/// <param name="name">Name of map type</param>
	public static void SetMap( string name )
	{
		if ( name == null ) return;

		SetMap( TypeLibrary.GetType( name ) );
	}

	/// <summary>
	/// Reset current map
	/// </summary>
	public static void ResetMap()
	{
		// todo: throw error
		if ( CurrentMap == null ) return;

		SetMap( TypeLibrary.GetType( CurrentMap.GetType() ) );
	}
}
