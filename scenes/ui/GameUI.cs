using Game.Manager;
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

	[Export]
	private BuildingManager buildingManager;

	private VBoxContainer buildingSectionContainer;

	private Label resourceLabel;

	public override void _Ready()
	{


		buildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");


		resourceLabel = GetNode<Label>("%ResourceLabel");
		CreateBuildingSections();
		buildingManager.AvailableResourceCountChanged += OnAvailableResourceCountChange;
	}

	public void HideUI()
	{
		Visible = false;
	}

	private void CreateBuildingSections()
	{
		if (buildingResources == null || buildingResources.Length == 0)
		{
			GD.PrintErr("[GameUI] No building resources assigned!");
			return;
		}

		foreach (var buildingResource in buildingResources)
		{
			GD.Print($"[GameUI] Creating section for: {buildingResource}");

			var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();

			if (buildingSection == null)
			{
				GD.PrintErr("[GameUI] Failed to instantiate BuildingSection!");
				continue;
			}

			buildingSectionContainer.AddChild(buildingSection);
			buildingSection.SetBuildingResource(buildingResource);

			GD.Print($"[GameUI] Added section to container");

			buildingSection.SelectButtonPressed += () =>
			{
				GD.Print($"[GameUI] Selected building resource: {buildingResource}");
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}

	private void OnAvailableResourceCountChange(int availableResourceCount)
	{
		resourceLabel.Text = $"{availableResourceCount}";
	}
}