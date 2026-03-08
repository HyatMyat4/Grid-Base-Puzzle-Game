using Game.Building;
using Game.Component;
using Game.Resources.Building;
using Game.UI;
using Godot;
using System;
using System.Linq;
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
					ChangeState(State.Normal);
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

		if (buildingGhost != null)
		{
			var gridPosition = gridManager.getMouseGridPosition();

			if (toPlaceBuildingResource != null && mouseGridCellPosition != gridPosition)
			{
				mouseGridCellPosition = gridPosition;

				UpdateHoverGridCell();

			}

			switch (currentState)
			{
				case State.Normal:
					break;
				case State.PlacingBuilding:

					buildingGhost.GlobalPosition = gridPosition * 64;
					break;
			}
		}
	}

	private void updateGridDisplay()
	{
		gridManager.ClearHighlightedTiles();
		gridManager.HightlightBuildableTiles();
		if (IsBuildingPlaceableAtTile(mouseGridCellPosition))
		{
			gridManager.HightlightExpandedBuildableTiles(mouseGridCellPosition, toPlaceBuildingResource.BuildableRadius);
			gridManager.HightlightResourceTiles(mouseGridCellPosition, toPlaceBuildingResource.BuildableRadius);
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


			Node2D building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
			ySortRoot.AddChild(building);

			Vector2I gridPosition = mouseGridCellPosition;
			building.GlobalPosition = gridPosition * 64;


			currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
			ChangeState(State.Normal);
		}
	}

	private void DestoryBuildingAtMousePosition()
	{
		var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>().FirstOrDefault((buildingComponent) => buildingComponent.GetGridCellPosition() == mouseGridCellPosition);
		if (buildingComponent == null) return;

		currentResourceCount += buildingComponent.BuildingResource.ResourceCost;
		buildingComponent.Distory();
	}

	private void ClearBuildingGhost()
	{
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

	private void UpdateHoverGridCell()
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				updateGridDisplay();
				break;
		}
	}

	private void ChangeState(State toState)
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				ClearBuildingGhost();
				toPlaceBuildingResource = null;
				break;
		}

		currentState = toState;

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
				ySortRoot.AddChild(buildingGhost);
				break;
		}

	}

	private void OnResourceTilesUpdated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{

		ChangeState(State.PlacingBuilding);
		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		toPlaceBuildingResource = buildingResource;
		updateGridDisplay();
	}
}
