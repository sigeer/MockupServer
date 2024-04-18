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
            var instance = new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, "data source=database.db").Build();
            instance.Ado.ExecuteScalar("""
                CREATE TABLE If Not Exists "MockupObject" (
                  "RequestUrl" varchar(200) not null COLLATE NOCASE,
                  "Method" varchar(20) not null COLLATE NOCASE,
                  "ResponseType" int not null,
                  "ResponseData" Text
                );
                """);
            services.AddSingleton<IFreeSql>(instance);
            services.AddScoped<DataService>();
            var _logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
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
