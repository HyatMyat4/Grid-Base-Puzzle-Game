using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	private HashSet<Vector2I> vaildBuildableTiles = new HashSet<Vector2I>();

	[Export]
	private TileMapLayer hightlightTileMapLayer;

	[Export]
	private TileMapLayer baseTerrainTileMapLayer;

	private List<TileMapLayer> allTilemapLayers = new();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameEvent.Instance.BuildingPlaced += OnBuildingPlaced;
		allTilemapLayers = GetAllTilemaplayers(baseTerrainTileMapLayer);

		foreach (var layer in allTilemapLayers)
		{
			GD.Print(layer?.Name);
		}
	}

	public bool IsTilePositionValid(Vector2I tilePosition)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);
			if (customData == null) continue;
			return (bool)customData.GetCustomData("buildable");
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

	public void HightlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		ClearHighlightedTiles();
		HightlightBuildableTiles();

		var validTiles = GetVaildTilesInRadius(rootCell, radius).ToHashSet();
		var expandedBuildableTiles = validTiles.Except(vaildBuildableTiles).Except(GetOccupiedTiles());
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in expandedBuildableTiles)
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
		var gridPosition = (mousePosition / 64).Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	private List<TileMapLayer> GetAllTilemaplayers(TileMapLayer rootTileMapLayer)
	{
		var result = new List<TileMapLayer>();
		var children = rootTileMapLayer.GetChildren();
		children.Reverse();

		foreach (var child in children)
		{
			if (child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTilemaplayers(childLayer));
			}
		}
		result.Add(rootTileMapLayer);
		return result;
	}

	private void UpdateVaildBuildableTiles(BuildingComponent buildingComponent)
	{
		var validTiles = GetVaildTilesInRadius(buildingComponent.GetGridCellPosition(), buildingComponent.BuildableRadius);
		vaildBuildableTiles.UnionWith(validTiles);
		vaildBuildableTiles.ExceptWith(GetOccupiedTiles());

	}

	private List<Vector2I> GetVaildTilesInRadius(Vector2I rootCell, int radius)
	{
		var result = new List<Vector2I>();
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{

				var tilePosition = new Vector2I(x, y);
				if (!IsTilePositionValid(tilePosition)) continue;
				result.Add(tilePosition);
			}
		}
		return result;
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateVaildBuildableTiles(buildingComponent);
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponenets = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		var occupiedTiles = buildingComponenets.Select(x => x.GetGridCellPosition());
		return occupiedTiles;
	}
}
