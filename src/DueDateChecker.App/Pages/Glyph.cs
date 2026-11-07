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
