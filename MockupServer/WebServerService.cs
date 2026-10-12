using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MockupServer.LocalDataSource;
using MockupServer.Models;
using Serilog;
using Serilog.Events;
using System.Net;

namespace MockupServer
{
    public class WebServerService
    {
        /// <summary>逐跳头，不应转发给客户端</summary>
        static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade",
        };

        public static IHost Create(GlobalCache options)
        {
            var builder = Host.CreateDefaultBuilder();

            builder.UseEnvironment(Environments.Staging);

            builder.ConfigureServices(services =>
            {
                services.Configure<KestrelServerOptions>(o =>
                {
                    o.AllowSynchronousIO = true;
                });

                services.AddMockupBaseService();
                services.AddMockupServerService();

                services.AddCors(options =>
                {
                    options.AddPolicy("cors", v =>
                    {
                        v.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders(HeaderNames.ContentDisposition);
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
                        options.MessageTemplate = "Request {IP,18} | {HttpMethod,6:u} | {ResponseCode} | {FullPath}";
                        options.GetLevel = (httpContext, elapsed, ex) => ex != null ? LogEventLevel.Error : LogEventLevel.Information;
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
                        HttpResponseMessage? data = null;
                        try
                        {
                            var mockupService = context.RequestServices.GetRequiredService<MockupService>();
                            data = await mockupService.GetResponseMessage(context);

                            foreach (var header in data.Headers)
                            {
                                if (!HopByHopHeaders.Contains(header.Key))
                                    context.Response.Headers[header.Key] = header.Value.ToArray();
                            }
                            if (data.Content != null)
                            {
                                foreach (var header in data.Content.Headers)
                                {
                                    if (!HopByHopHeaders.Contains(header.Key))
                                        context.Response.Headers[header.Key] = header.Value.ToArray();
                                }
                            }

                            context.Response.StatusCode = (int)data.StatusCode;
                            if (data.StatusCode != HttpStatusCode.NotModified && data.Content != null)
                            {
                                await data.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
                                await context.Response.Body.FlushAsync(context.RequestAborted);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error(ex, "请求处理失败");
                            if (!context.Response.HasStarted)
                                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        }
                        finally
                        {
                            data?.Dispose();
                        }
                    });
                });
            });

            return builder.Build();
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

        public static async Task StopServerAsync(this IHost? host, CancellationToken cancellationToken = default)
        {
            if (host == null)
                return;

            try
            {
                await host.StopAsync(cancellationToken);
            }
            finally
            {
                host.Dispose();
                State.IsRunning = false;
            }
        }
    }
}
