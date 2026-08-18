using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static partial class DesignSystem
{
    private sealed record ButtonStyleSet(
        StyleBoxFlat Normal,
        StyleBoxFlat Hover,
        StyleBoxFlat Pressed,
        StyleBoxFlat Disabled,
        StyleBoxFlat Focus);

    private static readonly Color Transparent = new(0.0f, 0.0f, 0.0f, 0.0f);

    private static readonly Dictionary<UiSurface, StyleBoxFlat> SurfaceStyles =
        new Dictionary<UiSurface, StyleBoxFlat>
        {
            [UiSurface.Window] = FlatBox(
                DesignTokens.SurfaceWindow,
                DesignTokens.BorderStrong,
                DesignTokens.RadiusLarge,
                DesignTokens.WindowPadding,
                DesignTokens.WindowShadowSize),
            [UiSurface.Card] = FlatBox(
                DesignTokens.SurfaceCard,
                DesignTokens.BorderSubtle,
                DesignTokens.RadiusMedium,
                DesignTokens.CardPadding,
                shadowSize: 2),
            [UiSurface.Inset] = FlatBox(
                DesignTokens.SurfaceInset,
                DesignTokens.BorderSubtle,
                DesignTokens.RadiusMedium,
                DesignTokens.InsetPadding,
                shadowSize: 0),
            [UiSurface.Floating] = FlatBox(
                DesignTokens.SurfaceWindow,
                DesignTokens.BorderStrong,
                DesignTokens.RadiusMedium,
                DesignTokens.FloatingPadding,
                DesignTokens.FloatingShadowSize),
        };

    private static readonly Dictionary<UiButtonRole, ButtonStyleSet> ButtonStyles =
        new Dictionary<UiButtonRole, ButtonStyleSet>
        {
            [UiButtonRole.Primary] = new(
                FlatBox(
                    DesignTokens.Accent,
                    DesignTokens.AccentHover,
                    DesignTokens.RadiusMedium,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    DesignTokens.AccentHover,
                    DesignTokens.FocusRing,
                    DesignTokens.RadiusMedium,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    DesignTokens.AccentPressed,
                    DesignTokens.AccentPressed,
                    DesignTokens.RadiusMedium,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    DesignTokens.SurfaceDisabled,
                    DesignTokens.BorderSubtle,
                    DesignTokens.RadiusMedium,
                    DesignTokens.SpaceSmall,
                    0),
                FocusStyle(DesignTokens.RadiusMedium)),
            [UiButtonRole.Secondary] = StandardButtonStyles(),
            [UiButtonRole.Ghost] = new(
                FlatBox(
                    Transparent,
                    Transparent,
                    DesignTokens.RadiusSmall,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    DesignTokens.SurfaceHover,
                    DesignTokens.BorderSubtle,
                    DesignTokens.RadiusSmall,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    DesignTokens.SurfacePressed,
                    DesignTokens.BorderSubtle,
                    DesignTokens.RadiusSmall,
                    DesignTokens.SpaceSmall,
                    0),
                FlatBox(
                    Transparent,
                    Transparent,
                    DesignTokens.RadiusSmall,
                    DesignTokens.SpaceSmall,
                    0),
                FocusStyle(DesignTokens.RadiusSmall)),
            [UiButtonRole.Row] = RowStyles(selected: false),
            [UiButtonRole.SelectedRow] = RowStyles(selected: true),
        };

    private static readonly Dictionary<UiTone, StyleBoxFlat> BadgeStyles =
        Enum.GetValues<UiTone>().ToDictionary(
            tone => tone,
            tone =>
            {
                var color = ToneColor(tone);
                return FlatBox(
                    WithAlpha(color, 0.12f),
                    WithAlpha(color, 0.62f),
                    DesignTokens.RadiusSmall,
                    DesignTokens.SpaceTight,
                    shadowSize: 0);
            });

    private static readonly StyleBoxFlat PopupHoverStyle = FlatBox(
        DesignTokens.SurfaceHover,
        DesignTokens.BorderSubtle,
        DesignTokens.RadiusSmall,
        DesignTokens.SpaceSmall,
        shadowSize: 0);

    private static readonly StyleBoxFlat DividerStyle = FlatBox(
        DesignTokens.BorderSubtle,
        DesignTokens.BorderSubtle,
        radius: 0,
        padding: 0,
        shadowSize: 0);

    private static void ApplyButtonColors(Button button, UiButtonRole role)
    {
        var normal = role == UiButtonRole.Primary
            ? DesignTokens.TextOnAccent
            : DesignTokens.TextPrimary;
        var hover = role == UiButtonRole.Primary
            ? DesignTokens.TextOnAccent
            : DesignTokens.AccentHover;
        var pressed = role == UiButtonRole.Primary
            ? DesignTokens.TextOnAccent
            : DesignTokens.TextPrimary;
        var selected = role == UiButtonRole.SelectedRow
            ? DesignTokens.AccentHover
            : normal;

        button.AddThemeFontSizeOverride("font_size", DesignTokens.FontBody);
        button.AddThemeColorOverride("font_color", selected);
        button.AddThemeColorOverride("font_hover_color", hover);
        button.AddThemeColorOverride("font_focus_color", selected);
        button.AddThemeColorOverride("font_pressed_color", pressed);
        button.AddThemeColorOverride("font_hover_pressed_color", pressed);
        button.AddThemeColorOverride("font_disabled_color", DesignTokens.TextDisabled);
    }

    private static void ApplyButtonStyles(Button button, UiButtonRole role)
    {
        var styles = ButtonStyles[role];
        button.AddThemeStyleboxOverride("normal", styles.Normal);
        button.AddThemeStyleboxOverride("hover", styles.Hover);
        button.AddThemeStyleboxOverride("pressed", styles.Pressed);
        button.AddThemeStyleboxOverride("hover_pressed", styles.Pressed);
        button.AddThemeStyleboxOverride("disabled", styles.Disabled);
        button.AddThemeStyleboxOverride("focus", styles.Focus);
    }

    private static ButtonStyleSet StandardButtonStyles() => new(
        FlatBox(
            DesignTokens.SurfaceCard,
            DesignTokens.BorderSubtle,
            DesignTokens.RadiusMedium,
            DesignTokens.SpaceSmall,
            0),
        FlatBox(
            DesignTokens.SurfaceHover,
            DesignTokens.BorderStrong,
            DesignTokens.RadiusMedium,
            DesignTokens.SpaceSmall,
            0),
        FlatBox(
            DesignTokens.SurfacePressed,
            DesignTokens.AccentPressed,
            DesignTokens.RadiusMedium,
            DesignTokens.SpaceSmall,
            0),
        FlatBox(
            DesignTokens.SurfaceDisabled,
            DesignTokens.BorderSubtle,
            DesignTokens.RadiusMedium,
            DesignTokens.SpaceSmall,
            0),
        FocusStyle(DesignTokens.RadiusMedium));

    private static ButtonStyleSet RowStyles(bool selected)
    {
        var baseColor = selected ? DesignTokens.AccentSoft : DesignTokens.SurfaceInset;
        var border = selected ? DesignTokens.Accent : DesignTokens.BorderSubtle;
        return new ButtonStyleSet(
            FlatBox(
                baseColor,
                border,
                DesignTokens.RadiusSmall,
                DesignTokens.SpaceSmall,
                0),
            FlatBox(
                DesignTokens.SurfaceHover,
                DesignTokens.BorderStrong,
                DesignTokens.RadiusSmall,
                DesignTokens.SpaceSmall,
                0),
            FlatBox(
                DesignTokens.SurfacePressed,
                DesignTokens.AccentPressed,
                DesignTokens.RadiusSmall,
                DesignTokens.SpaceSmall,
                0),
            FlatBox(
                DesignTokens.SurfaceDisabled,
                DesignTokens.BorderSubtle,
                DesignTokens.RadiusSmall,
                DesignTokens.SpaceSmall,
                0),
            FocusStyle(DesignTokens.RadiusSmall));
    }

    private static StyleBoxFlat SurfaceStyle(UiSurface surface) => SurfaceStyles[surface];

    private static StyleBoxFlat BadgeStyle(UiTone tone) => BadgeStyles[tone];

    private static StyleBoxFlat FocusStyle(int radius)
    {
        var focus = FlatBox(
            Transparent,
            DesignTokens.FocusRing,
            radius,
            padding: 0,
            shadowSize: 0);
        focus.DrawCenter = false;
        focus.SetBorderWidthAll(DesignTokens.FocusWidth);
        focus.SetExpandMarginAll(1.0f);
        return focus;
    }

    private static StyleBoxFlat FlatBox(
        Color background,
        Color border,
        int radius,
        int padding,
        int shadowSize)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            ShadowColor = new Color(
                0.0f,
                0.0f,
                0.0f,
                shadowSize > 0 ? 0.42f : 0.0f),
            ShadowOffset = new Vector2(0.0f, shadowSize > 0 ? 3.0f : 0.0f),
            ShadowSize = shadowSize,
        };
        style.SetBorderWidthAll(border.A <= 0.001f ? 0 : DesignTokens.BorderWidth);
        style.SetCornerRadiusAll(radius);
        style.SetContentMarginAll(padding);
        return style;
    }

    private static Color ToneColor(UiTone tone) => tone switch
    {
        UiTone.Info => DesignTokens.Info,
        UiTone.Positive => DesignTokens.Positive,
        UiTone.Warning => DesignTokens.Warning,
        UiTone.Danger => DesignTokens.Danger,
        _ => DesignTokens.TextSecondary,
    };
}
