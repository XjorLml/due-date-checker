namespace DueDateChecker.App.Pages;

/// <summary>
/// The row status from the live prototype: glyph and word in the status colour, without the filled
/// pill (DESIGN prototype alignment, "Due row"). Details keeps <see cref="StatusBadge"/>. The word is
/// always present, so status never depends on colour alone (principle 3).
/// </summary>
public sealed class StatusLine : HorizontalStackLayout
{
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(string), typeof(StatusLine), "", propertyChanged: (view, _, _) => ((StatusLine)view).Update());

    private readonly Label _glyph = new() { FontFamily = "JakartaSemibold", FontSize = 13, VerticalOptions = LayoutOptions.Center };
    private readonly Label _word = new() { FontFamily = "JakartaMedium", FontSize = 13, VerticalOptions = LayoutOptions.Center };

    public StatusLine()
    {
        Spacing = 4;
        AutomationProperties.SetIsInAccessibleTree(_glyph, false);
        AutomationProperties.SetIsInAccessibleTree(_word, false);
        Children.Add(_glyph);
        Children.Add(_word);
        Update();
    }

    public string Status { get => (string)GetValue(StatusProperty); set => SetValue(StatusProperty, value); }

    private void Update()
    {
        var (glyph, token) = Status switch
        {
            "Paid" => ("✓", "Completed"),
            "Overdue" => ("!", "Destructive"),
            "Due today" => ("●", "DueSoon"),
            _ => ("○", "TextSecondary")
        };
        _glyph.Text = glyph;
        _word.Text = Status;
        foreach (var label in new[] { _glyph, _word })
            label.SetAppThemeColor(Label.TextColorProperty, Theme.Color(token + "Light"), Theme.Color(token + "Dark"));
        SemanticProperties.SetDescription(this, $"Status: {Status}");
    }
}

/// <summary>Colour tokens from Colors.xaml for views built in code.</summary>
internal static class Theme
{
    public static Color Color(string key)
        => Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color ? color : Colors.Gray;
}
