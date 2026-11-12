using Microsoft.Extensions.Logging;
using Moonlight.App.Data;
using Moonlight.App.ViewModels;
using Moonlight.App.Views;

#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace Moonlight.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if MAUI_DEVFLOW
		builder.AddMauiDevFlowAgent();
#endif

		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "moonlight.db3");
		builder.Services.AddSingleton(_ => new SQLiteDatabase(databasePath));
		builder.Services.AddTransient<RemindersViewModel>();
		builder.Services.AddTransient<RemindersView>();
		builder.Services.AddTransient<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
