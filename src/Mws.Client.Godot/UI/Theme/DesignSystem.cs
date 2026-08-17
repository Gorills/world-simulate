using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static partial class DesignSystem
{
    internal static void ApplyBackdrop(ColorRect background)
    {
        ArgumentNullException.ThrowIfNull(background);
        background.Color = DesignTokens.Canvas;
    }

    internal static void ApplySurface(PanelContainer panel, UiSurface surface)
    {
        ArgumentNullException.ThrowIfNull(panel);
        panel.AddThemeStyleboxOverride("panel", SurfaceStyle(surface));
    }

    internal static void ApplyText(Label label, UiTextRole role = UiTextRole.Body)
    {
        ArgumentNullException.ThrowIfNull(label);
        var (size, color) = role switch
        {
            UiTextRole.Display => (DesignTokens.FontDisplay, DesignTokens.TextPrimary),
            UiTextRole.Heading => (DesignTokens.FontHeading, DesignTokens.TextPrimary),
            UiTextRole.SectionHeading => (DesignTokens.FontSection, DesignTokens.Accent),
            UiTextRole.Muted => (DesignTokens.FontMuted, DesignTokens.TextSecondary),
            UiTextRole.Caption => (DesignTokens.FontCaption, DesignTokens.TextMuted),
            UiTextRole.Metric => (DesignTokens.FontMetric, DesignTokens.AccentHover),
            _ => (DesignTokens.FontBody, DesignTokens.TextPrimary),
        };

        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeConstantOverride("line_spacing", role == UiTextRole.Caption ? 1 : 2);
    }

    internal static void ApplyHeading(Label label) => ApplyText(label, UiTextRole.Heading);

    internal static void ApplyLabel(Label label, bool muted = false) =>
        ApplyText(label, muted ? UiTextRole.Muted : UiTextRole.Body);

    internal static void ApplyButton(Button button, UiButtonRole role = UiButtonRole.Secondary)
    {
        ArgumentNullException.ThrowIfNull(button);
        button.FocusMode = Control.FocusModeEnum.All;
        button.CustomMinimumSize = new Vector2(
            button.CustomMinimumSize.X,
            role == UiButtonRole.Ghost ? DesignTokens.CompactControlHeight : DesignTokens.ControlHeight);

        ApplyButtonColors(button, role);
        ApplyButtonStyles(button, role);
    }

    internal static void ApplySelectedButton(Button button) =>
        ApplyButton(button, UiButtonRole.SelectedRow);

    internal static void ApplyOptionButton(OptionButton button)
    {
        ArgumentNullException.ThrowIfNull(button);
        ApplyButton(button, UiButtonRole.Secondary);

        var popup = button.GetPopup();
        popup.AddThemeFontSizeOverride("font_size", DesignTokens.FontBody);
        popup.AddThemeColorOverride("font_color", DesignTokens.TextPrimary);
        popup.AddThemeColorOverride("font_hover_color", DesignTokens.TextPrimary);
        popup.AddThemeColorOverride("font_disabled_color", DesignTokens.TextDisabled);
        popup.AddThemeStyleboxOverride("panel", SurfaceStyle(UiSurface.Floating));
        popup.AddThemeStyleboxOverride("hover", PopupHoverStyle);
        popup.AddThemeConstantOverride("item_start_padding", DesignTokens.SpaceMedium);
        popup.AddThemeConstantOverride("item_end_padding", DesignTokens.SpaceMedium);
        popup.AddThemeConstantOverride("v_separation", DesignTokens.SpaceTight);
    }

    internal static void ApplyBadge(Label label, UiTone tone)
    {
        ArgumentNullException.ThrowIfNull(label);
        ApplyText(label, UiTextRole.Caption);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.CustomMinimumSize = new Vector2(44.0f, 24.0f);

        var color = ToneColor(tone);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeStyleboxOverride("normal", BadgeStyle(tone));
    }

    internal static void ApplyDivider(HSeparator divider)
    {
        ArgumentNullException.ThrowIfNull(divider);
        divider.AddThemeConstantOverride("separation", 1);
        divider.AddThemeStyleboxOverride("separator", DividerStyle);
    }

    internal static void ApplyScroll(ScrollContainer scroll)
    {
        ArgumentNullException.ThrowIfNull(scroll);
        scroll.DrawFocusBorder = true;
        scroll.FollowFocus = true;
        scroll.AddThemeStyleboxOverride("panel", SurfaceStyle(UiSurface.Inset));
        scroll.AddThemeStyleboxOverride(
            "focus",
            ButtonStyles[UiButtonRole.Secondary].Focus);
    }

    internal static Color DataColor(UiDataColor role) => role switch
    {
        UiDataColor.MapBackground => DesignTokens.SurfaceInset,
        UiDataColor.Road => DesignTokens.DebugRoad,
        UiDataColor.Field => DesignTokens.DebugField,
        UiDataColor.Building => DesignTokens.DebugBuilding,
        UiDataColor.Home => DesignTokens.DebugHome,
        UiDataColor.Route => DesignTokens.DebugRoute,
        UiDataColor.Player => DesignTokens.TextPrimary,
        UiDataColor.Working => DesignTokens.Warning,
        UiDataColor.Eating => DesignTokens.Danger,
        UiDataColor.Resting => DesignTokens.DebugResting,
        UiDataColor.Idle => DesignTokens.Info,
        UiDataColor.Danger => DesignTokens.Danger,
        _ => DesignTokens.TextMuted,
    };

    private static Color WithAlpha(Color color, float alpha) =>
        new(color.R, color.G, color.B, alpha);
}
