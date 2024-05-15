using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockupServer.LocalDataSource;
using MockupServer.Models;
using Serilog;
using Serilog.Events;
using System.Reflection.PortableExecutable;
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

                builder.ConfigureServices(services =>
                {
                    services.Configure<KestrelServerOptions>(o =>
                    {
                        o.AllowSynchronousIO = true;
                    });

                    services.AddMockupBaseService();
                    services.AddMockupServerService();

                    services.AddSingleton<GlobalCache>(options);

                    services.AddCors(options =>
                    {
                        options.AddPolicy("cors", v =>
                        {
                            v.SetIsOriginAllowed(_ => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("Content-Disposition");
                        });
                    });
                });

                builder.UseSerilog(Log.Logger);

                builder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(new string[]
                    {
                        $"http://*:{options.ListenerPort}",
                        $"https://*:{options.ListenerPort+1}"
                    });

                    webBuilder.Configure((IApplicationBuilder app) =>
                    {
                        app.UseSerilogRequestLogging(options =>
                        {
                            options.Logger = Log.Logger;
                            // Customize the message template
                            options.MessageTemplate = "Request {IP,18} | {HttpMethod,6:u} | {ResponseCode} | {FullPath}";

                            // Emit debug-level events instead of the defaults
                            options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;

                            // Attach additional properties to the request completion event
                            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                            {
                                diagnosticContext.Set("FullPath", httpContext.Request.GetEncodedUrl().ToString());
                                diagnosticContext.Set("HttpMethod", httpContext.Request.Method);
                                diagnosticContext.Set("IP", httpContext.Connection.RemoteIpAddress?.ToString());
                                diagnosticContext.Set("ResponseCode", httpContext.Response.StatusCode);
                            };
                        });

                        app.UseCors("cors");

                        app.Run(async (context) =>
                        {
                            try
                            {
                                var logger = context.RequestServices.GetService<ILogger<WebServerService>>();
                                logger!.LogInformation("request");

                                var dbService = context.RequestServices.GetService<MockupService>()!;
                                var data = await dbService.GetResponseMessage(context);
                                if (data != null)
                                {
                                    foreach (var header in data.Content.Headers)
                                    {
                                        context.Response.Headers[header.Key] = header.Value.ToArray();
                                    }
                                    context.Response.StatusCode = (int)data.StatusCode;
                                    if (data.StatusCode != System.Net.HttpStatusCode.NotModified)
                                    {
                                        await data.Content.CopyToAsync(context.Response.Body);
                                        await context.Response.Body.FlushAsync();
                                    }
                                }
                                else
                                    context.Response.StatusCode = 204;
                            }
                            catch (Exception ex)
                            {
                                Log.Logger.Error(ex.ToString());
                            }
                        });

                    });


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
