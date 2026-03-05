using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;
using System;
using System.Numerics;
namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click";

	private readonly StringName ACTION_CANCEL = "cancel";

	private readonly StringName ACTION_RIGHT_CLICK = "right_click";

	[Export]
	private GridManager gridManager;
	[Export]
	private GameUI gameUI;
	[Export]
	private Node2D ySortRoot;
	[Export]
	private PackedScene buildingGhostScene;

	private enum State
	{
		Normal,
		PlacingBuilding
	}

	private int currentResourceCount;
	private int startingResourceCount = 4;

	private int currentlyUsedResourceCount;

	private BuildingResource toPlaceBuildingResource;

	private Vector2I mouseGridCellPosition;

	private BuildingGhost buildingGhost;

	private State currentState;

	private int AvailableResourceCount => startingResourceCount + currentResourceCount - currentlyUsedResourceCount;

	public override void _Ready()
	{
		gridManager.ResourcetilesUpdated += OnResourceTilesUpdated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
	}


	public override void _UnhandledInput(InputEvent evt)
	{

		switch (currentState)
		{
			case State.Normal:
				if (evt.IsActionPressed(ACTION_RIGHT_CLICK))
				{
					DestoryBuildingAtMousePosition();
				}
				break;
			case State.PlacingBuilding:
				if (evt.IsActionPressed(ACTION_CANCEL))
				{
					ClearBuildingGhost();
				}
				else if (
					  toPlaceBuildingResource != null &&
					  evt.IsActionPressed(ACTION_LEFT_CLICK) &&
					  IsBuildingPlaceableAtTile(mouseGridCellPosition)
					)
				{
					GD.Print("Left Clicked");
					PlacedBuildingAtMousePosition();
				}
				break;
			default:
				break;
		}

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!IsInstanceValid(buildingGhost)) return;
		var gridPosition = gridManager.getMouseGridPosition();
		buildingGhost.GlobalPosition = gridPosition * 64;
		if (toPlaceBuildingResource != null && mouseGridCellPosition != gridPosition)
		{
			mouseGridCellPosition = gridPosition;
			updateGridDisplay();

		}
	}

	private void updateGridDisplay()
	{

		if (mouseGridCellPosition == null) return;
		gridManager.ClearHighlightedTiles();
		gridManager.HightlightBuildableTiles();
		if (IsBuildingPlaceableAtTile(mouseGridCellPosition.Value))
		{
			gridManager.HightlightExpandedBuildableTiles(mouseGridCellPosition.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HightlightResourceTiles(mouseGridCellPosition.Value, toPlaceBuildingResource.BuildableRadius);
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}
	}

	private void PlacedBuildingAtMousePosition()
	{
		{
			if (!mouseGridCellPosition.HasValue) return;

			Node2D building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
			ySortRoot.AddChild(building);

			Vector2I gridPosition = mouseGridCellPosition.Value;
			building.GlobalPosition = gridPosition * 64;


			currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
			ClearBuildingGhost();
		}
	}

	private void DestoryBuildingAtMousePosition()
	{

	}

	private void ClearBuildingGhost()
	{
		mouseGridCellPosition = null;
		gridManager.ClearHighlightedTiles();
		if (IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
		}

		buildingGhost = null;
	}

	private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
	{
		return gridManager.IsTilePositionBuildable(tilePosition) &&
			 AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
	}

	private void OnResourceTilesUpdated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{
		// If already exists, remove it first
		if (buildingGhost != null && IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
			buildingGhost = null;
		}

		buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
		ySortRoot.AddChild(buildingGhost);

		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		toPlaceBuildingResource = buildingResource;
		updateGridDisplay();
	}
}
