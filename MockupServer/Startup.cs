using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockupServer.Configs;
using MockupServer.LocalDataSource;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
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
            services.AddScoped<IMongoClient>(x => new MongoClient(Configuration["MongoDB"]));
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
                    var data = await dbService.SendObject(RequestBody.GetHttpRequestMessage(context, url), option.OriginalServiceUrl, relativeUrl);
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
}
