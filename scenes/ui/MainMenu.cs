using Godot;

namespace Game.UI;

public partial class MainMenu : Node
{
	private Button playButton;

	private Button quitButton;
	private Control mainMenuContainer;
	private LevenSelectScreen levenSelectScreen;

	public override void _Ready()
	{

		playButton = GetNode<Button>("%PlayButton");
		quitButton = GetNode<Button>("%QuitButton");
		mainMenuContainer = GetNode<Control>("%MainMenuContainer");
		levenSelectScreen = GetNode<LevenSelectScreen>("%LevenSelectScreen");

		levenSelectScreen.Visible = false;
		mainMenuContainer.Visible = true;

		playButton.Pressed += OnPlayButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed;


		levenSelectScreen.BackPressed += OnLevelSelectBackPressed;
	}

	private void OnPlayButtonPressed()
	{

		levenSelectScreen.Visible = true;
		mainMenuContainer.Visible = false;

	}

	private void OnLevelSelectBackPressed()
	{
		levenSelectScreen.Visible = false;
		mainMenuContainer.Visible = true;
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

}