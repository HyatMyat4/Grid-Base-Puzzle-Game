using Game.Resources.Building;
using Godot;
using System;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{

	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

	[Export]
	private BuildingResource[] buildingResources;

	[Export]
	private PackedScene buildingSectionScene;

	private VBoxContainer buildingSectionContainer;


	public override void _Ready()
	{
		buildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
		CreateBuildingSections();
	}

	private void CreateBuildingSections()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();
			buildingSectionContainer.AddChild(buildingSection);
			buildingSection.SetBuildingResource(buildingResource);

			buildingSection.SelectButtonPressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}
}
