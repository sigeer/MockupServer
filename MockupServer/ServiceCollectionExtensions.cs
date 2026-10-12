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
        // UI 与 Host 两个容器共享同一 FreeSql 实例，避免并发连接同一 SQLite 文件
        static readonly Lazy<IFreeSql> SharedFreeSql = new(() =>
        {
            var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=database.db")
                .UseAutoSyncStructure(true)
                .Build();

            // 连接级 busy_timeout（不落盘），缓解并发写锁等待失败
            fsql.Ado.ExecuteNonQuery("PRAGMA busy_timeout=5000;");
            return fsql;
        });

        public static IServiceCollection AddMockupBaseService(this IServiceCollection services)
        {
            services.AddSingleton<IFreeSql>(SharedFreeSql.Value);
            services.AddScoped<DataService>();

            return services;
        }

        public static IServiceCollection AddLogService(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .CustomWriteTo()
                    .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger: Log.Logger, dispose: false);
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
