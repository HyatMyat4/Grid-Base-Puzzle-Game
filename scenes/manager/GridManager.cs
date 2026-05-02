using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{

	private const string IS_BUILDABLE = "is_buildable";

	private const string IS_WOOD = "is_wood";

	private const string IS_IGNORE = "is_ignored";


	[Signal]
	public delegate void ResourcetilesUpdatedEventHandler(int collectedTiles);
	[Signal]
	public delegate void GridStateUpdatedEventHandler();

	private HashSet<Vector2I> vaildBuildableTiles = new HashSet<Vector2I>();

	private HashSet<Vector2I> validBuildableAttackTiles = new HashSet<Vector2I>();

	private HashSet<Vector2I> collectedResourceTiles = new HashSet<Vector2I>();

	private HashSet<Vector2I> allTilesInBuildingRadius = new HashSet<Vector2I>();

	private HashSet<Vector2I> occupiedTiles = new HashSet<Vector2I>();

	private HashSet<Vector2I> goblinOccupiedTiles = new();

	private HashSet<Vector2I> attackTiles = new();

	[Export]
	private TileMapLayer hightlightTileMapLayer;

	[Export]
	private TileMapLayer baseTerrainTileMapLayer;

	private List<TileMapLayer> allTilemapLayers = new();

	private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		GameEvent.Instance.Connect(GameEvent.SignalName.BuildingPlaced, Callable.From<BuildingComponent>(OnBuildingPlaced));
		GameEvent.Instance.Connect(GameEvent.SignalName.BuildingDestroyed, Callable.From<BuildingComponent>(OnBuildingDestoryed));
		allTilemapLayers = GetAllTilemaplayers(baseTerrainTileMapLayer);
		MapTileMapLayersToElevationLayers();

	}
	public (TileMapLayer, bool) GetTileCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null || (bool)customData.GetCustomData(IS_IGNORE))
			{
				continue;
			}
			var value = customData.GetCustomData(dataName);
			return (layer, (bool)value);
		}

		return (null, false);
	}



	public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
	{
		return allTilesInBuildingRadius.Contains(tilePosition);
	}

	public bool IsTileAreaBuildable(Rect2I tileArea, bool isAttackTiles = false)
	{
		var tiles = tileArea.ToTiles();

		if (tiles.Count == 0) return false;

		(TileMapLayer firstTileMapLayer, _) = GetTileCustomData(tiles[0], IS_BUILDABLE);
		var targetElevationLayer = firstTileMapLayer != null ? tileMapLayerToElevationLayer[firstTileMapLayer] : null;

		var tileSetToCheck = GetBuildableTileSet(isAttackTiles);
		if (isAttackTiles)
		{
			tileSetToCheck = tileSetToCheck.Except(occupiedTiles).ToHashSet();
		}

		return tiles.All((tilePosition) =>
		{
			(TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(tilePosition, IS_BUILDABLE);
			var elevationLayer = tileMapLayer != null ? tileMapLayerToElevationLayer[tileMapLayer] : null;
			return isBuildable && tileSetToCheck.Contains(tilePosition) && elevationLayer == targetElevationLayer;
		});
	}



	public void HighlightGoblinOccupiedTiles()
	{

		var atlasCoords = new Vector2I(2, 0);
		foreach (var tilePosition in goblinOccupiedTiles)
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HightlightBuildableTiles(bool isAttackTiles = false)
	{
		foreach (var tilePosition in GetBuildableTileSet(isAttackTiles))
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HightlightExpandedBuildableTiles(Rect2I tiledArea, int radius)
	{

		var validTiles = GetVaildTilesInRadius(tiledArea, radius).ToHashSet();
		var expandedBuildableTiles = validTiles.Except(vaildBuildableTiles).Except(occupiedTiles).Except(goblinOccupiedTiles);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in expandedBuildableTiles)
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HightlightAttackTiles(Rect2I tiledArea, int radius)
	{

		var buildingAreaTiles = tiledArea.ToTiles().ToHashSet();
		var validTiles = GetVaildTilesInRadius(tiledArea, radius).ToHashSet().Except(validBuildableAttackTiles).Except(buildingAreaTiles);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in validTiles)
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HightlightResourceTiles(Rect2I tiledArea, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(tiledArea, radius);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in resourceTiles)
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void ClearHighlightedTiles()
	{
		hightlightTileMapLayer.Clear();
	}

	public Vector2I GetMouseGridPosistionWithDimesionOffset(Vector2 dimensions)
	{
		var mousePosition = hightlightTileMapLayer.GetGlobalMousePosition() / 64;
		mousePosition -= dimensions / 2;
		mousePosition = mousePosition.Round();
		return new Vector2I((int)mousePosition.X, (int)mousePosition.Y);
	}

	public Vector2I getMouseGridPosition()
	{
		var mousePosition = hightlightTileMapLayer.GetGlobalMousePosition();
		return ConvertWorldPositionToTilePosition(mousePosition);
	}

	public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
	{
		var tilePosition = (worldPosition / 64).Floor();
		return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
	}

	public HashSet<Vector2I> GetBuildableTileSet(bool isAttackTiles = false)
	{
		return isAttackTiles ? validBuildableAttackTiles : vaildBuildableTiles;
	}


	private List<TileMapLayer> GetAllTilemaplayers(Node2D rootNode)
	{
		var result = new List<TileMapLayer>();
		var children = rootNode.GetChildren();
		children.Reverse();

		foreach (var child in children)
		{
			if (child is Node2D childNode)
			{
				result.AddRange(GetAllTilemaplayers(childNode));
			}
		}

		if (rootNode is TileMapLayer tileMapLayer)
		{
			result.Add(tileMapLayer);
		}

		return result;
	}
	private void UpdateGoblinOccupiedTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPosition());

		if (buildingComponent.BuildingResource.IsDangerBuilding())
		{
			var tilesInRadius = GetVaildTilesInRadius(buildingComponent.GetTileArea(), buildingComponent.BuildingResource.DangerRadius).ToHashSet();
			tilesInRadius.ExceptWith(occupiedTiles);
			goblinOccupiedTiles.UnionWith(tilesInRadius);
		}

	}

	private void UpdateVaildBuildableTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPosition());
		var tileArea = new Rect2I(buildingComponent.GetGridCellPosition(), buildingComponent.BuildingResource.Dimensions);
		var allTiles = GetTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius, (_) => true);
		allTilesInBuildingRadius.UnionWith(allTiles);
		var validTiles = GetVaildTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius);
		vaildBuildableTiles.UnionWith(validTiles);
		vaildBuildableTiles.ExceptWith(occupiedTiles);
		validBuildableAttackTiles.UnionWith(vaildBuildableTiles);
		vaildBuildableTiles.ExceptWith(goblinOccupiedTiles);

		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var tileArea = new Rect2I(buildingComponent.GetGridCellPosition(), buildingComponent.BuildingResource.Dimensions);
		var resourcetiles = GetResourceTilesInRadius(tileArea, buildingComponent.BuildingResource.ResourceRadius);

		var oldResourceTileCount = collectedResourceTiles.Count;

		collectedResourceTiles.UnionWith(resourcetiles);

		if (oldResourceTileCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourcetilesUpdated, collectedResourceTiles.Count);
		}
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateAttackTiles(BuildingComponent buildingComponent)
	{
		if (buildingComponent.BuildingResource.IsAttackBuilding()) return;



		var newAttackTiles = GetTilesInRadius(buildingComponent.GetTileArea(), buildingComponent.BuildingResource.AttackRadius, (_) => true).ToHashSet();

		attackTiles.UnionWith(newAttackTiles);
	}

	private void RecalculateGrid()
	{
		occupiedTiles.Clear();
		vaildBuildableTiles.Clear();
		validBuildableAttackTiles.Clear();
		allTilesInBuildingRadius.Clear();
		goblinOccupiedTiles.Clear();
		attackTiles.Clear();

		var buildingComponents = BuildingComponent.GetVaildBuildingComponents(this);

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateBuildingComponentGridState(buildingComponent);
		}

		EmitSignal(SignalName.ResourcetilesUpdated, collectedResourceTiles.Count);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void CheckGoblinCampDestruction()
	{
		var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);
	}

	private bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosition, float radius)
	{
		var distanceX = centerPosition.X - (tilePosition.X + .5);
		var distanceY = centerPosition.Y - (tilePosition.Y + .5);
		var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
		return distanceSquared <= radius * radius;
	}

	private List<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		var tileAreaF = tileArea.ToRect2F();
		var tileAreaCenter = tileAreaF.GetCenter();
		var radiusMod = Mathf.Max(tileAreaF.Size.X, tileAreaF.Size.Y) / 2;

		int checkedCount = 0;
		int insideCircleCount = 0;
		int passedFilterCount = 0;

		for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
		{
			for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
			{
				checkedCount++;

				var tilePosition = new Vector2I(x, y);

				bool insideCircle = IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod);
				if (!insideCircle) continue;

				insideCircleCount++;

				bool passedFilter = filterFn(tilePosition);
				if (!passedFilter) continue;

				passedFilterCount++;

				result.Add(tilePosition);

			}
		}

		return result;
	}

	private void MapTileMapLayersToElevationLayers()
	{
		foreach (var layer in allTilemapLayers)
		{
			ElevationLayer elevationLayer;
			Node startNode = layer;
			do
			{
				var parent = startNode.GetParent();
				elevationLayer = parent as ElevationLayer;
				startNode = parent;
			} while (elevationLayer == null && startNode != null);

			tileMapLayerToElevationLayer[layer] = elevationLayer;
		}
	}



	private List<Vector2I> GetVaildTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea, radius, (tilePosition) =>
		{
			return GetTileCustomData(tilePosition, IS_BUILDABLE).Item2;
		});
	}

	private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea, radius, (tilePosition) =>
		{
			return GetTileCustomData(tilePosition, IS_WOOD).Item2;
		});
	}

	private void UpdateBuildingComponentGridState(BuildingComponent buildingComponent)
	{
		UpdateGoblinOccupiedTiles(buildingComponent);
		UpdateVaildBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
		UpdateAttackTiles(buildingComponent);

	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateBuildingComponentGridState(buildingComponent);
		CheckGoblinCampDestruction();
	}

	private void OnBuildingDestoryed(BuildingComponent buildingComponent)
	{
		RecalculateGrid();
	}

}
