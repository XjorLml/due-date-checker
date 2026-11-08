using Android.App;
using Android.Runtime;

namespace DueDateChecker.App;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public static IServiceProvider? ReminderServices { get; private set; }
    protected override MauiApp CreateMauiApp()
    {
        var app = MauiProgram.CreateMauiApp(); ReminderServices = app.Services; return app;
    }
}
