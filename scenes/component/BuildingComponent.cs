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

	[Export]
	private BuildingAnimatorComponent buildingAnimatorComponent;

	private HashSet<Vector2I> occupiedTiles = new();

	public BuildingResource BuildingResource { get; private set; }

	public bool IsDestorying { get; private set; }


	public static IEnumerable<BuildingComponent> GetVaildBuildingComponents(Node node)
	{
		return node.GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>().Where((buildingComponent) => !buildingComponent.IsDestorying);
	}

	public static IEnumerable<BuildingComponent> GetDangerBuildingComponents(Node node)
	{
		return GetVaildBuildingComponents(node).Where((buildingComponent) => buildingComponent.BuildingResource.IsDangerBuilding());
	}

	public override void _Ready()
	{
		if (buildingResourcePath != null)
		{
			BuildingResource = GD.Load<BuildingResource>(buildingResourcePath);
		}

		if (buildingAnimatorComponent != null)
		{
			buildingAnimatorComponent.DestoryAnimationFinished += OnDestoryAnimationFinished;
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

	public Rect2I GetTileArea()
	{
		var rootCell = GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, BuildingResource.Dimensions);
		return tileArea;
	}

	public void Distory()
	{
		IsDestorying = true;
		GameEvent.EmitBuildingDestroyed(this);
		buildingAnimatorComponent?.PlayDestoryAnimation();
		if (buildingAnimatorComponent == null)
		{
			Owner.QueueFree();
		}

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

	private void OnDestoryAnimationFinished()
	{
		Owner.QueueFree();
	}
}

