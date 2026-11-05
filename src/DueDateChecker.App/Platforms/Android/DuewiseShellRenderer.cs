using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace DueDateChecker.App.Platforms.Android;

/// <summary>
/// Android-only presentation of the Shell bottom bar. Shell owns the five slots and the
/// navigation; this renderer only (1) keeps the bar readable at large text, (2) disables the
/// centre slot that Shell reserved for Add, and (3) floats the lime Add disc over that slot.
/// </summary>
public sealed class DuewiseShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new TabAppearance(this, shellItem);

    private sealed class TabAppearance(IShellContext context, ShellItem item)
        : ShellBottomNavViewAppearanceTracker(context, item)
    {
        private const float DiscDp = 56f;      // DESIGN §7 floating action: 56 dp disc
        private const float RaiseDp = 16f;     // raised about 16 dp above the bar's top edge
        private const float RingDp = 4f;       // surface-coloured ring separating disc from bar
        private readonly IShellContext _context = context;
        private AddDock? _dock;

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);
            var resources = bottomView.Resources!;
            var fontScale = Math.Max(1f, resources.Configuration!.FontScale);
            // Native 56 dp crowds a 200% caption against its icon. Keep the design's
            // 64 dp baseline and grow the bar with text; Android still owns insets.
            var height = 64f + 20f * (fontScale - 1f);
            bottomView.SetMinimumHeight((int)Math.Ceiling(height * resources.DisplayMetrics!.Density));
            bottomView.LabelVisibilityMode = NavigationBarView.LabelVisibilityLabeled;

            var menu = bottomView.Menu;
            if (menu.Size() > AppShell.AddSlot)
            {
                // The slot keeps its width and "Add" caption but never selects a tab; the disc handles taps.
                var slot = menu.GetItem(AppShell.AddSlot)!;
                slot.SetEnabled(false);
            }

            _dock ??= AddDock.Wrap(bottomView, _context.Shell as AppShell);
            _dock?.ApplyColors(appearance);
        }

        /// <summary>
        /// A FrameLayout inserted between Shell's layout and its BottomNavigationView. The bar keeps
        /// a 16 dp top margin inside it and the disc is pinned to the dock's top edge, so the disc
        /// rises 16 dp above the bar and overlaps its centre slot.
        /// When Shell hides the bar (editor, Details, Reminders) the dock measures to zero.
        /// </summary>
        private sealed class AddDock : FrameLayout
        {
            private readonly BottomNavigationView _bar;
            private readonly FrameLayout _disc;
            private readonly ImageView _plus;
            private readonly GradientDrawable _discShape;
            private readonly float _density;

            private AddDock(Context context, BottomNavigationView bar, AppShell? shell) : base(context)
            {
                _bar = bar;
                _density = context.Resources!.DisplayMetrics!.Density;
                SetClipChildren(false);
                SetClipToPadding(false);

                _discShape = new GradientDrawable();
                _discShape.SetShape(ShapeType.Oval);
                _disc = new FrameLayout(context)
                {
                    Background = _discShape,
                    // Above the Material bar's own 8 dp elevation: Android routes touches to the highest Z child,
                    // so a lower disc would lose every tap that lands inside the bar's bounds.
                    Elevation = Dp(12),
                    Clickable = true,
                    Focusable = true,
                    ContentDescription = "Add due"
                };
                _plus = new ImageView(context);
                var plusId = context.Resources.GetIdentifier("nav_add", "drawable", context.PackageName);
                if (plusId != 0) _plus.SetImageResource(plusId);
                _disc.AddView(_plus, new LayoutParams(Dp(24), Dp(24), GravityFlags.Center));
                _disc.Click += async (_, _) => { if (shell is not null) await shell.OpenAddAsync(); };
                AddView(_disc, new LayoutParams(Dp(DiscDp), Dp(DiscDp), GravityFlags.Top | GravityFlags.CenterHorizontal));
            }

            public static AddDock? Wrap(BottomNavigationView bar, AppShell? shell)
            {
                if (bar.Parent is not ViewGroup parent || parent is AddDock) return null;
                var index = parent.IndexOfChild(bar);
                var layoutParams = bar.LayoutParameters;
                var dock = new AddDock(bar.Context!, bar, shell);
                parent.RemoveView(bar);
                // The bar sits RaiseDp below the dock top; the disc is pinned to the top edge, so it rises above the bar.
                dock.AddView(bar, 0, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, GravityFlags.Bottom) { TopMargin = dock.Dp(RaiseDp) });
                parent.AddView(dock, index, layoutParams);
                return dock;
            }

            public void ApplyColors(IShellAppearanceElement appearance)
            {
                var context = Context!;
                var accent = context.GetColor(Resource.Color.duewiseAccent);
                var onAccent = context.GetColor(Resource.Color.duewiseOnAccent);
                var ring = appearance.EffectiveTabBarBackgroundColor?.ToPlatform() ?? new global::Android.Graphics.Color(context.GetColor(Resource.Color.duewiseSurface));
                _discShape.SetColor(accent);
                _discShape.SetStroke(Dp(RingDp), ring);
                _plus.ImageTintList = global::Android.Content.Res.ColorStateList.ValueOf(new global::Android.Graphics.Color(onAccent));
                Invalidate();
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                if (_bar.Visibility == ViewStates.Gone)
                {
                    SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), 0);
                    return;
                }
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }

            protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
            {
                if (_bar.Visibility == ViewStates.Gone)
                {
                    _disc.Layout(0, 0, 0, 0);
                    return;
                }
                base.OnLayout(changed, left, top, right, bottom);
            }

            private int Dp(float dp) => (int)Math.Round(dp * _density);
        }
    }
}
