using Game.Component;
using Godot;

namespace Game.Autoload;

public partial class GameEvent : Node
{

	public static GameEvent Instance { get; private set; }


	[Signal]
	public delegate void BuildingPlacedEventHandler(BuildingComponent buildingComponent);

	[Signal]
	public delegate void BuildingDestroyedEventHandler(BuildingComponent buildingComponent);

	public override void _Notification(int what)
	{
		base._Notification(what);

		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

	public static void EmitBuildingDestroyed(BuildingComponent buildingComponent)
	{
		Instance.EmitSignal(SignalName.BuildingDestroyed, buildingComponent);
	}

	public static void EmitBuildingPlaced(BuildingComponent buildingComponent)
	{
		Instance.EmitSignal(SignalName.BuildingPlaced, buildingComponent);
	}

}
