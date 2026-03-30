using Game.Autoload;
using Godot;
namespace Game.UI;

public partial class MainMenu : Node
{

	private Button playButton;
	public override void _Ready()
	{
		playButton = GetNode<Button>("%PlayButton");

		playButton.Pressed += OnPlayButtonPressed;
	}

	private void OnPlayButtonPressed()
	{
		LevenManager.Instance.ChangeToLeven(0);
	}
}
