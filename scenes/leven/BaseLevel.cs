using Game.Manager;
using Godot;


namespace GridBasePuzzle;

public partial class BaseLeven : Node
{

	private GridManager gridManager;
	private GoldMine goldMine;
	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("GridManager");
		goldMine = GetNode<GoldMine>("%GoldMine");

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
