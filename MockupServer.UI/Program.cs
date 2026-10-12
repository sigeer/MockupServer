using Microsoft.Extensions.DependencyInjection;

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

            var services = new ServiceCollection();
            services.AddMockupBaseService();
            services.AddLogService();
            services.AddSingleton<Form1>();

            using var serviceProvider = services.BuildServiceProvider();
            var form = serviceProvider.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}
