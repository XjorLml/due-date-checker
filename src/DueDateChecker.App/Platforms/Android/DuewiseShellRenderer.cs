using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using AndroidX.Core.View;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace DueDateChecker.App.Platforms.Android;

/// <summary>
/// Android-only presentation of the Shell bottom bar. Shell owns the five slots and the
/// navigation; this renderer only (1) keeps the bar readable at large text, (2) disables the
/// centre slot that Shell reserved for Add, (3) floats the lime Add disc over that slot, and
/// (4) draws the Focus look: a floating rounded card, a lime pill behind the selected icon and
/// Plus Jakarta Sans captions (DESIGN §7 bottom bar).
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
            // UX13: captions stop growing at LayoutRules.MaxCaptionScale, so the bar does too.
            var fontScale = Math.Clamp(resources.Configuration!.FontScale, 1f, (float)ViewModels.LayoutRules.MaxCaptionScale);
            // Native 56 dp crowds an enlarged caption against its icon. Keep the design's
            // 64 dp baseline and grow the bar with the capped text. The system navigation inset is
            // added below the card by AddDock, not inside it.
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
            ApplyFocusLook(bottomView);
            _captions ??= CaptionFitter.Attach(bottomView);
        }

        private CaptionFitter? _captions;

        private const float CardRadiusDp = 22f;   // Focus bar: floating card (prototype .app-nav, refinement "focus")
        private const float CardMarginDp = 10f;   // canvas visible left, right and below the card
        private const float CardElevationDp = 6f;
        private const float CardInnerPaddingDp = 4f;
        private const float PillWidthDp = 36f;    // lime pill behind the selected icon
        private const float PillHeightDp = 32f;
        private const float PillRadiusDp = 11f;

        /// <summary>
        /// Focus look of the bar as selected in the HTML exploration (gap found 2026-11-07): a rounded
        /// surface card with a 1 dp border, and the selected tab's icon on a lime pill in OnAccent.
        /// Shell's base appearance runs first and sets plain colours; this replaces them on every call,
        /// so a theme change (which re-runs SetAppearance) picks up the matching tokens.
        /// </summary>
        private static void ApplyFocusLook(BottomNavigationView bar)
        {
            var density = bar.Resources!.DisplayMetrics!.Density;
            int Dp(float dp) => (int)Math.Round(dp * density);

            var card = new GradientDrawable();
            card.SetColor(Token("Surface"));
            card.SetCornerRadius(Dp(CardRadiusDp));
            card.SetStroke(Math.Max(1, Dp(1)), Token("Border"));
            bar.Background = card;
            bar.Elevation = Dp(CardElevationDp);
            bar.ClipToOutline = true; // ripples stay inside the rounded corners
            // Inner side padding keeps the first and last captions clear of the rounded corners (prototype: 4 px).
            bar.SetPadding(Dp(CardInnerPaddingDp), bar.PaddingTop, Dp(CardInnerPaddingDp), bar.PaddingBottom);
            if (OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                var shadow = new global::Android.Graphics.Color(bar.Context!.GetColor(Resource.Color.duewiseShadow));
                bar.SetOutlineSpotShadowColor(shadow);
                bar.SetOutlineAmbientShadowColor(shadow);
            }

            bar.ItemActiveIndicatorEnabled = true;
            bar.ItemActiveIndicatorColor = ColorStateList.ValueOf(Token("Accent"));
            bar.ItemActiveIndicatorWidth = Dp(PillWidthDp);
            bar.ItemActiveIndicatorHeight = Dp(PillHeightDp);
            bar.ItemActiveIndicatorShapeAppearance = ShapeAppearanceModel.InvokeBuilder().SetAllCornerSizes(Dp(PillRadiusDp)).Build();

            // Selected icon sits on lime, so it uses OnAccent (in dark theme Primary is lime too and would vanish).
            bar.ItemIconTintList = States(Token("OnAccent"), Token("TextSecondary"), Token("TextSecondary"));
            // Captions: selected Primary; the disabled Add slot is the disc's label, so it reads as Text; others TextSecondary.
            bar.ItemTextColor = States(Token("Primary"), Token("Text"), Token("TextSecondary"));
        }

        private static ColorStateList States(global::Android.Graphics.Color selected, global::Android.Graphics.Color disabled, global::Android.Graphics.Color other)
            => new(
                [[global::Android.Resource.Attribute.StateChecked], [-global::Android.Resource.Attribute.StateEnabled], []],
                [selected.ToArgb(), disabled.ToArgb(), other.ToArgb()]);

        /// <summary>A DESIGN colour token from Colors.xaml for the app's current theme (follows the in-app Appearance choice).</summary>
        internal static global::Android.Graphics.Color Token(string name)
        {
            var app = Microsoft.Maui.Controls.Application.Current;
            var key = name + (app?.RequestedTheme == AppTheme.Dark ? "Dark" : "Light");
            // A missing token must not crash the bar; grey shows up in review instead.
            return app?.Resources.TryGetValue(key, out var value) == true && value is Color color
                ? color.ToPlatform()
                : global::Android.Graphics.Color.Gray;
        }

        /// <summary>
        /// UX13 (decided 2026-11-06): every caption uses DESIGN's 12 sp caption size, follows the system text
        /// size only up to LayoutRules.MaxCaptionScale, and shrinks slightly when the longest caption would
        /// not fit its slot (narrow phones). Active and inactive captions get the same size, so selecting a
        /// tab no longer enlarges its caption past the slot. Runs after each layout because the slot width is
        /// only known then; it changes sizes only when they differ, so it settles after one pass.
        /// </summary>
        private sealed class CaptionFitter : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            private const float CaptionSp = 12f;   // DESIGN typography.caption
            private const float SlotPaddingDp = 4f; // keeps a caption off the slot's edges
            private readonly BottomNavigationView _bar;

            private CaptionFitter(BottomNavigationView bar) => _bar = bar;

            public static CaptionFitter Attach(BottomNavigationView bar)
            {
                var fitter = new CaptionFitter(bar);
                bar.ViewTreeObserver?.AddOnGlobalLayoutListener(fitter);
                return fitter;
            }

            public void OnGlobalLayout()
            {
                var labels = Descendants(_bar).OfType<TextView>().ToList();
                var items = _bar.Menu.Size();
                if (labels.Count == 0 || items == 0 || _bar.Width <= 0) return;

                // Fonts first, so the fit below measures the faces that will be drawn (Jakarta is wider than Roboto).
                ApplyTypefaces(items);
                var resources = _bar.Resources!;
                var density = resources.DisplayMetrics!.Density;
                var basePx = CaptionSp * density;
                // Measure every caption at the normal size in its own typeface (the active one may be bold).
                var longestDp = labels.Max(label =>
                {
                    using var paint = new global::Android.Graphics.Paint(label.Paint) { TextSize = basePx };
                    return paint.MeasureText(label.Text ?? "");
                }) / density;
                var slotDp = (_bar.Width - _bar.PaddingLeft - _bar.PaddingRight) / density / items - 2 * SlotPaddingDp;

                var scale = ViewModels.LayoutRules.CaptionScale(resources.Configuration!.FontScale, slotDp, longestDp);
                var px = (float)(basePx * scale);
                foreach (var label in labels)
                {
                    if (Math.Abs(label.TextSize - px) > 0.5f) label.SetTextSize(global::Android.Util.ComplexUnitType.Px, px);
                }
            }

            /// <summary>
            /// Captions use the app's Plus Jakarta Sans like the Focus prototype: Bold for the selected tab,
            /// SemiBold for the Add disc's label, Medium otherwise. Material swaps label views when the
            /// selection changes, so this runs after every layout and only sets a face that differs.
            /// </summary>
            private void ApplyTypefaces(int items)
            {
                if (_bar.ChildCount == 0 || _bar.GetChildAt(0) is not ViewGroup menuView) return;
                for (var i = 0; i < menuView.ChildCount && i < items; i++)
                {
                    var face = i == AppShell.AddSlot ? Face("PlusJakartaSans-SemiBold.ttf")
                        : _bar.Menu.GetItem(i)!.IsChecked ? Face("PlusJakartaSans-Bold.ttf") : Face("PlusJakartaSans-Medium.ttf");
                    if (face is null || menuView.GetChildAt(i) is not { } itemView) continue;
                    foreach (var label in Descendants(itemView).OfType<TextView>())
                    {
                        if (!Equals(label.Typeface, face)) label.Typeface = face;
                    }
                }
            }

            private readonly Dictionary<string, global::Android.Graphics.Typeface?> _faces = [];

            /// <summary>The app's font files are packaged as Android assets (MauiFont); load each once.</summary>
            private global::Android.Graphics.Typeface? Face(string file)
            {
                if (_faces.TryGetValue(file, out var face)) return face;
                try { face = global::Android.Graphics.Typeface.CreateFromAsset(_bar.Context!.Assets, file); }
                catch (Java.Lang.RuntimeException) { face = null; } // missing asset: keep the system font
                return _faces[file] = face;
            }

            private static IEnumerable<AView> Descendants(AView view)
            {
                if (view is not ViewGroup group) yield break;
                for (var i = 0; i < group.ChildCount; i++)
                {
                    if (group.GetChildAt(i) is not { } child) continue;
                    yield return child;
                    foreach (var nested in Descendants(child)) yield return nested;
                }
            }
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
                // Side and bottom margins let the canvas show around the floating card.
                dock.AddView(bar, 0, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, GravityFlags.Bottom) { TopMargin = dock.Dp(RaiseDp) });
                dock.PlaceCard(0, 0, 0);
                // Replaces Material's own listener, which would pad the inside of the card with the system
                // navigation inset; the inset becomes part of the margin instead, so the card floats above it.
                ViewCompat.SetOnApplyWindowInsetsListener(bar, new InsetsToMargin(dock));
                parent.AddView(dock, index, layoutParams);
                ViewCompat.RequestApplyInsets(bar);
                return dock;
            }

            private void PlaceCard(int left, int right, int bottom)
            {
                if (_bar.LayoutParameters is not LayoutParams lp) return;
                var (l, r, b) = (Dp(CardMarginDp) + left, Dp(CardMarginDp) + right, Dp(CardMarginDp) + bottom);
                if (lp.LeftMargin == l && lp.RightMargin == r && lp.BottomMargin == b) return;
                lp.LeftMargin = l; lp.RightMargin = r; lp.BottomMargin = b;
                _bar.LayoutParameters = lp;
            }

            private sealed class InsetsToMargin(AddDock dock) : Java.Lang.Object, AndroidX.Core.View.IOnApplyWindowInsetsListener
            {
                public WindowInsetsCompat? OnApplyWindowInsets(AView? view, WindowInsetsCompat? insets)
                {
                    if (view is null || insets is null) return insets;
                    var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
                    view.SetPadding(view.PaddingLeft, view.PaddingTop, view.PaddingRight, 0);
                    dock.PlaceCard(bars?.Left ?? 0, bars?.Right ?? 0, bars?.Bottom ?? 0);
                    return insets;
                }
            }

            public void ApplyColors(IShellAppearanceElement appearance)
            {
                var context = Context!;
                var accent = context.GetColor(Resource.Color.duewiseAccent);
                var onAccent = context.GetColor(Resource.Color.duewiseOnAccent);
                var ring = appearance.EffectiveTabBarBackgroundColor?.ToPlatform() ?? new global::Android.Graphics.Color(context.GetColor(Resource.Color.duewiseSurface));
                _discShape.SetColor(accent);
                _discShape.SetStroke(Dp(RingDp), ring);
                // The page canvas shows around the floating card and behind the raised disc.
                SetBackgroundColor(Token("Background"));
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
