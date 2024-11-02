using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Luxoria.Modules;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using Luxoria.Core.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public Window Window => m_window;
        private MainWindow m_window;

        private readonly Startup _startup;
        private readonly IHost _host;
        private readonly ILoggerService _logger;
        
        private readonly IModuleService _moduleService;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            _startup = new Startup();
            _host = CreateHostBuilder(_startup).Build();
            _moduleService = _host.Services.GetRequiredService<IModuleService>();
            _logger = _host.Services.GetRequiredService<ILoggerService>();
        }

        public static IHostBuilder CreateHostBuilder(Startup startup)
        {
            return Host.CreateDefaultBuilder().ConfigureServices((context, services) => startup.ConfigureServices(context, services));
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);
            // You can also log to a file or any other logging mechanism
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _logger.Log("Application is starting...");

            // Show splash screen
            var splashScreen = new SplashScreen();
            splashScreen.Activate();

            _logger.Log("Modules loaded. Closing slasph screen...");
            await Task.Delay(500);

            // Load modules asynchronously and update the splash screen with the module names
            await LoadModulesAsync(splashScreen);

            // Close the splash screen after loading modules
            splashScreen.DispatcherQueue.TryEnqueue(() =>
            {
                splashScreen.Close();
            });

            var eventBus = _host.Services.GetRequiredService<IEventBus>();
            m_window = new MainWindow(eventBus);
            m_window.Activate();
        }

        private async Task LoadModulesAsync(SplashScreen splashScreen)
        {
            using (var scope = _host.Services.CreateScope())
            {
                string modulesPath = Path.Combine(AppContext.BaseDirectory, "modules");

                // Check if the modules directory exists
                if (!Directory.Exists(modulesPath))
                {
                    _logger.Log($"Modules directory not found: {modulesPath}", "General", LogLevel.Warning);

                    // Create the modules directory if it doesn't exist
                    Directory.CreateDirectory(modulesPath);

                    _logger.Log($"Modules directory created: {modulesPath}");
                }

                // Get all module DLL files in the modules directory
                string[] moduleFiles = Directory.GetFiles(modulesPath, "*.dll");

                var loader = new ModuleLoader();

                foreach (string moduleFile in moduleFiles)
                {
                    string moduleName = Path.GetFileNameWithoutExtension(moduleFile);

                    _logger.Log("Trying to load : " + moduleName);

                    // Update the splash screen with the module name being loaded
                    splashScreen.DispatcherQueue.TryEnqueue(() =>
                    {
                        splashScreen.CurrentModuleTextBlock.Text = $"Loading {moduleName}...";
                    });

                    // Small delay to ensure the splash screen updates properly
                    await Task.Delay(300); // 0.3 second delay

                    try
                    {
                        // Load the module in a background thread
                        await Task.Run(() =>
                        {
                            IModule module = loader.LoadModule(moduleFile);
                            if (module != null)
                            {
                                // Display module information
                                _logger.Log($"Module loaded: {moduleName}");
                                _logger.Log($"Module name: {module.Name}");
                                _logger.Log($"Module version: {module.Version}");
                                _logger.Log($"Module description: {module.Description}");
                                // Save the module to ModuleService
                                _moduleService.AddModule(module);
                            }
                            else
                            {
                                _logger.Log($"No valid module found in: {moduleFile}", "General", LogLevel.Warning);
                            }
                        });
                    }
                    catch (FileNotFoundException ex)
                    {
                        _logger.Log($"File not found for module [{moduleFile}]: {ex.Message}", "General", LogLevel.Error);
                    }
                    catch (BadImageFormatException ex)
                    {
                        _logger.Log($"Invalid module file format for [{moduleFile}]: {ex.Message}", "General", LogLevel.Error);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Failed to load module [{moduleFile}]: {ex.Message}", "General", LogLevel.Error);
                    }
                }

                // Update the splash screen with the module name being loaded
                splashScreen.DispatcherQueue.TryEnqueue(() =>
                {
                    splashScreen.CurrentModuleTextBlock.Text = $"Initializing modules...";
                });

                // Small delay to ensure the splash screen updates properly
                await Task.Delay(300); // 0.3 second delay
                _moduleService.InitializeModules(new ModuleContext());
            }
        }
    }
}
