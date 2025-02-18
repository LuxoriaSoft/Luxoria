using Luxoria.App.EventHandlers;
using Luxoria.App.Interfaces;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Luxoria.App
{
    public sealed partial class MainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _loggerService;
        private readonly IModuleService _moduleService;
        private readonly IModuleUIService _uiService;

        // Event handlers
        private readonly ImageUpdatedHandler _imageUpdatedHandler;
        private readonly CollectionUpdatedHandler _collectionUpdatedHandler;

        public MainWindow(IEventBus eventBus, ILoggerService loggerService, IModuleService moduleService, IModuleUIService uiService)
        {
            InitializeComponent();

            _eventBus = eventBus;
            _loggerService = loggerService;
            _moduleService = moduleService;
            _uiService = uiService;

            _imageUpdatedHandler = new ImageUpdatedHandler(_loggerService);
            _collectionUpdatedHandler = new CollectionUpdatedHandler(_loggerService);

            InitializeEventBus();

            LoadComponents();
        }

        private void InitializeEventBus()
        {
            _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);
            _eventBus.Subscribe<RequestWindowHandleEvent>(OnRequestWindowHandle);
        }

        private async Task ShowModalAsync(UIElement content, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot
            };

            dialog.Closed += (_, _) => dialog.Content = null; // Prevent reuse issues

            await dialog.ShowAsync();
        }

        private void LoadComponents()
        {
            foreach (var item in _moduleService.GetModules().OfType<IModuleUI>().SelectMany(m => m.Items))
            {
                _loggerService.Log($"[x] Loading: {item.Name} components ({item.SmartButtons.Count} items)");

                void HandleButtonClick()
                {
                    _ = HandleButtonClickAsync(item);
                }

                if (item.IsLeftLocated)
                {
                    MainMenu.AddLeftButton(item.Name, HandleButtonClick);
                }
                else
                {
                    MainMenu.AddRightButton(item.Name, HandleButtonClick);
                }
            }
        }

        private async Task HandleButtonClickAsync(ILuxMenuBarItem item)
        {
            var button = item.IsLeftLocated ? MainMenu.GetLeftButton(item.Name) : MainMenu.GetRightButton(item.Name);
            if (button == null)
            {
                Debug.WriteLine($"[Warning] Button not found for item: {item.Name}");
                return;
            }

            if (item.SmartButtons.Count > 1)
            {
                AttachFlyoutMenu(button, item);
            }
            else
            {
                await HandleSmartButtonClick((SmartButton)item.SmartButtons.First());
            }
        }

        private void AttachFlyoutMenu(UIElement button, ILuxMenuBarItem item)
        {
            if (button is not FrameworkElement frameworkElement) return;

            var flyout = new MenuFlyout();

            foreach (var smartButton in item.SmartButtons.Cast<SmartButton>())
            {
                var flyoutItem = new MenuFlyoutItem { Text = smartButton.Name };
                flyoutItem.Click += async (_, __) => await HandleSmartButtonClick(smartButton);
                flyout.Items.Add(flyoutItem);
            }

            FlyoutBase.SetAttachedFlyout(frameworkElement, flyout);
            FlyoutBase.ShowAttachedFlyout(frameworkElement);
        }

        private async Task HandleSmartButtonClick(SmartButton smartButton)
        {
            foreach (var (key, value) in smartButton.Pages)
            {
                if (value == null)
                {
                    Debug.WriteLine($"[Warning] Page is null for type {key}");
                    continue;
                }

                switch (key)
                {
                    case SmartButtonType.Window:
                        new Microsoft.UI.Xaml.Window { Content = value }.Activate();
                        break;
                    case SmartButtonType.LeftPanel:
                        LeftPanelContent.Content = value;
                        break;
                    case SmartButtonType.MainPanel:
                        CenterPanelContent.Content = value;
                        break;
                    case SmartButtonType.RightPanel:
                        RightPanelContent.Content = value;
                        break;
                    case SmartButtonType.BottomPanel:
                        BottomPanelContent.Content = value;
                        break;
                    case SmartButtonType.Modal:
                        await ShowModalAsync(value, smartButton.Name);
                        break;
                }
            }
        }

        private void OnRequestWindowHandle(RequestWindowHandleEvent e)
        {
            var handle = WindowNative.GetWindowHandle(this);
            Debug.WriteLine($"/SENDING Window Handle: {handle}");
            e.OnHandleReceived?.Invoke(handle);
        }
    }
}
