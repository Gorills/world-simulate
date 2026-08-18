using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static partial class DesignSystem
{
    internal static void ApplyPageMargin(MarginContainer margin)
    {
        ArgumentNullException.ThrowIfNull(margin);
        ApplyMargins(margin, DesignTokens.PageMargin);
    }

    internal static void ApplyMargins(MarginContainer margin, int pixels)
    {
        ArgumentNullException.ThrowIfNull(margin);
        margin.AddThemeConstantOverride("margin_left", pixels);
        margin.AddThemeConstantOverride("margin_top", pixels);
        margin.AddThemeConstantOverride("margin_right", pixels);
        margin.AddThemeConstantOverride("margin_bottom", pixels);
    }

    internal static void ApplyStack(BoxContainer container, UiGap gap)
    {
        ArgumentNullException.ThrowIfNull(container);
        container.AddThemeConstantOverride("separation", Gap(gap));
    }

    internal static void ApplyGrid(GridContainer container, UiGap horizontal, UiGap vertical)
    {
        ArgumentNullException.ThrowIfNull(container);
        container.AddThemeConstantOverride("h_separation", Gap(horizontal));
        container.AddThemeConstantOverride("v_separation", Gap(vertical));
    }

    private static int Gap(UiGap gap) => gap switch
    {
        UiGap.Tight => DesignTokens.SpaceTight,
        UiGap.Small => DesignTokens.SpaceSmall,
        UiGap.Large => DesignTokens.SpaceLarge,
        _ => DesignTokens.SpaceMedium,
    };
}
