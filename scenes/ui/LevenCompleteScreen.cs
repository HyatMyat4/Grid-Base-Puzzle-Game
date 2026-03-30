using Game.Autoload;
using Godot;
namespace Game.UI;


public partial class LevenCompleteScreen : CanvasLayer
{
	private Button nextLevelButton;

	public override void _Ready()
	{
		nextLevelButton = GetNode<Button>("%NextLevenButton");

		nextLevelButton.Pressed += OnNextLevelButtonPressed;


	}

	private void OnNextLevelButtonPressed()
	{
		LevenManager.Instance.ChangeToNextLevel();
	}
}
