using Game.Building;
using Game.Component;
using Game.Resources.Building;
using Game.UI;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click";

	private readonly StringName ACTION_CANCEL = "cancel";

	private readonly StringName ACTION_RIGHT_CLICK = "right_click";

	[Export]
	private int startingResourceCount = 4;
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

	private int currentlyUsedResourceCount;

	private BuildingResource toPlaceBuildingResource;

	private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);

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
					  IsBuildingPlaceableAtArea(hoveredGridArea)
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
			var mouseGridPosition = gridManager.getMouseGridPosition();
			var rootCell = hoveredGridArea.Position;
			if (toPlaceBuildingResource != null && rootCell != mouseGridPosition)
			{
				hoveredGridArea.Position = mouseGridPosition;

				UpdateHoverGridArea();

			}

			switch (currentState)
			{
				case State.Normal:
					break;
				case State.PlacingBuilding:

					buildingGhost.GlobalPosition = mouseGridPosition * 64;
					break;
			}
		}
	}

	private void updateGridDisplay()
	{
		gridManager.ClearHighlightedTiles();
		gridManager.HightlightBuildableTiles();
		if (IsBuildingPlaceableAtArea(hoveredGridArea))
		{
			gridManager.HightlightExpandedBuildableTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);
			gridManager.HightlightResourceTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);
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

			Vector2I gridPosition = hoveredGridArea.Position;
			building.GlobalPosition = gridPosition * 64;


			currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
			ChangeState(State.Normal);
		}
	}

	private void DestoryBuildingAtMousePosition()
	{

		var rootCell = hoveredGridArea.Position;
		var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>().FirstOrDefault((buildingComponent) =>
		{
			return buildingComponent.BuildingResource.IsDeletable && buildingComponent.IsTileInBuildingArea(rootCell);
		});
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

	private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
	{

		var tilesArea = GetTiledPositionsInTileArea(tileArea);
		var allTilesBuildable = tilesArea.All((tilePosition) => gridManager.IsTilePositionBuildable(tilePosition));
		return allTilesBuildable && AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
	}

	private List<Vector2I> GetTiledPositionsInTileArea(Rect2I tileArea)
	{
		var result = new List<Vector2I>();
		for (int x = tileArea.Position.X; x < tileArea.End.X; x++)
		{
			for (int y = tileArea.Position.Y; y < tileArea.End.Y; y++)
			{
				result.Add(new Vector2I(x, y));
			}
		}

		return result;
	}

	private void UpdateHoverGridArea()
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
		hoveredGridArea.Size = buildingResource.Dimensions;
		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		toPlaceBuildingResource = buildingResource;
		updateGridDisplay();
	}
}
