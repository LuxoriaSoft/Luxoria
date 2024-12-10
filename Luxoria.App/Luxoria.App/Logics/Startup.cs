using Luxoria.Core.LMGUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Luxoria.App.Core.Interfaces;
using Luxoria.App.Core;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Services;
using Luxoria.SDK.Models;

namespace Luxoria.App
{
    public class Startup
    {
        private const string LOG_SECTION = "Startup";

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            ILoggerService logger = new LoggerService();
            logger.Log("Configuring services...", LOG_SECTION, LogLevel.Info);

            // Register Event Bus
            services.AddSingleton<IEventBus, EventBus>();

            // Register Module Service
            services.AddSingleton<IModuleService, ModuleService>();

            // Register Logger Service
            services.AddSingleton(logger);

            // Register GuiRenderer
            services.AddSingleton<GuiRenderer>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            logger.Log("Services registered successfully!", LOG_SECTION, LogLevel.Info);
        }

        public void Configure(HostBuilderContext context, IHostBuilder appBuilder)
        {
            appBuilder.ConfigureServices(ConfigureServices);
        }
    }
}
