using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static class DesignTokens
{
    public static readonly Color Background = new(0.055f, 0.063f, 0.075f);
    public static readonly Color Surface = new(0.11f, 0.125f, 0.145f);
    public static readonly Color TextPrimary = new(0.92f, 0.93f, 0.90f);
    public static readonly Color TextMuted = new(0.63f, 0.67f, 0.70f);
    public static readonly Color Accent = new(0.82f, 0.69f, 0.36f);
    public static readonly Color Positive = new(0.45f, 0.78f, 0.55f);

    public const int SpaceSmall = 8;
    public const int SpaceMedium = 16;
    public const int SpaceLarge = 24;
    public const int BodyFontSize = 18;
    public const int HeadingFontSize = 24;
    public const int ControlHeight = 44;
}
