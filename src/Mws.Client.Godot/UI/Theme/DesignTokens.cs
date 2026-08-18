using Godot;

namespace Mws.Client.Godot.UI.Theme;

internal static class DesignTokens
{
    // Palette: quiet dark surfaces with a warm, village-compatible focus accent.
    internal static readonly Color Canvas = new(0.035f, 0.043f, 0.052f, 1.0f);
    internal static readonly Color CanvasOverlay = new(0.020f, 0.025f, 0.031f, 0.92f);
    internal static readonly Color SurfaceWindow = new(0.075f, 0.090f, 0.108f, 0.985f);
    internal static readonly Color SurfaceCard = new(0.095f, 0.112f, 0.132f, 0.985f);
    internal static readonly Color SurfaceInset = new(0.050f, 0.061f, 0.074f, 0.96f);
    internal static readonly Color SurfaceHover = new(0.135f, 0.151f, 0.168f, 1.0f);
    internal static readonly Color SurfacePressed = new(0.165f, 0.174f, 0.181f, 1.0f);
    internal static readonly Color SurfaceDisabled = new(0.070f, 0.078f, 0.088f, 0.82f);

    internal static readonly Color BorderSubtle = new(0.235f, 0.255f, 0.270f, 0.70f);
    internal static readonly Color BorderStrong = new(0.390f, 0.410f, 0.420f, 0.86f);
    internal static readonly Color FocusRing = new(0.94f, 0.76f, 0.38f, 1.0f);

    internal static readonly Color TextPrimary = new(0.93f, 0.94f, 0.92f, 1.0f);
    internal static readonly Color TextSecondary = new(0.76f, 0.79f, 0.80f, 1.0f);
    internal static readonly Color TextMuted = new(0.57f, 0.61f, 0.63f, 1.0f);
    internal static readonly Color TextDisabled = new(0.44f, 0.47f, 0.49f, 1.0f);
    internal static readonly Color TextOnAccent = new(0.105f, 0.090f, 0.060f, 1.0f);

    internal static readonly Color Accent = new(0.84f, 0.64f, 0.30f, 1.0f);
    internal static readonly Color AccentHover = new(0.92f, 0.72f, 0.38f, 1.0f);
    internal static readonly Color AccentPressed = new(0.72f, 0.51f, 0.22f, 1.0f);
    internal static readonly Color AccentSoft = new(0.84f, 0.64f, 0.30f, 0.16f);

    internal static readonly Color Info = new(0.37f, 0.67f, 0.82f, 1.0f);
    internal static readonly Color Positive = new(0.43f, 0.76f, 0.55f, 1.0f);
    internal static readonly Color Warning = new(0.91f, 0.65f, 0.26f, 1.0f);
    internal static readonly Color Danger = new(0.88f, 0.34f, 0.30f, 1.0f);

    internal static readonly Color DebugRoad = new(0.45f, 0.36f, 0.25f, 0.92f);
    internal static readonly Color DebugField = new(0.33f, 0.34f, 0.19f, 0.88f);
    internal static readonly Color DebugBuilding = new(0.39f, 0.42f, 0.45f, 0.94f);
    internal static readonly Color DebugHome = new(0.52f, 0.43f, 0.31f, 0.96f);
    internal static readonly Color DebugRoute = new(0.69f, 0.76f, 0.82f, 0.62f);
    internal static readonly Color DebugResting = new(0.63f, 0.49f, 0.86f, 1.0f);

    internal const int SpaceTight = 4;
    internal const int SpaceSmall = 8;
    internal const int SpaceMedium = 14;
    internal const int SpaceLarge = 22;
    internal const int PageMargin = 24;

    internal const int RadiusSmall = 6;
    internal const int RadiusMedium = 10;
    internal const int RadiusLarge = 14;

    internal const int FontCaption = 14;
    internal const int FontMuted = 16;
    internal const int FontBody = 17;
    internal const int FontSection = 18;
    internal const int FontHeading = 23;
    internal const int FontDisplay = 28;
    internal const int FontMetric = 20;

    internal const int ControlHeight = 42;
    internal const int CompactControlHeight = 34;
    internal const int WindowPadding = 20;
    internal const int CardPadding = 16;
    internal const int InsetPadding = 10;
    internal const int FloatingPadding = 14;

    internal const int BorderWidth = 1;
    internal const int FocusWidth = 2;
    internal const int WindowShadowSize = 10;
    internal const int FloatingShadowSize = 12;
}
