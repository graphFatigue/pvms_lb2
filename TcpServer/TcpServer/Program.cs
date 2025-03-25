using TcpServer.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<TcpServerService>();
    });

await builder.Build().RunAsync();
