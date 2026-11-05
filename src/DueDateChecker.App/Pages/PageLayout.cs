namespace DueDateChecker.App.Pages;

/// <summary>DESIGN §5 margins in MAUI logical units, independent of text scaling.
/// Focus baseline: 20 dp content inset, 14 dp in compact frames, wider on tablets.</summary>
internal static class PageLayout
{
    /// <param name="footer">An optional fixed action row below the scrolling content; it shares the horizontal margins.</param>
    /// <param name="extraBottom">Extra bottom clearance, used by root pages so the last row stays clear of the raised Add action.</param>
    public static void Connect(ContentPage page, Layout content, Layout? footer = null, double extraBottom = 0)
    {
        page.SizeChanged += (_, _) => Apply();
        page.Loaded += (_, _) => Apply();

        void Apply()
        {
            var width = page.Width;
            if (width <= 0) return;
            var margin = width < 360 ? 14 : width < 600 ? 20 : width < 840 ? 32 : 40;
            var readable = Math.Min(width, width >= 840 ? 720 : width >= 600 ? 640 : width);
            content.WidthRequest = readable;
            content.HorizontalOptions = LayoutOptions.Center;
            content.Padding = new Thickness(margin, 12, margin, 24 + extraBottom);
            if (footer is null) return;
            footer.WidthRequest = readable;
            footer.HorizontalOptions = LayoutOptions.Center;
            footer.Padding = new Thickness(margin, 12, margin, 12);
        }
    }
}
