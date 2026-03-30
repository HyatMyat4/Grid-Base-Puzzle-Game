using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{

	[Signal]
	public delegate void SelectButtonPressedEventHandler();

	private Label titleLabel;

	private Label descriptionLabel;

	private Label costLabel;
	private Button selectButton;
	public override void _Ready()
	{
		titleLabel = GetNode<Label>("%TittleLabel");
		descriptionLabel = GetNode<Label>("%DescriptionLabel");
		costLabel = GetNode<Label>("%CostLabel");
		selectButton = GetNode<Button>("%Button");

		selectButton.Pressed += OnSelectButtonPressed;
	}


	public void SetBuildingResource(BuildingResource buildingResource)
	{
		titleLabel.Text = buildingResource.Displayname;
		costLabel.Text = $"{buildingResource.ResourceCost}";
		descriptionLabel.Text = buildingResource.Description;
	}

	private void OnSelectButtonPressed()
	{
		EmitSignal(SignalName.SelectButtonPressed);
	}

}
