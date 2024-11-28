using Luxoria.Core.Interfaces;
using Luxoria.Core.Services;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Luxoria.App
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            ILoggerService logger = new LoggerService();
            logger.Log("Configuring services...", "Startup", LogLevel.Info);
            // Register services here

            // Register services from Luxoria.Core
            
            // Register Event Bus
            logger.Log("Registering Event Bus...", "Startup", LogLevel.Info);
            services.AddSingleton<IEventBus, EventBus>();
            logger.Log("Event Bus registered successfully !", "Startup", LogLevel.Info);

            // Register Module Loader
            logger.Log("Registering Module Loader...", "Startup", LogLevel.Info);
            services.AddSingleton<IModuleService, ModuleService>();
            logger.Log("Module Loader registered successfully !", "Startup", LogLevel.Info);

            // Register Logger Service
            logger.Log("Registering Logger Service...", "Startup", LogLevel.Info);
            services.AddSingleton(logger);
            logger.Log("Logger Service registered successfully !", "Startup", LogLevel.Info);

            logger.Log("Services registered successfully !", "Startup", LogLevel.Info);
        }
    }
}
