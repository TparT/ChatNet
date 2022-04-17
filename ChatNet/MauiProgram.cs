using ChatNet.Data;
using ChatNet.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Sockets.Plugin;

namespace ChatNet;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddBlazorWebView();
		builder.Services.AddSingleton<WeatherForecastService>();

        //builder.Services.AddScoped(_ => new ServerConnectionClient());
        //builder.Services.AddScoped(_ => App.GetService<SocketListener>());
        builder.Services.AddScoped(_ => App.GetService<TcpSocketListener>());
        builder.Services.AddScoped(_ => App.GetService<NetworkingService>());
        builder.Services.AddScoped(_ => new AudioPlayerManager());

        foreach (ServiceDescriptor service in builder.Services)
			DependencyService.RegisterSingleton(service.ServiceType);

        //builder.Services.AddScoped(_ => new TcpConnection(IPAddress.Parse("10.0.0.16"), 6969, new AutoResetEvent(false), false));
		builder
			.RegisterBlazorMauiWebView()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});



		return builder.Build();
	}
}
