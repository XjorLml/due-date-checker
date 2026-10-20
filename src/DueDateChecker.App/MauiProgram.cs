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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "dues.db");
		builder.Services.AddSingleton(new DatabaseInitializer(databasePath));
		builder.Services.AddSingleton<IDueRepository>(new SqliteDueRepository(databasePath));
		builder.Services.AddSingleton<DatabaseDispatcher>();
		builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
		builder.Services.AddTransient<DuesListViewModel>();
		builder.Services.AddTransient<DueEditViewModel>();
		builder.Services.AddTransient<DuesListPage>();
		builder.Services.AddTransient<DueEditPage>();
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
		Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("DuewiseOutline", (handler, _) =>
			handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent));
#endif
	}
}
