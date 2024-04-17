using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using MockupServer.Storage;
using Serilog;
using Serilog.Events;
using Utility.Serilog.Extension;

namespace MockupServer.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();


            ServiceCollection services = new ServiceCollection();

            services.AddMockupBaseService();
            services.AddSingleton<Form1>();

            var serviceProvider = services.BuildServiceProvider();
            var form = serviceProvider.GetService<Form1>()!;
            Application.Run(form);
        }
    }
}