namespace DueDateChecker.App.Pages;

/// <summary>DESIGN §5 margins in MAUI logical units, independent of text scaling.</summary>
internal static class PageLayout
{
    public static void Connect(ContentPage page, Layout content)
    {
        page.SizeChanged += (_, _) => Apply();
        page.Loaded += (_, _) => Apply();

        void Apply()
        {
            var width = page.Width;
            if (width <= 0) return;
            var margin = width < 360 ? 16 : width < 600 ? 24 : width < 840 ? 32 : 40;
            content.WidthRequest = Math.Min(width, width >= 840 ? 720 : width >= 600 ? 640 : width);
            content.HorizontalOptions = LayoutOptions.Center;
            content.Padding = new Thickness(margin, 16, margin, 24);
        }
    }
}
