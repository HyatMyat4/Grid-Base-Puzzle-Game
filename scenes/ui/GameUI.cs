using Game.Resources.Building;
using Godot;
using System;

namespace Game.UI;

public partial class GameUI : MarginContainer
{

	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

	[Export]
	private BuildingResource[] buildingResources;

	private HBoxContainer hBoxContainer;


	public override void _Ready()
	{
		hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
		createBuildingButtons();
	}

	private void createBuildingButtons()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingButton = new Button();
			buildingButton.Text = $"Place {buildingResource.Displayname}";
			hBoxContainer.AddChild(buildingButton);

			buildingButton.Pressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}
}
