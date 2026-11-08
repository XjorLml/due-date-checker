using Microsoft.Maui.Controls.Shapes;

namespace DueDateChecker.App.Pages;

/// <summary>
/// An in-page outline glyph from the live prototype's icon set (prototypes/design-exploration/app.js),
/// drawn as a vector path so it takes any theme colour without per-theme image files. 24-unit
/// geometry, 1.8 stroke, round caps and joins (DESIGN §6). Decorative: the control that hosts it
/// carries the accessible name.
/// </summary>
public sealed class Glyph : ContentView
{
    // Rect and circle elements of the prototype are written as path data so one Path draws each glyph.
    private static readonly Dictionary<string, string> Paths = new(StringComparer.Ordinal)
    {
        ["bell"] = "M6 9A6 6 0 0 1 18 9C18 16 21 16 21 18H3C3 16 6 16 6 9M10 21H14",
        ["back"] = "M14 6L8 12L14 18",
        ["next"] = "M9 6L15 12L9 18",
        ["arrow"] = "M5 12H19M14 7L19 12L14 17",
        ["check"] = "M5 12L9 16L19 6",
        ["edit"] = "M15 5L19 9M4 20L9 19L21 7A2.8 2.8 0 0 0 17 3L5 15Z",
        ["trash"] = "M3 6H21M9 6V3H15V6M5 6L6 21H18L19 6M10 10V17M14 10V17",
        ["repeat"] = "M17 2L21 6L17 10M3 11V8A2 2 0 0 1 5 6H21M7 22L3 18L7 14M21 13V16A2 2 0 0 1 19 18H3",
        ["search"] = "M4 10.5A6.5 6.5 0 1 0 17 10.5A6.5 6.5 0 1 0 4 10.5ZM16 16L21 21",
        ["shield"] = "M12 3L20 6V12C20 16 15 20 12 21C9 20 4 16 4 12V6ZM8 12L11 15L16 9",
        ["calendar"] = "M6 5H18A3 3 0 0 1 21 8V18A3 3 0 0 1 18 21H6A3 3 0 0 1 3 18V8A3 3 0 0 1 6 5ZM7 3V7M17 3V7M3 10H21M8 14H10M14 14H16M8 17H10",
        ["plus"] = "M12 5V19M5 12H19",
        ["minus"] = "M5 12H19",
        // Due types (prototype "Bill", "Task", "Subscription", "Other").
        // Philippine preset-category symbols share the same outline treatment.
        ["home"] = "M3 10L12 3L21 10V21H3ZM9 21V14H15V21",
        ["bolt"] = "M13 2L3 14H11L10 22L21 9H13Z",
        ["water"] = "M12 2C10 6 5 10 5 15A7 7 0 0 0 19 15C19 10 14 6 12 2ZM8 15A4 4 0 0 0 12 19",
        ["wifi"] = "M2 8C7 3 17 3 22 8M5 12C9 8 15 8 19 12M8 16C10 14 14 14 16 16M12 20H12.1",
        ["mobile"] = "M7 2H17V22H7ZM7 17H17M11 20H13",
        ["tv"] = "M3 7H21V20H3ZM8 2L12 7L16 2",
        ["credit"] = "M3 5H21V19H3ZM3 10H21M7 15H10",
        ["government"] = "M2 8L12 2L22 8ZM4 10V19M9 10V19M15 10V19M20 10V19M2 22H22",
        ["health"] = "M12 21L4 13C-2 6 7 0 12 6C17 0 26 6 20 13ZM9 10H15M12 7V13",
        ["school"] = "M2 9L12 3L22 9L12 15ZM6 12V18C10 22 14 22 18 18V12M22 9V17",
        ["family"] = "M8 3A3 3 0 1 0 8 9A3 3 0 1 0 8 3ZM17 5A2.5 2.5 0 1 0 17 10A2.5 2.5 0 1 0 17 5ZM2 21V17A6 6 0 0 1 14 17V21M15 13A5 5 0 0 1 22 18V21",
        ["business"] = "M3 7H21V21H3ZM8 7V3H16V7M3 12C9 16 15 16 21 12M10 13V16H14V13",
        ["bill"] = "M6 3H18V21L15 19L12 21L9 19L6 21ZM9 7H15M9 11H15M9 15H12",
        ["task"] = "M8 4H16A4 4 0 0 1 20 8V16A4 4 0 0 1 16 20H8A4 4 0 0 1 4 16V8A4 4 0 0 1 8 4ZM8 12L11 15L16 9",
        ["subscription"] = "M12 3L20 8V16L12 21L4 16V8ZM4 8L12 13L20 8M12 13V21",
        ["other"] = "M9 4H15A5 5 0 0 1 20 9V15A5 5 0 0 1 15 20H9A5 5 0 0 1 4 15V9A5 5 0 0 1 9 4ZM8 12H16M12 8V16",
    };

    private static readonly PathGeometryConverter Converter = new();

    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(string), typeof(Glyph), "", propertyChanged: (view, _, _) => ((Glyph)view).UpdateGeometry());

    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(Glyph), Colors.Black, propertyChanged: (view, _, value) => ((Glyph)view)._path.Stroke = new SolidColorBrush((Color)value));

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(Glyph), 24d, propertyChanged: (view, _, _) => ((Glyph)view).UpdateSize());

    private readonly Microsoft.Maui.Controls.Shapes.Path _path = new()
    {
        StrokeThickness = 1.8,
        StrokeLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round,
        Aspect = Stretch.None,
        AnchorX = 0,
        AnchorY = 0,
        Stroke = new SolidColorBrush(Colors.Black),
    };

    public Glyph()
    {
        Content = _path;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        UpdateSize();
    }

    /// <summary>One of the keys above, such as "bell" or "bill". Unknown names draw nothing.</summary>
    public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
    /// <summary>Rendered size in dp; the 24-unit geometry is scaled to it.</summary>
    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    /// <summary>The glyph name for a due type, matching the prototype's type icons.</summary>
    public static string ForType(Core.Domain.DueCategory type) => type switch
    {
        Core.Domain.DueCategory.Bill => "bill",
        Core.Domain.DueCategory.Task => "task",
        Core.Domain.DueCategory.Subscription => "subscription",
        _ => "other"
    };

    private void UpdateGeometry()
    {
        if (!Paths.TryGetValue(Kind ?? "", out var data)) { _path.Data = null; return; }
        // A fresh geometry per glyph: shapes do not share Geometry instances.
        _path.Data = (Geometry)Converter.ConvertFromInvariantString(data)!;
    }

    private void UpdateSize()
    {
        WidthRequest = HeightRequest = Size;
        _path.WidthRequest = _path.HeightRequest = 24;
        _path.Scale = Size / 24d;
    }
}
