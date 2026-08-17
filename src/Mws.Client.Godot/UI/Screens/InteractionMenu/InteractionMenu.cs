using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.InteractionMenu;

public partial class InteractionMenu : VBoxContainer
{
    private Button _askAboutWork = null!;
    private Button _encourage = null!;
    private Button _shareRation = null!;

    public event Action<ResidentInteractionChoice>? ChoiceRequested;

    public override void _Ready()
    {
        _askAboutWork = GetNode<Button>("AskAboutWork");
        _encourage = GetNode<Button>("Encourage");
        _shareRation = GetNode<Button>("ShareRation");

        DesignSystem.ApplyLabel(GetNode<Label>("Heading"), muted: true);
        foreach (var button in Buttons())
        {
            DesignSystem.ApplyButton(button);
        }

        _askAboutWork.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.AskAboutWork);
        _encourage.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.Encourage);
        _shareRation.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.ShareRation);
    }

    public void SetResident(ResidentProjection resident)
    {
        ArgumentNullException.ThrowIfNull(resident);
        GetNode<Label>("Heading").Text = GameLocalization.Format("UI_INTERACT_WITH", resident.Name);
    }

    public void FocusFirst() => _askAboutWork.GrabFocus();

    public bool HasMenuFocus() => Buttons().Any(button => button.HasFocus());

    private IEnumerable<Button> Buttons()
    {
        yield return _askAboutWork;
        yield return _encourage;
        yield return _shareRation;
    }
}
