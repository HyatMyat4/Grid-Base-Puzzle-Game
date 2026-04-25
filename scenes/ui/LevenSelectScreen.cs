using Game.Autoload;
using Godot;
using System;

namespace Game.UI;

public partial class LevenSelectScreen : MarginContainer
{
	[Signal]
	public delegate void BackPressedEventHandler();

	[Export]
	private PackedScene levelSelectSectionScene;

	private GridContainer gridContainer;

	private Button backButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("%GridContainer");
		backButton = GetNode<Button>("BackButton");

		var levelDefinitions = LevenManager.GetLevenDefinitions();
		for (var i = 0; i < levelDefinitions.Length; i++)
		{
			var levenDefinition = levelDefinitions[i];
			var levelSelectSection = levelSelectSectionScene.Instantiate<LevenSelectSection>();
			gridContainer.AddChild(levelSelectSection);

			levelSelectSection.SetLevelDefinition(levenDefinition);
			levelSelectSection.SetLevelIndex(i);
			levelSelectSection.LevelSelected += OnLevelSelected;
		}

		backButton.Pressed += OnBackButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnLevelSelected(int levelIndex)
	{
		LevenManager.Instance.ChangeToLeven(levelIndex);
	}

	private void OnBackButtonPressed()
	{
		EmitSignal(SignalName.BackPressed);
	}
}
