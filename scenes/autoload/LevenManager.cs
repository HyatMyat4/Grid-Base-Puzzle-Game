using Godot;

namespace Game.Autoload;

public partial class LevenManager : Node
{

	public static LevenManager Instance { get; private set; }

	[Export]
	private PackedScene[] levenScenes;

	private int currentLevelIndex;


	public override void _Notification(int what)
	{
		base._Notification(what);

		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

	public void ChangeToLeven(int levenIndex)
	{
		if (levenIndex >= levenScenes?.Length || levenIndex < 0) return;
		currentLevelIndex = levenIndex;
		var levenScene = levenScenes[currentLevelIndex];
		GetTree().ChangeSceneToPacked(levenScene);
	}

	public void ChangeToNextLevel()
	{
		ChangeToLeven(currentLevelIndex + 1);
	}


}
