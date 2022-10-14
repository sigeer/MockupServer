using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using System.Text;

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
                        $"--urls=http://localhost:{options.Port}"
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
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("Quartz", LogEventLevel.Information)

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
    }

    public static class RequestBody
    {
        public static HttpRequestMessage GetHttpRequestMessage(HttpContext context, string totalUrl)
        {
            var requestMsg = new HttpRequestMessageFeature(context).HttpRequestMessage;
            requestMsg.RequestUri = new Uri(totalUrl);
            if (requestMsg.Content != null)
                requestMsg.Content = requestMsg.Content.Headers.ContentLength == 0 ? null : requestMsg.Content;
            //requestMsg.Headers.Remove("Accept");
            //requestMsg.Headers.Remove("Connection");
            //requestMsg.Headers.Remove("Host");
            //requestMsg.Headers.Remove("User-Agent");
            requestMsg.Headers.Remove("Accept-Encoding");
            requestMsg.Headers.Remove("Accept-Language");
            //requestMsg.Headers.Remove("Origin");
            //requestMsg.Headers.Remove("Referer");
            return requestMsg;
        }
    }
}
