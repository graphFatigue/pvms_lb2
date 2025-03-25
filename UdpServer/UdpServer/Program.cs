using Microsoft.Extensions.Hosting;
using UdpServer.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<UdpServerService>();
    });

await builder.Build().RunAsync();