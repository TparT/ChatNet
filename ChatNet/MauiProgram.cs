using Microsoft.AspNetCore.Components.WebView.Maui;
using ChatNet.Data;
using ChatNet.Services;
using System.Net;

namespace ChatNet;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.RegisterBlazorMauiWebView()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddBlazorWebView();
		builder.Services.AddSingleton<WeatherForecastService>();

        builder.Services.AddScoped(_ => new ServerConnectionClient());
        //builder.Services.AddScoped(_ => new TcpConnection(IPAddress.Parse("10.0.0.16"), 6969, new AutoResetEvent(false), false));


		return builder.Build();
	}
}
