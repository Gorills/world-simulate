using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.InteractionMenu;

public partial class InteractionMenu : PanelContainer
{
    private Label _heading = null!;
    private Button _askAboutWork = null!;
    private Button _encourage = null!;
    private Button _shareRation = null!;
    private ResidentProjection? _resident;

    public event Action<ResidentInteractionChoice>? ChoiceRequested;

    public override void _Ready()
    {
        var content = GetNode<VBoxContainer>("Content");
        _heading = GetNode<Label>("Content/Heading");
        _askAboutWork = GetNode<Button>("Content/AskAboutWork");
        _encourage = GetNode<Button>("Content/Encourage");
        _shareRation = GetNode<Button>("Content/ShareRation");

        DesignSystem.ApplySurface(this, UiSurface.Card);
        DesignSystem.ApplyStack(content, UiGap.Small);
        DesignSystem.ApplyText(_heading, UiTextRole.SectionHeading);
        foreach (var button in Buttons())
        {
            DesignSystem.ApplyButton(button, UiButtonRole.Secondary);
        }

        _askAboutWork.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.AskAboutWork);
        _encourage.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.Encourage);
        _shareRation.Pressed += () => ChoiceRequested?.Invoke(ResidentInteractionChoice.ShareRation);
        RefreshLocalization();
    }

    public void SetResident(ResidentProjection resident)
    {
        ArgumentNullException.ThrowIfNull(resident);
        _resident = resident;
        RefreshLocalization();
    }

    public void RefreshLocalization()
    {
        if (_heading is null)
        {
            return;
        }

        _heading.Text = _resident is null
            ? GameLocalization.Tr("UI_INTERACT")
            : GameLocalization.Format("UI_INTERACT_WITH", _resident.Name);
        _askAboutWork.Text = GameLocalization.Tr("UI_ASK_ABOUT_WORK");
        _encourage.Text = GameLocalization.Tr("UI_ENCOURAGE");
        _shareRation.Text = GameLocalization.Tr("UI_SHARE_RATION");
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
