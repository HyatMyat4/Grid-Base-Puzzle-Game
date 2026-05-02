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

	[Signal]
	public delegate void AvailableResourceCountChangedEventHandler(int availableResourceCount);


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

	private Godot.Vector2 buildingGhostDimensions;

	private State currentState;

	private int AvailableResourceCount => startingResourceCount + currentResourceCount - currentlyUsedResourceCount;




	public override void _Ready()
	{
		gridManager.ResourcetilesUpdated += OnResourceTilesUpdated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
		CallDeferred(nameof(EmitInitialResource));

	}

	private void EmitInitialResource()
	{
		EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
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

					PlacedBuildingAtMousePosition();
				}
				break;
			default:
				break;
		}

	}

	public void SetStartingResourceCount(int count)
	{
		startingResourceCount = count;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (buildingGhost != null)
		{

			Vector2I mouseGridPosition = Vector2I.Zero;

			switch (currentState)
			{
				case State.Normal:
					mouseGridPosition = gridManager.getMouseGridPosition();
					break;
				case State.PlacingBuilding:
					mouseGridPosition = gridManager.GetMouseGridPosistionWithDimesionOffset(buildingGhostDimensions);
					buildingGhost.GlobalPosition = mouseGridPosition * 64;
					break;
			}


			var rootCell = hoveredGridArea.Position;
			if (toPlaceBuildingResource != null && rootCell != mouseGridPosition)
			{
				hoveredGridArea.Position = mouseGridPosition;

				UpdateHoverGridArea();

			}


		}
	}

	private void updateGridDisplay()
	{
		gridManager.ClearHighlightedTiles();

		if (toPlaceBuildingResource.IsAttackBuilding())
		{
			gridManager.HighlightGoblinOccupiedTiles();
			gridManager.HightlightBuildableTiles();
		}
		else
		{
			gridManager.HightlightBuildableTiles();
			gridManager.HighlightGoblinOccupiedTiles();
		}



		if (IsBuildingPlaceableAtArea(hoveredGridArea))
		{

			if (toPlaceBuildingResource.IsAttackBuilding())
			{
				gridManager.HightlightAttackTiles(hoveredGridArea, toPlaceBuildingResource.AttackRadius);
			}
			else
			{
				gridManager.HightlightExpandedBuildableTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);
			}

			gridManager.HightlightResourceTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);


			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}

		buildingGhost.DoHoverAnimation();
	}

	private void PlacedBuildingAtMousePosition()
	{
		{

			Node2D building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
			ySortRoot.AddChild(building);

			Vector2I gridPosition = hoveredGridArea.Position;
			building.GlobalPosition = gridPosition * 64;
			building.GetFirstNodeOfType<BuildingAnimatorComponent>()?.PlayInAnimation();

			currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
			ChangeState(State.Normal);

			EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
		}
	}

	private void DestoryBuildingAtMousePosition()
	{

		var rootCell = hoveredGridArea.Position;
		var buildingComponent = BuildingComponent.GetVaildBuildingComponents(this).FirstOrDefault((buildingComponent) =>
		{
			return buildingComponent.BuildingResource.IsDeletable && buildingComponent.IsTileInBuildingArea(rootCell);
		});


		if (buildingComponent == null) return;

		currentResourceCount += buildingComponent.BuildingResource.ResourceCost;
		buildingComponent.Distory();

		EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
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

		var isattacktiles = toPlaceBuildingResource.IsAttackBuilding();
		var allTilesBuildable = gridManager.IsTileAreaBuildable(tileArea, isattacktiles);

		bool hasEnoughResources = AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;

		bool result = allTilesBuildable && hasEnoughResources;

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
		currentlyUsedResourceCount = resourceCount;
		EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
	}

	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{

		ChangeState(State.PlacingBuilding);
		hoveredGridArea.Size = buildingResource.Dimensions;
		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddSpriteNode(buildingSprite);
		buildingGhost.SetDimensions(buildingResource.Dimensions);
		buildingGhostDimensions = buildingResource.Dimensions;
		toPlaceBuildingResource = buildingResource;
		updateGridDisplay();
	}
}
