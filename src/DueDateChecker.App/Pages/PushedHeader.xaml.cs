namespace DueDateChecker.App.Pages;

public partial class PushedHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(PushedHeader), "");
    public static readonly BindableProperty ShowBackProperty = BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PushedHeader), true);
    public static readonly BindableProperty ActionKindProperty = BindableProperty.Create(
        nameof(ActionKind), typeof(string), typeof(PushedHeader), "", propertyChanged: (view, _, _) => ((PushedHeader)view).UpdateAction());
    public static readonly BindableProperty ActionDescriptionProperty = BindableProperty.Create(nameof(ActionDescription), typeof(string), typeof(PushedHeader), "");
    public static readonly BindableProperty ActionVisibleProperty = BindableProperty.Create(
        nameof(ActionVisible), typeof(bool), typeof(PushedHeader), true, propertyChanged: (view, _, _) => ((PushedHeader)view).UpdateAction());
    public static readonly BindableProperty ActionEnabledProperty = BindableProperty.Create(
        nameof(ActionEnabled), typeof(bool), typeof(PushedHeader), true, propertyChanged: (view, _, _) => ((PushedHeader)view).UpdateAction());
    public static readonly BindableProperty ActionIsDestructiveProperty = BindableProperty.Create(
        nameof(ActionIsDestructive), typeof(bool), typeof(PushedHeader), false, propertyChanged: (view, _, _) => ((PushedHeader)view).UpdateAction());
    public static readonly BindableProperty HasActionProperty = BindableProperty.Create(nameof(HasAction), typeof(bool), typeof(PushedHeader), false);

    public PushedHeader()
    {
        InitializeComponent();
        UpdateAction();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public bool ShowBack { get => (bool)GetValue(ShowBackProperty); set => SetValue(ShowBackProperty, value); }
    /// <summary>A <see cref="Glyph"/> name for the trailing action, such as "edit" or "trash"; empty hides it.</summary>
    public string ActionKind { get => (string)GetValue(ActionKindProperty); set => SetValue(ActionKindProperty, value); }
    public string ActionDescription { get => (string)GetValue(ActionDescriptionProperty); set => SetValue(ActionDescriptionProperty, value); }
    public bool ActionVisible { get => (bool)GetValue(ActionVisibleProperty); set => SetValue(ActionVisibleProperty, value); }
    public bool ActionEnabled { get => (bool)GetValue(ActionEnabledProperty); set => SetValue(ActionEnabledProperty, value); }
    /// <summary>Draws the action as the prototype's outlined red bin button.</summary>
    public bool ActionIsDestructive { get => (bool)GetValue(ActionIsDestructiveProperty); set => SetValue(ActionIsDestructiveProperty, value); }
    public bool HasAction { get => (bool)GetValue(HasActionProperty); private set => SetValue(HasActionProperty, value); }

    /// <summary>Header Back. When nobody handles it, the page is popped like system Back.</summary>
    public event EventHandler? BackClicked;
    public event EventHandler? ActionClicked;

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (BackClicked is not null) { BackClicked(this, EventArgs.Empty); return; }
        try { await Shell.Current.GoToAsync(".."); }
        catch (Exception) { } // already leaving or nothing to pop
    }

    private void OnActionClicked(object? sender, EventArgs e) => ActionClicked?.Invoke(this, EventArgs.Empty);

    private void UpdateAction()
    {
        HasAction = ActionVisible && !string.IsNullOrEmpty(ActionKind);
        if (ActionGlyph is null) return;
        var (light, dark) = ActionIsDestructive ? ("DestructiveLight", "DestructiveDark") : ("TextLight", "TextDark");
        ActionGlyph.SetAppThemeColor(Glyph.ColorProperty, Theme.Color(light), Theme.Color(dark));
        ActionFrame.Opacity = ActionEnabled ? 1 : 0.4;
        if (ActionIsDestructive)
        {
            ActionFrame.StrokeThickness = 1;
            ActionFrame.SetAppThemeColor(Border.StrokeProperty, Theme.Color("BorderLight"), Theme.Color("BorderDark"));
        }
        else ActionFrame.StrokeThickness = 0;
    }
}
