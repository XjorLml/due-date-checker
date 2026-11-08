namespace DueDateChecker.App.Pages;

/// <summary>Shared status pill. Bind <see cref="Status"/> to a row's StatusText ("Paid", "Overdue",
/// "Due today" or "Upcoming"); the XAML triggers choose tint, glyph and text colour from that word.</summary>
public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(nameof(Status), typeof(string), typeof(StatusBadge), "Upcoming");

    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusBadge() => InitializeComponent();
}
