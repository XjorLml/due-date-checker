using Microsoft.Extensions.Logging;

using DueDateChecker.App.Pages;
using DueDateChecker.App.Services;
using DueDateChecker.App.ViewModels;
using DueDateChecker.Core.Data;
using DueDateChecker.Core.Domain;

namespace DueDateChecker.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		ConfigureOutlinedFields();
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
				fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
				fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemibold");
				fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
			});

#if ANDROID
		builder.ConfigureMauiHandlers(handlers =>
			handlers.AddHandler(typeof(Shell), typeof(Platforms.Android.DuewiseShellRenderer)));
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "dues.db");
		builder.Services.AddSingleton<IAppPreferencesStorage, MauiAppPreferencesStorage>();
		builder.Services.AddSingleton<AppPreferencesService>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddSingleton(new DatabaseInitializer(databasePath));
		builder.Services.AddSingleton(new SqliteDueRepository(databasePath));
        builder.Services.AddSingleton<IReminderStore>(new SqliteReminderStore(databasePath));
        builder.Services.AddSingleton<IReminderPlatform, Platforms.Android.AndroidReminderPlatform>();
        builder.Services.AddSingleton<ReminderCoordinator>(services => new(
            services.GetRequiredService<DatabaseDispatcher>(), services.GetRequiredService<SqliteDueRepository>(),
            services.GetRequiredService<IReminderStore>(), services.GetRequiredService<IReminderPlatform>(), services.GetRequiredService<TimeProvider>()));
        builder.Services.AddSingleton<IDueRepository>(services => new ReminderAwareDueRepository(
            services.GetRequiredService<SqliteDueRepository>(), services.GetRequiredService<ReminderCoordinator>()));
        builder.Services.AddSingleton<ReminderTapRouter>();
        builder.Services.AddTransient<ReminderSettingsViewModel>();
        builder.Services.AddTransient<ReminderSettingsPage>();
		builder.Services.AddSingleton<DatabaseDispatcher>();
		builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
		builder.Services.AddTransient<DuesListViewModel>();
		builder.Services.AddTransient<DueEditViewModel>();
		builder.Services.AddTransient<DueDetailViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<CalendarViewModel>();
		builder.Services.AddTransient<DuesListPage>();
		builder.Services.AddTransient<DueEditPage>();
		builder.Services.AddTransient<DueDetailPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<CalendarPage>();
		builder.Services.AddTransient<AppShell>();

		return builder.Build();
	}

	private static void ConfigureOutlinedFields()
	{
#if ANDROID
		// FieldBorder owns focus/error outlines; remove the native underline inside it.
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
			handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
		Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
			handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
		Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
			handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
		Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
			handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
#endif
	}
}
