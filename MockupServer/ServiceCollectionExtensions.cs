using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using MockupServer.LocalDataSource;
using MockupServer.Storage;
using Serilog;
using Serilog.Events;
using Utility.Serilog.Extension;

namespace MockupServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockupBaseService(this IServiceCollection services)
        {
            var instance = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=database.db")
                .UseAutoSyncStructure(true)
                .Build();

            services.AddSingleton<IFreeSql>(instance);
            services.AddScoped<DataService>();
            var _logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .CustomWriteTo()
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger: _logger, dispose: true);
            });
            return services;
        }

        public static IServiceCollection AddMockupServerService(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<MockupService>();

            return services;
        }
    }
}
