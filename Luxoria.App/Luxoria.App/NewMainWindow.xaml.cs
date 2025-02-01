using Luxoria.App.Components;
using Luxoria.App.Components.Dialogs;
using Luxoria.App.EventHandlers;
using Luxoria.App.Interfaces;
using Luxoria.App.Views;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Luxoria.App
{
    public sealed partial class NewMainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _loggerService;

        // Handlers for different events
        private readonly ImageUpdatedHandler _imageUpdatedHandler;
        private readonly CollectionUpdatedHandler _collectionUpdatedHandler;
        private readonly IModuleService _moduleService;
        private readonly IModuleUIService _uiService;


        /// <summary>
        /// Constructor for the main window of the application.
        /// </summary>
        public NewMainWindow(IEventBus eventBus, ILoggerService loggerService, IModuleService moduleService, IModuleUIService uiService)
        {
            InitializeComponent();

            // Dependency injection
            _eventBus = eventBus;
            _loggerService = loggerService;

            // Initialize event handlers
            _imageUpdatedHandler = new ImageUpdatedHandler(_loggerService);
            _collectionUpdatedHandler = new CollectionUpdatedHandler(_loggerService);

            _moduleService = moduleService;
            _uiService = uiService;

            // Subscribe handlers to the event bus
            InitializeEventBus();

            // LoadComponents
            LoadComponents();
        }

        /// <summary>
        /// Initialize the event bus and subscribe handlers to events.
        /// </summary>
        private void InitializeEventBus()
        {
            // Subscribe to events that will be published through the event bus
            _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);
        }

        /// <summmary>
        /// </summmary>
        public void EnableEasyLoader()
        {

        }

        /// <summary>
        /// </summary>
        private async Task ShowModalAsync(UIElement content, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// </summary>
        private void LoadComponents()
        {
            foreach (var item in _moduleService
                .GetModules()
                .Where(m => m is IModuleUI)
                .SelectMany(m => ((IModuleUI)m).Items))
            {
                Debug.WriteLine("Item: " + item.Name);

                Action act = () =>
                {

                    var button = item.IsLeftLocated ? MainMenu.GetLeftButton(item.Name) : MainMenu.GetRightButton(item.Name);

                    if (button == null)
                    {
                        Debug.WriteLine($"Button not found for item: {item.Name}");
                        return;
                    }

                    if (item.SmartButtons.Count > 1)
                    {
                        var flyout = new MenuFlyout();

                        foreach (var smartButton in item.SmartButtons)
                        {
                            var flyoutItem = new MenuFlyoutItem
                            {
                                Text = smartButton.Name
                            };

                            flyoutItem.Click += async (sender, e) =>
                            {
                                foreach (var gmodule in smartButton.Pages)
                                {
                                    if (gmodule.Value == null)
                                    {
                                        Debug.WriteLine($"[FlyoutItem Click] Page null for type {gmodule.Key}");
                                        continue;
                                    }

                                    switch (gmodule.Key)
                                    {
                                        case GModules.SmartButtonType.Window:
                                            var newWindow = new Microsoft.UI.Xaml.Window();
                                            newWindow.Content = gmodule.Value;
                                            newWindow.Activate();
                                            break;

                                        case GModules.SmartButtonType.LeftPanel:
                                            LeftPanelContent.Content = gmodule.Value;
                                            break;

                                        case GModules.SmartButtonType.MainPanel:
                                            CenterPanelContent.Content = gmodule.Value;
                                            break;

                                        case GModules.SmartButtonType.RightPanel:
                                            RightPanelContent.Content = gmodule.Value;
                                            break;

                                        case GModules.SmartButtonType.BottomPanel:
                                            BottomPanelContent.Content = gmodule.Value;
                                            break;

                                        case GModules.SmartButtonType.Modal:
                                            var modalContent = gmodule.Value;
                                            await ShowModalAsync(modalContent, item.Name);
                                            break;
                                    }
                                }
                            };

                            flyout.Items.Add(flyoutItem);
                        }

                        FlyoutBase.SetAttachedFlyout(button, flyout);

                        FlyoutBase.ShowAttachedFlyout(button);
                    }
                    else if (item.SmartButtons.Count == 1)
                    {
                        foreach (var gmodule in item.SmartButtons[0].Pages)
                        {
                            switch (gmodule.Key)
                            {
                                case GModules.SmartButtonType.Window:
                                    var newWindow = new Microsoft.UI.Xaml.Window();
                                    newWindow.Content = gmodule.Value;
                                    newWindow.Activate();
                                    break;

                                case GModules.SmartButtonType.LeftPanel:
                                    LeftPanelContent.Content = gmodule.Value;
                                    break;

                                case GModules.SmartButtonType.MainPanel:
                                    CenterPanelContent.Content = gmodule.Value;
                                    break;

                                case GModules.SmartButtonType.RightPanel:
                                    RightPanelContent.Content = gmodule.Value;
                                    break;

                                case GModules.SmartButtonType.BottomPanel:
                                    BottomPanelContent.Content = gmodule.Value;
                                    break;

                                case GModules.SmartButtonType.Modal:
                                    var modalContent = gmodule.Value;
                                    ShowModalAsync(modalContent, item.Name).GetAwaiter().GetResult();
                                    break;
                            }
                        }
                    }
                };
                if (item.IsLeftLocated)
                {    
                    MainMenu.AddLeftButton(item.Name, act);
                }
                else
                {
                    MainMenu.AddRightButton(item.Name, act);
                }
            }
        }
    }
}
