using Luxoria.Core;
using Luxoria.Core.Interfaces;
using Luxoria.Core.Services;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;

namespace Luxoria.App
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            Debug.WriteLine("Configuring services...");
            // Register services here

            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<IModuleService, ModuleService>();

            Debug.WriteLine("Services registered successfully !");
        }
    }
}
