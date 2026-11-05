namespace DueDateChecker.App.Pages;

/// <summary>Single-choice segmented control. Bind <see cref="Options"/> to the labels and
/// <see cref="SelectedIndex"/> (two-way by default) to the chosen position; each segment is a real
/// Button so keyboard, switch and TalkBack users get a focusable, named control with a selected state.</summary>
public partial class SegmentedControl : ContentView
{
    public static readonly BindableProperty OptionsProperty = BindableProperty.Create(
        nameof(Options), typeof(IReadOnlyList<string>), typeof(SegmentedControl), null,
        propertyChanged: (bindable, _, _) => ((SegmentedControl)bindable).Rebuild());

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(SegmentedControl), -1, BindingMode.TwoWay,
        propertyChanged: (bindable, _, _) => ((SegmentedControl)bindable).UpdateStyles());

    private readonly List<Button> _segments = [];

    public SegmentedControl() => InitializeComponent();

    public IReadOnlyList<string>? Options
    {
        get => (IReadOnlyList<string>?)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private void Rebuild()
    {
        Host.Children.Clear();
        Host.ColumnDefinitions.Clear();
        _segments.Clear();
        var options = Options ?? [];
        for (var index = 0; index < options.Count; index++)
        {
            var position = index;
            Host.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            var button = new Button { Text = options[index], AutomationId = $"{AutomationId}-{options[index]}" };
            button.Clicked += (_, _) => SelectedIndex = position;
            _segments.Add(button);
            Host.Add(button, index, 0);
        }
        UpdateStyles();
    }

    private void UpdateStyles()
    {
        var resources = Application.Current?.Resources;
        if (resources is null) return;
        for (var index = 0; index < _segments.Count; index++)
        {
            var selected = index == SelectedIndex;
            _segments[index].Style = (Style)resources[selected ? "SegmentButtonSelected" : "SegmentButton"];
            SemanticProperties.SetDescription(_segments[index], $"{_segments[index].Text}, {(selected ? "selected" : "not selected")}");
        }
    }
}
