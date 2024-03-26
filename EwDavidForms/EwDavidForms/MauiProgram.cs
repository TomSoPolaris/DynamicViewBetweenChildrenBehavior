
namespace EwDavidForms;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();
			//.UseMauiCompatibility(); // uncomment if using Compat StackLayout on BehaviorPage

		return builder.Build();
	}
}
