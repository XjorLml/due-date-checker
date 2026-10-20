using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace DueDateChecker.App.Platforms.Android;

public sealed class DuewiseShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new TabAppearance(this, shellItem);

    private sealed class TabAppearance(IShellContext context, ShellItem item)
        : ShellBottomNavViewAppearanceTracker(context, item)
    {
        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);
            var resources = bottomView.Resources!;
            var fontScale = Math.Max(1f, resources.Configuration!.FontScale);
            // Native 56 dp crowds a 200% caption against its icon. Keep the design's
            // 64 dp baseline and grow the bar with text; Android still owns insets.
            var height = 64f + 20f * (fontScale - 1f);
            bottomView.SetMinimumHeight((int)Math.Ceiling(height * resources.DisplayMetrics!.Density));
        }
    }
}
