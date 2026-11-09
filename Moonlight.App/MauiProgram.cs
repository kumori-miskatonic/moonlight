using Microsoft.Extensions.Logging;
using Moonlight.App.Data;
using Moonlight.App.ViewModels;

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

		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "moonlight.db3");
		builder.Services.AddSingleton(_ => new SQLiteDatabase(databasePath));
		builder.Services.AddTransient<RemindersViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
