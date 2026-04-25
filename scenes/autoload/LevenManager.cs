using System.Linq;
using Godot;

namespace Game.Autoload;

public partial class LevenManager : Node
{

	public static LevenManager Instance { get; private set; }

	[Export]
	private LevenDefinitionResource[] levenDefinitions;

	private int currentLevelIndex;


	public override void _Notification(int what)
	{
		base._Notification(what);

		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}

	public static LevenDefinitionResource[] GetLevenDefinitions()
	{
		return Instance.levenDefinitions.ToArray();
	}

	public void ChangeToLeven(int levenIndex)
	{
		if (levenIndex >= levenDefinitions?.Length || levenIndex < 0) return;
		currentLevelIndex = levenIndex;
		var levenDefinition = levenDefinitions[currentLevelIndex];
		GetTree().ChangeSceneToFile(levenDefinition.levelScenePath);
	}

	public void ChangeToNextLevel()
	{
		ChangeToLeven(currentLevelIndex + 1);
	}


}
