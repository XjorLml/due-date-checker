namespace DueDateChecker.App.Pages;

public sealed class DueProgressView : GraphicsView, IDrawable
{
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(float), typeof(DueProgressView), 0f, propertyChanged: Redraw);
    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(DueProgressView), Colors.Transparent, propertyChanged: Redraw);
    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(DueProgressView), Colors.Transparent, propertyChanged: Redraw);
    public float Progress { get => (float)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
    public Color TrackColor { get => (Color)GetValue(TrackColorProperty); set => SetValue(TrackColorProperty, value); }
    public Color ProgressColor { get => (Color)GetValue(ProgressColorProperty); set => SetValue(ProgressColorProperty, value); }

    public DueProgressView() => Drawable = this;
    private static void Redraw(BindableObject bindable, object oldValue, object newValue) => ((DueProgressView)bindable).Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - 12;
        if (size <= 0) return;
        var x = (dirtyRect.Width - size) / 2;
        var y = (dirtyRect.Height - size) / 2;
        canvas.StrokeSize = 8;
        canvas.StrokeColor = TrackColor;
        canvas.DrawEllipse(x, y, size, size);
        canvas.StrokeColor = ProgressColor;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawArc(x, y, size, size, 90, 90 - Math.Clamp(Progress, 0, 1) * 360, true, false);
    }
}
