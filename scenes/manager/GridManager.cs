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

	private HashSet<Vector2I> collectedResourceTiles = new HashSet<Vector2I>();

	private HashSet<Vector2I> occupiedTiles = new HashSet<Vector2I>();

	[Export]
	private TileMapLayer hightlightTileMapLayer;

	[Export]
	private TileMapLayer baseTerrainTileMapLayer;

	private List<TileMapLayer> allTilemapLayers = new();

	private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameEvent.Instance.BuildingPlaced += OnBuildingPlaced;
		GameEvent.Instance.BuildingDestroyed += OnBuildingDestoryed;
		allTilemapLayers = GetAllTilemaplayers(baseTerrainTileMapLayer);

	}
	public bool TileHasCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null || (bool)customData.GetCustomData(IS_IGNORE))
			{
				continue;
			}

			var value = customData.GetCustomData(dataName);


			return (bool)value;
		}

		return false;
	}


	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return vaildBuildableTiles.Contains(tilePosition);
	}

	public void HightlightBuildableTiles()
	{
		foreach (var tilePosition in vaildBuildableTiles)
		{
			hightlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HightlightExpandedBuildableTiles(Rect2I tiledArea, int radius)
	{

		var validTiles = GetVaildTilesInRadius(tiledArea, radius).ToHashSet();
		var expandedBuildableTiles = validTiles.Except(vaildBuildableTiles).Except(occupiedTiles);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in expandedBuildableTiles)
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

	private void UpdateVaildBuildableTiles(BuildingComponent buildingComponent)
	{
		occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPosition());
		var tileArea = new Rect2I(buildingComponent.GetGridCellPosition(), buildingComponent.BuildingResource.Dimensions);
		var validTiles = GetVaildTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius);
		vaildBuildableTiles.UnionWith(validTiles);
		vaildBuildableTiles.ExceptWith(occupiedTiles);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var tileArea = new Rect2I(buildingComponent.GetGridCellPosition(), buildingComponent.BuildingResource.Dimensions);
		var resourcetiles = GetResourceTilesInRadius(tileArea, buildingComponent.BuildingResource.ResourceRadius);
		var oldResourceTileCount = collectedResourceTiles.Count;
		GD.Print(oldResourceTileCount, collectedResourceTiles.Count);
		collectedResourceTiles.UnionWith(resourcetiles);
		if (oldResourceTileCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourcetilesUpdated, collectedResourceTiles.Count);
		}
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void RecalculateGrid(BuildingComponent excludeBuildingComponent)
	{
		occupiedTiles.Clear();
		vaildBuildableTiles.Clear();
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>().Where((buildingComponent) => buildingComponent != excludeBuildingComponent);
		foreach (var buildingComponent in buildingComponents)
		{
			UpdateVaildBuildableTiles(buildingComponent);
			UpdateCollectedResourceTiles(buildingComponent);
		}

		EmitSignal(SignalName.ResourcetilesUpdated, collectedResourceTiles.Count);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private List<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
		{
			for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if (!filterFn(tilePosition)) continue;
				result.Add(tilePosition);
			}
		}
		return result;
	}

	private void MapTileMapLayersToElevationLayers()
	{
		foreach (var layer in allTilemapLayers)
		{
			
		}
	}

	private List<Vector2I> GetVaildTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_BUILDABLE);
		});
	}

	private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(tileArea, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_WOOD);
		});
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateVaildBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
	}

	private void OnBuildingDestoryed(BuildingComponent buildingComponent)
	{
		RecalculateGrid(buildingComponent);
	}

}
