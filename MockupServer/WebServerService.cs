using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Configs;
using MockupServer.Utils;
using Serilog;
using Serilog.Events;

namespace MockupServer
{
    public class WebServerService
    {
        public static IHost Create(ServerOptions options)
        {
            try
            {
                var builder = Host.CreateDefaultBuilder(new string[]
                {
                        "--e ASPNETCORE_ENVIRONMENT=\"Development\"",
                        $"--urls=http://localhost:{options.Port};https://localhost:{options.Port+1};"
                });

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
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .WriteTo.Console()
                        .WriteTo.Async(a => a.File("logs/All-.txt", rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(
                        a => a.File("logs/Debug-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(
                        a => a.File("logs/Info-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Async(
                        a => a.File("logs/Warning-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(
                        a => a.File("logs/Error-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Async(
                        a => a.File("logs/Fatal-.txt", rollingInterval: RollingInterval.Day)
                    ));
                });

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ServerOptions>(options);
                });

                builder = builder.ConfigureWebHostDefaults(x =>
                {
                    x.UseKestrel().UseStartup<Startup>();
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
                var builder = Host.CreateDefaultBuilder(new string[]
                {
                        "--e ASPNETCORE_ENVIRONMENT=\"Development\"",
                        $"--urls=http://localhost:{ArgumentsUtils.GetValue(args, "Port")}"
                });
                builder.ConfigureAppConfiguration((config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddCommandLine(args);
                });

                builder.UseSerilog((context, config) =>
                {
                    //config.ReadFrom.Configuration(configuration);
                    config.Enrich.FromLogContext()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .WriteTo.Console()
                        .WriteTo.Async(a => a.File("logs/All-.txt", rollingInterval: RollingInterval.Day))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(
                        a => a.File("logs/Debug-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(
                        a => a.File("logs/Info-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Async(
                        a => a.File("logs/Warning-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(
                        a => a.File("logs/Error-.txt", rollingInterval: RollingInterval.Day)
                    ))
                    .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Async(
                        a => a.File("logs/Fatal-.txt", rollingInterval: RollingInterval.Day)
                    ));
                });

                builder = builder.ConfigureWebHostDefaults(x =>
                {
                    x.ConfigureServices((builderContext, services) =>
                    {
                        var options = builderContext.Configuration.Get<ServerOptions>();
                        services.AddSingleton<ServerOptions>(options);
                    });
                    x.UseKestrel().UseStartup<Startup>();
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
    }
}
