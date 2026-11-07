using Android.App;
using Android.Graphics.Drawables;
using Android.Widget;
using DueDateChecker.App.Services;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Snackbar;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace DueDateChecker.App.Platforms.Android;

/// <summary>
/// UX06 messages as Material snackbars (DESIGN toasts/snackbars). A snackbar lives on the activity's
/// content view, not on a page, so a message shown right after the editor closes stays visible on the
/// page it returned to. Material also announces it through a polite live region and keeps an action
/// visible longer when the user has set a longer accessibility timeout.
/// </summary>
public sealed class AndroidMessageHost : IMessageHost
{
    private const int InformationMs = 3000;
    private const int UndoMs = 5000;
    private const float RadiusDp = 14f; // DESIGN radius-md

    public void Show(string message) => Present(message, InformationMs, undo: null);

    public void ShowUndo(string message, Func<Task<bool>> undo) => Present(message, UndoMs, undo);

    private static void Present(string message, int duration, Func<Task<bool>>? undo) => MainThread.BeginInvokeOnMainThread(() =>
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        var content = activity?.FindViewById(global::Android.Resource.Id.Content);
        if (activity is null || content is null) return;
        // Posted so a page that has just been navigated to lays out its bottom bar before the anchor is measured.
        content.Post(() => Build(activity, content, message, duration, undo).Show());
    });

    private static Snackbar Build(Activity activity, AView content, string message, int duration, Func<Task<bool>>? undo)
    {
        var snackbar = Snackbar.Make(content, message, duration);

        // Above the bottom bar and its Add disc when the bar shows (DESIGN: 16 above the bar); at the
        // bottom edge on pages that hide it.
        if (FindBottomBar(activity.Window?.DecorView) is { IsShown: true } bar)
            snackbar.SetAnchorView(bar.Parent as AView ?? bar);

        // Dark surface with light text in light mode, inverted in dark mode; Undo uses the other theme's primary.
        var dark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;
        var shape = new GradientDrawable();
        shape.SetColor(Token(dark ? "TextDark" : "TextLight"));
        shape.SetCornerRadius(RadiusDp * content.Resources!.DisplayMetrics!.Density);
        snackbar.View.Background = shape;
        snackbar.SetTextColor(Token(dark ? "SurfaceDark" : "SurfaceLight"));
        snackbar.SetActionTextColor(Token(dark ? "PrimaryLight" : "PrimaryDark"));
        snackbar.SetTextMaxLines(4); // 200% text must not cut a message off

        var typeface = IPlatformApplication.Current?.Services.GetService<IFontManager>()?.GetTypeface(Microsoft.Maui.Font.OfSize("JakartaMedium", 14));
        if (typeface is not null)
        {
            foreach (var text in Descendants(snackbar.View).OfType<TextView>())
            {
                text.Typeface = typeface;
                text.LetterSpacing = 0f; // Material spaces button text out ("U n d o"); the app's buttons do not
                text.SetAllCaps(false);
            }
        }

        // The Undo service handles its own failures and messages; the task is not awaited here.
        if (undo is not null) snackbar.SetAction("Undo", view => _ = undo());
        return snackbar;
    }

    private static global::Android.Graphics.Color Token(string key) =>
        Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color.ToPlatform()
            : global::Android.Graphics.Color.Gray;

    private static BottomNavigationView? FindBottomBar(AView? view) =>
        view is null ? null : view as BottomNavigationView ?? Descendants(view).OfType<BottomNavigationView>().FirstOrDefault();

    private static IEnumerable<AView> Descendants(AView view)
    {
        if (view is not AViewGroup group) yield break;
        for (var i = 0; i < group.ChildCount; i++)
        {
            if (group.GetChildAt(i) is not { } child) continue;
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }
}
