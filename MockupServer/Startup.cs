using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using Serilog;
using Serilog.Events;

namespace MockupServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(o =>
              {
                  o.AllowSynchronousIO = true;
              });
            services.AddLogging();
            services.AddSingleton<Http.HttpClientPool>();
            var instance = new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, "data source=database.db").Build();
            instance.Ado.ExecuteScalar("""
                CREATE TABLE If Not Exists "MockupObject" (
                  "RequestUrl" varchar(100),
                  "ResponseData" varchar(1000)
                );
                """);
            services.AddSingleton<IFreeSql>(instance);
            services.AddScoped<MockupService>();
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
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
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
                    var dbService = context.RequestServices.GetService<MockupService>()!;
                    var option = context.RequestServices.GetService<ServerOptions>()!;
                    var url = $"{context.Request.Scheme}://{option.OriginalServiceUrl}{context.Request.Path}{context.Request.QueryString}";
                    var relativeUrl = $"{context.Request.Path}{context.Request.QueryString}";
                    var data = await dbService.SendObject(await RequestBody.GetHttpRequestMessage(context, url), option.OriginalServiceUrl, relativeUrl);
                    if (data != null)
                    {
                        await data.Content.CopyToAsync(context.Response.Body);
                        await context.Response.Body.FlushAsync();
                    }
                    else
                        context.Response.StatusCode = 204;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.ToString());
                    context.Response.StatusCode = 204;
                }
            });
        }
    }

    public static class RequestBody
    {
        public static async Task<HttpRequestMessage> GetHttpRequestMessage(HttpContext context, string totalUrl)
        {
            var requestMsg = new HttpRequestMessageFeature(context).HttpRequestMessage;
            requestMsg.RequestUri = new Uri(totalUrl);
            //if (requestMsg.Content != null)
            //    requestMsg.Content = requestMsg.Content.Headers.ContentLength == 0 ? null : requestMsg.Content;
            //requestMsg.Headers.Remove("Accept");
            //requestMsg.Headers.Remove("Connection");
            //requestMsg.Headers.Remove("Host");
            //requestMsg.Headers.Remove("User-Agent");
            requestMsg.Headers.Remove("Accept-Encoding");
            requestMsg.Headers.Remove("Accept-Language");
            //requestMsg.Headers.Remove("Origin");
            //requestMsg.Headers.Remove("Referer");
            return await CloneHttpRequestMessageAsync(requestMsg);
        }
        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            HttpRequestMessage copyOfRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            // Copy the request's content (via a MemoryStream) into the cloned object
            var ms = new MemoryStream();
            if (request.Content != null)
            {
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                copyOfRequest.Content = new StreamContent(ms);

                // Copy the content headers
                if (request.Content.Headers != null)
                {
                    foreach (var h in request.Content.Headers)
                    {
                        copyOfRequest.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }

            copyOfRequest.Version = request.Version;

            foreach (KeyValuePair<string, object> prop in request.Options)
            {
                copyOfRequest.Options.TryAdd(prop.Key, prop.Value);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                copyOfRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return copyOfRequest;
        }
    }
}
