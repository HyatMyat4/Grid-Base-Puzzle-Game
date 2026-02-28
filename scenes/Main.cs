using Game.Manager;
using Godot;


namespace GridBasePuzzle;

public partial class Main : Node
{

	private GridManager gridManager;
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2I? mouseGridCellPosition;

	private Node2D ySortRoot;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/building/Buiding.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		ySortRoot = GetNode<Node2D>("YSortRoot");
		placeBuildingButton.Pressed += OnButtonPressed;
		cursor.Visible = false;
		//placeBuildingButton.Connect(Button.SignalName.Pressed, Callable.From(OnButtonPressed));
		GD.Print("Hello World");
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (mouseGridCellPosition.HasValue && cursor.Visible && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(mouseGridCellPosition.Value))
		{
			GD.Print("Left Clicked");
			PlacedBuildingAtMousePosition();
			cursor.Visible = false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!cursor.Visible) return;
		var gridPosition = gridManager.getMouseGridPosition();
		cursor.GlobalPosition = gridPosition * 64;
		if (!mouseGridCellPosition.HasValue || mouseGridCellPosition.Value != gridPosition)
		{
			mouseGridCellPosition = gridPosition;
			gridManager.HightlightExpandedBuildableTiles(mouseGridCellPosition.Value, 3);
		}
	}

	private void PlacedBuildingAtMousePosition()
	{
		{
			if (!mouseGridCellPosition.HasValue) return;

			Node2D building = buildingScene.Instantiate<Node2D>();
			ySortRoot.AddChild(building);

			Vector2I gridPosition = mouseGridCellPosition.Value;
			building.GlobalPosition = gridPosition * 64;
			mouseGridCellPosition = null;

			gridManager.ClearHighlightedTiles();
			GD.Print("gridPosition", building.GlobalPosition);
		}
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
		GD.Print("button pressed");
	}
}
