namespace DueDateChecker.App;

public partial class AppShell : Shell
{
	private readonly Pages.DuesListPage _duesPage;

	public AppShell(Pages.DuesListPage duesPage)
	{
		InitializeComponent();
		_duesPage = duesPage;
		DuesContent.Content = duesPage;
		Routing.RegisterRoute(nameof(Pages.DueEditPage), typeof(Pages.DueEditPage));
	}

	public Task RefreshDuesAsync() => CurrentPage == _duesPage ? _duesPage.RefreshAsync() : Task.CompletedTask;
}
