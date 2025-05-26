using Luxoria.App.Interfaces;
using Luxoria.App.Services;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.LogTargets;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Luxoria.App.Logics
{
    public class Startup
    {
        private const string LOG_SECTION = "Startup";
        /// <summary>
        /// Configure services
        /// </summary>
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            LoggerService logger = new(LogLevel.Debug, new DebugLogTarget(), new FileLogTarget("log.txt"));
            logger.Log("Configuring services...", LOG_SECTION, LogLevel.Info);
            // Register services here

            // Register services from Luxoria.SDK (first)
            // Register Logger Service
            logger.Log("Registering Logger Service...", LOG_SECTION, LogLevel.Info);
            services.AddSingleton(logger as ILoggerService);
            logger.Log("Logger Service registered successfully !", LOG_SECTION, LogLevel.Info);

            // Register Vault Service
            logger.Log("Registering Vault Service...", LOG_SECTION, LogLevel.Info);
            services.AddSingleton<IVaultService>(new VaultService(logger));
            logger.Log("Vault Service registered successfully !", LOG_SECTION, LogLevel.Info);

            // Register Event Bus
            logger.Log("Registering Event Bus...", LOG_SECTION, LogLevel.Info);
            services.AddSingleton<IEventBus, EventBus>();
            logger.Log("Event Bus registered successfully !", LOG_SECTION, LogLevel.Info);

            // Register Module Loader
            logger.Log("Registering Module Loader...", LOG_SECTION, LogLevel.Info);
            services.AddSingleton<IModuleService, ModuleService>();
            logger.Log("Module Loader registered successfully !", LOG_SECTION, LogLevel.Info);

            // Register ModuleUI Service
            logger.Log("Registering IModuleUI...", LOG_SECTION, LogLevel.Info);
            services.AddSingleton<IModuleUIService, ModuleUIService>();
            logger.Log("IModuleUI registered successfully !", LOG_SECTION, LogLevel.Info);

            logger.Log("Services registered successfully !", LOG_SECTION, LogLevel.Info);
        }
    }
}
