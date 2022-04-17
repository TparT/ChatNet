using ChatNet.Services;
using Sockets.Plugin;

namespace ChatNet;

public partial class App : Application
{
    protected static IServiceProvider ServiceProvider { get; set; }

    void SetupServices()
    {
        var services = new ServiceCollection();
        //services.AddScoped(_ => new SocketListener());
        services.AddScoped(_ => new TcpSocketListener());
        services.AddScoped(_ => new NetworkingService());
        // TODO: Add core services here

        ServiceProvider = services.BuildServiceProvider();
    }

    public App()
    {
        InitializeComponent();

        SetupServices();

        MainPage = new MainPage();
    }

    public static T GetService<T>() where T : class
        => ServiceProvider.GetService<T>();
}
