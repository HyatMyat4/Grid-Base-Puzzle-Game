using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Resources.Building;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{

	[Export(PropertyHint.File, "*tres")]
	private string buildingResourcePath;

	private HashSet<Vector2I> occupiedTiles = new();

	public BuildingResource BuildingResource { get; private set; }


	public override void _Ready()
	{
		if (buildingResourcePath != null)
		{
			BuildingResource = GD.Load<BuildingResource>(buildingResourcePath);
		}
		AddToGroup(nameof(BuildingComponent));
		Callable.From(Initialize).CallDeferred();
	}

	public Vector2I GetGridCellPosition()
	{
		var gridPosition = GlobalPosition / 64;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	public void CalculateOccupiedCellPosition()
	{

		var gridPosition = GetGridCellPosition();
		for (int x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
		{
			for (int y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
			{
				occupiedTiles.Add(new Vector2I(x, y));
			}
		}

	}
	public HashSet<Vector2I> GetOccupiedCellPosition()
	{
		return occupiedTiles.ToHashSet();
	}

	public void Distory()
	{
		GameEvent.EmitBuildingDestroyed(this);
		Owner.QueueFree();
	}

	public bool IsTileInBuildingArea(Vector2I tilePosition)
	{
		return occupiedTiles.Contains(tilePosition);
	}

	private void Initialize()
	{
		CalculateOccupiedCellPosition();
		GameEvent.EmitBuildingPlaced(this);
	}
}

