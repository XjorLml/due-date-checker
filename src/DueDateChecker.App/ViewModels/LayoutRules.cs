namespace DueDateChecker.App.ViewModels;

/// <summary>
/// UX Wave 5 sizing rules (decided 2026-11-06), kept free of MAUI and Android types so they can be
/// tested. All sizes are in device-independent units (dp).
/// </summary>
public static class LayoutRules
{
    /// <summary>UX13: bar captions follow the system text size up to 130%, the level from which
    /// DESIGN already switches to compact behaviour. Icons and TalkBack names carry the rest.</summary>
    public const double MaxCaptionScale = 1.3;

    /// <summary>UX13: a caption may shrink this far below its normal size to fit a narrow slot; past
    /// that it is truncated rather than made unreadably small.</summary>
    public const double MinCaptionScale = 0.85;

    /// <summary>DESIGN day cell: 44 dp wide wherever seven of them fit.</summary>
    public const double DayCellWidth = 44;

    /// <summary>WCAG 2.2 AA minimum target size (24 px; 24 dp is never smaller).</summary>
    public const double MinimumTarget = 24;

    private const double DayGap = 2; // MonthGrid ColumnSpacing

    /// <summary>
    /// UX13: the scale for bar captions. <paramref name="slotWidth"/> is the room a caption has in one
    /// slot and <paramref name="longestCaptionWidth"/> the widest caption at normal size; 0 means not yet
    /// measured, which uses the capped system size alone.
    /// </summary>
    public static double CaptionScale(double fontScale, double slotWidth, double longestCaptionWidth)
    {
        var scale = Math.Min(fontScale, MaxCaptionScale);
        if (slotWidth > 0 && longestCaptionWidth > 0)
            scale = Math.Min(scale, Math.Max(MinCaptionScale, slotWidth / longestCaptionWidth));
        return scale;
    }

    /// <summary>
    /// UX15 (option B, an exception to DESIGN's 44 dp target): the minimum width of a day cell in a
    /// month row <paramref name="gridWidth"/> wide. 44 dp wherever seven columns and their gaps fit;
    /// otherwise the cells share the row (about 40 dp at 320 dp), never below 24.
    /// </summary>
    public static double DayCellMinimumWidth(double gridWidth)
    {
        if (gridWidth <= 0) return DayCellWidth;
        var shared = (gridWidth - 6 * DayGap) / 7;
        return Math.Max(MinimumTarget, Math.Min(DayCellWidth, Math.Floor(shared)));
    }

    /// <summary>UX16: root page headings drop from Heading1 (26) to the Heading2 size (20) when the
    /// window is wider than it is tall, so landscape shows more rows.</summary>
    public static double RootHeadingSize(double width, double height) => width > 0 && width > height ? 20 : 26;
}
