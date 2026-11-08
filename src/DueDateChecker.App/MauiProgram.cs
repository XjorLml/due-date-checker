using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

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
		// UX05: the system save/open dialogs answer through the activity result.
		builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android.OnActivityResult(
			(_, requestCode, resultCode, data) => Platforms.Android.AndroidDueFileService.OnActivityResult(requestCode, resultCode, data))));
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
        builder.Services.AddSingleton<ReminderPaymentService>();
        builder.Services.AddSingleton<IDueArchiveStore>(new SqliteDueArchive(databasePath));
        builder.Services.AddSingleton<IDueFileService, Platforms.Android.AndroidDueFileService>();
        builder.Services.AddTransient<DataTransferViewModel>();
        builder.Services.AddSingleton<IMessageHost, Platforms.Android.AndroidMessageHost>();
        builder.Services.AddSingleton<DeletionUndo>();
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

		// UX07: the editor's "dd MMM yyyy" uses the same invariant month names as every other date in the
		// app ("06 Nov 2026"); MAUI would format with the phone's language. Runs after MAUI's own update.
		foreach (var key in new[] { nameof(DatePicker.Date), nameof(DatePicker.Format) })
		{
			Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(key, (handler, view) =>
			{
				if (view.Date is { } date)
					handler.PlatformView.Text = date.ToString(view.Format, System.Globalization.CultureInfo.InvariantCulture);
			});
		}

		// UX08: the amount keyboard offers digits and a decimal point only. MAUI's Numeric keyboard also
		// sets the "signed" flag, which adds a minus key although amounts cannot be negative. MAUI rebuilds
		// the input type when these properties change, so the flag is removed after each of them.
		foreach (var key in new[] { nameof(Entry.Keyboard), nameof(Entry.IsPassword), nameof(Entry.IsTextPredictionEnabled), nameof(Entry.IsSpellCheckEnabled), nameof(Entry.IsReadOnly) })
		{
			Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(key, (handler, view) =>
			{
				if (view.Keyboard == Keyboard.Numeric)
					handler.PlatformView.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
			});
		}
#endif
	}
}
