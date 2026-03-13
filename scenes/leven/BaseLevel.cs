using Game;
using Game.Manager;
using Godot;


namespace GridBasePuzzle;

public partial class BaseLevel : Node
{

	private GridManager gridManager;
	private GoldMine goldMine;
	private GameCamera gameCamera;
	private Node2D baseBuilding;
	private TileMapLayer baseTerrainTilemapLayer;

	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("GridManager");
		goldMine = GetNode<GoldMine>("%GoldMine");
		gameCamera = GetNode<GameCamera>("GameCamera");
		baseTerrainTilemapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
		baseBuilding = GetNode<Node2D>("%Base");

		Rect2I usedRect = baseTerrainTilemapLayer.GetUsedRect();


		gameCamera.SetBoundingRect(usedRect);
		gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

		gridManager.GridStateUpdated += OnGridStateUpdate;
	}

	private void OnGridStateUpdate()
	{
		var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
		if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
		{
			goldMine.SetActive();
			GD.Print("Win");
		}
	}
}
