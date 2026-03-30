using Game;
using Game.Manager;
using Game.UI;
using Godot;


namespace GridBasePuzzle;

public partial class BaseLevel : Node
{

	[Export]
	private PackedScene levenCompleteScreenScene;

	private GridManager gridManager;
	private GoldMine goldMine;
	private GameCamera gameCamera;
	private Node2D baseBuilding;
	private TileMapLayer baseTerrainTilemapLayer;

	private GameUI gameUI;

	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("GridManager");
		goldMine = GetNode<GoldMine>("%GoldMine");
		gameCamera = GetNode<GameCamera>("GameCamera");
		baseTerrainTilemapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
		baseBuilding = GetNode<Node2D>("%Base");
		gameUI = GetNode<GameUI>("GameUI");
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
			var levenCompleteScreen = levenCompleteScreenScene.Instantiate<LevenCompleteScreen>();
			AddChild(levenCompleteScreen);
			goldMine.SetActive();
			gameUI.HideUI();
			GD.Print("Win");
		}
	}
}
