using Luxoria.App.Core.Interfaces;
using Luxoria.Core.LMGUI;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Luxoria.App
{
    public partial class App : Application
    {
        public Window Window => m_window;
        private MainWindow m_window;

        private readonly IHost _host;
        private readonly ILoggerService _logger;
        private readonly IModuleService _moduleService;
        private readonly GuiRenderer _guiRenderer;

        public App()
        {
            InitializeComponent();
            _host = CreateHostBuilder().Build();

            // Retrieve services
            _logger = _host.Services.GetRequiredService<ILoggerService>();
            _moduleService = _host.Services.GetRequiredService<IModuleService>();
            _guiRenderer = _host.Services.GetRequiredService<GuiRenderer>();
        }

        public static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var startup = new Startup();
                    startup.ConfigureServices(context, services);
                });
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            _logger.Log("Application is starting...");

            // Show splash screen
            var splashScreen = new SplashScreen();
            splashScreen.Activate();

            // Load modules and update splash screen
            await LoadModulesAsync(splashScreen);

            splashScreen.DispatcherQueue.TryEnqueue(splashScreen.Close);

            // Resolve MainWindow via DI
            m_window = _host.Services.GetRequiredService<MainWindow>();
            m_window.Activate();

            // Initialize GuiRenderer
            //_guiRenderer.Initialize(m_window);
        }

        private async Task LoadModulesAsync(SplashScreen splashScreen)
        {
            string modulesPath = GetOrCreateModulesDirectory();
            var loader = new ModuleLoader();

            string[] moduleFolders = Directory.GetDirectories(modulesPath);

            foreach (string moduleFolder in moduleFolders)
            {
                await LoadModulesFromFolderAsync(moduleFolder, loader, splashScreen);
            }

            await UpdateSplashScreenAsync(splashScreen, "Initializing modules...");
            _moduleService.InitializeModules(_guiRenderer, new ModuleContext());
        }

        private string GetOrCreateModulesDirectory()
        {
            string modulesPath = Path.Combine(AppContext.BaseDirectory, "modules");

            if (!Directory.Exists(modulesPath))
            {
                _logger.Log($"Modules directory not found: {modulesPath}", "General", LogLevel.Warning);
                Directory.CreateDirectory(modulesPath);
                _logger.Log($"Modules directory created: {modulesPath}");
            }

            return modulesPath;
        }

        private async Task LoadModulesFromFolderAsync(string moduleFolder, ModuleLoader loader, SplashScreen splashScreen)
        {
            string[] moduleFiles = Directory.GetFiles(moduleFolder, "*.Lux.dll");

            if (moduleFiles.Length == 0)
            {
                _logger.Log($"No module DLL files found in: {moduleFolder}", "General", LogLevel.Warning);
                return;
            }

            foreach (string moduleFile in moduleFiles)
            {
                string moduleName = Path.GetFileNameWithoutExtension(moduleFile);
                await LoadModuleAsync(moduleFile, moduleName, loader, splashScreen);
            }
        }

        private async Task LoadModuleAsync(string moduleFile, string moduleName, ModuleLoader loader, SplashScreen splashScreen)
        {
            await UpdateSplashScreenAsync(splashScreen, $"Loading {moduleName}...");
            _logger.Log($"Trying to load: {moduleName}");

            try
            {
                IModule module = await Task.Run(() => loader.LoadModule(moduleFile));
                if (module != null)
                {
                    _logger.Log($"Module loaded: {moduleName}");
                    _moduleService.AddModule(module);
                }
                else
                {
                    _logger.Log($"No valid module found in: {moduleFile}", "General", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to load module [{moduleFile}]: {ex.Message}", "General", LogLevel.Error);
            }
        }

        private static async Task UpdateSplashScreenAsync(SplashScreen splashScreen, string message)
        {
            splashScreen.DispatcherQueue.TryEnqueue(() =>
            {
                splashScreen.CurrentModuleTextBlock.Text = message;
            });

            await Task.Delay(300);
        }
    }
}
