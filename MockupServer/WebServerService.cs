using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Models;
using Serilog;
using Serilog.Events;
using Utility.Common;
using Utility.Serilog.Extension;

namespace MockupServer
{
    public class WebServerService
    {
        public static IHost Create(GlobalCache options)
        {
            try
            {
                State.Port = options.ListenerPort;
                var builder = Host.CreateDefaultBuilder();

                builder.UseEnvironment(Environments.Staging);
                builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                });

                builder.UseSerilog((context, config) =>
                {
                    //config.ReadFrom.Configuration(configuration);
                    config.Enrich.FromLogContext()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .CustomWriteTo();
                });

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<GlobalCache>(options);
                });

                builder = builder.ConfigureWebHostDefaults(x =>
                {
                    x.UseUrls(new string[]
                    {
                        $"http://*:{options.ListenerPort}",
                        $"https://*:{options.ListenerPort+1}"
                    }).UseStartup<Startup>();
                });


                var app = builder.Build();

                return app;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public static IHost Create(string[] args)
        {
            try
            {
                var options = new GlobalCache()
                {
                    ListenerPort = int.Parse(ArgumentsUtils.GetValue(args, "Port")),
                };
                return Create(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }

    public static class IHostExtension
    {
        public static async Task StartServerAsync(this IHost? host, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(host);
            await host.StartAsync(cancellationToken);
            State.IsRunning = true;
        }

        public static async Task StopServerAysnc(this IHost? host, CancellationToken cancellationToken = default)
        {
            if (host == null)
                return;

            await host.StopAsync(cancellationToken);
            host.Dispose();
            State.IsRunning = false;
        }
    }

}
