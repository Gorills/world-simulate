using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static class DesignSystem
{
    public static void ApplyBackground(ColorRect background)
    {
        ArgumentNullException.ThrowIfNull(background);
        background.Color = DesignTokens.Background;
    }

    public static void ApplyHeading(Label label)
    {
        ApplyLabel(label);
        label.AddThemeFontSizeOverride("font_size", DesignTokens.HeadingFontSize);
        label.AddThemeColorOverride("font_color", DesignTokens.Accent);
    }

    public static void ApplyLabel(Label label, bool muted = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        label.AddThemeFontSizeOverride("font_size", DesignTokens.BodyFontSize);
        label.AddThemeColorOverride(
            "font_color",
            muted ? DesignTokens.TextMuted : DesignTokens.TextPrimary);
    }

    public static void ApplyButton(Button button)
    {
        ArgumentNullException.ThrowIfNull(button);
        button.CustomMinimumSize = new Vector2(button.CustomMinimumSize.X, DesignTokens.ControlHeight);
        button.FocusMode = Control.FocusModeEnum.All;
        button.AddThemeFontSizeOverride("font_size", DesignTokens.BodyFontSize);
        button.AddThemeColorOverride("font_color", DesignTokens.TextPrimary);
        button.AddThemeColorOverride("font_hover_color", DesignTokens.Accent);
        button.AddThemeColorOverride("font_focus_color", DesignTokens.Accent);
        button.AddThemeColorOverride("font_pressed_color", DesignTokens.Positive);
    }

    public static void ApplyOptionButton(OptionButton button)
    {
        ArgumentNullException.ThrowIfNull(button);
        ApplyButton(button);

        var popup = button.GetPopup();
        popup.AddThemeFontSizeOverride("font_size", DesignTokens.BodyFontSize);
        popup.AddThemeColorOverride("font_color", DesignTokens.TextPrimary);
        popup.AddThemeColorOverride("font_hover_color", DesignTokens.Accent);
        popup.AddThemeColorOverride("font_disabled_color", DesignTokens.TextMuted);
    }

    public static void ApplySelectedButton(Button button)
    {
        ArgumentNullException.ThrowIfNull(button);
        button.AddThemeColorOverride("font_color", DesignTokens.Accent);
    }
}
