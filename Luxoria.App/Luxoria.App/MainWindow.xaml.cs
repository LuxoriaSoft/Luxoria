using Luxoria.App.EventHandlers;
using Luxoria.App.Interfaces;
using Luxoria.GModules;
using Luxoria.GModules.Helpers;
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
using Windows.UI.Notifications;
using WinRT.Interop;

namespace Luxoria.App
{
    /// <summary>
    /// The main application window that handles UI initialization, event subscriptions, 
    /// and module-based component loading.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _loggerService;
        private readonly IModuleService _moduleService;
        private readonly IModuleUIService _uiService;

        // Event handlers
        private readonly ImageUpdatedHandler _imageUpdatedHandler;
        private readonly CollectionUpdatedHandler _collectionUpdatedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="eventBus">Event bus for handling global events.</param>
        /// <param name="loggerService">Service for logging application activity.</param>
        /// <param name="moduleService">Service for managing modules.</param>
        /// <param name="uiService">Service for managing UI modules.</param>
        public MainWindow(IEventBus eventBus, ILoggerService loggerService, IModuleService moduleService, IModuleUIService uiService)
        {
            InitializeComponent();

            _eventBus = eventBus;
            _loggerService = loggerService;
            _moduleService = moduleService;
            _uiService = uiService;

            _imageUpdatedHandler = new ImageUpdatedHandler(_loggerService);
            _collectionUpdatedHandler = new CollectionUpdatedHandler(_loggerService);

            // Load App Icon
            WindowHelper.SetCaption(AppWindow, "Luxoria_icon");

            InitializeEventBus();
            LoadComponents();
        }

        /// <summary>
        /// Subscribes to necessary events for the application.
        /// </summary>
        private void InitializeEventBus()
        {
            _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);
            _eventBus.Subscribe<RequestWindowHandleEvent>(OnRequestWindowHandle);
            _eventBus.Subscribe<ToastNotificationEvent>(OnToastNotificationHandle);
        }

        /// <summary>
        /// Displays a modal dialog with the specified content and title.
        /// </summary>
        /// <param name="content">The UI element to display in the modal.</param>
        /// <param name="title">The title of the modal dialog.</param>
        private async Task ShowModalAsync(UIElement content, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot
            };

            // Prevent reuse issues
            dialog.Closed += (_, _) => dialog.Content = null;

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Loads UI components from registered modules and attaches buttons to the menu.
        /// </summary>
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

        /// <summary>
        /// Handles the click event for menu buttons and determines whether to show a flyout menu or load a smart button directly.
        /// </summary>
        /// <param name="item">The menu item associated with the button.</param>
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

        /// <summary>
        /// Attaches a flyout menu to a button containing multiple smart button options.
        /// </summary>
        /// <param name="button">The UI element to attach the flyout menu to.</param>
        /// <param name="item">The menu item containing multiple smart buttons.</param>
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

        /// <summary>
        /// Handles the click event for an individual smart button and loads the corresponding UI element.
        /// </summary>
        /// <param name="smartButton">The smart button that was clicked.</param>
        private async Task HandleSmartButtonClick(SmartButton smartButton)
        {
            foreach (var (key, value) in smartButton.Pages)
            {
                if (value == null)
                {
                    Debug.WriteLine($"[Warning] Page is null for type {key}");
                    continue;
                }


                if (value is Page)
                {
                    Page valuePage = (Page)value;
                    if (valuePage is null)
                    {
                        Debug.WriteLine($"[Warning] Page is null for type {key}");
                        continue;
                    }

                    switch (key)
                    {
                        case SmartButtonType.Window:
                            var window = new Window { Content = valuePage };
                            smartButton.OnClose += () =>
                            {
                                window.Close();
                            };
                            window.Activate();
                            break;
                        case SmartButtonType.LeftPanel:
                            LeftPanelContent.Content = valuePage;
                            break;
                        case SmartButtonType.MainPanel:
                            CenterPanelContent.Content = valuePage;
                            break;
                        case SmartButtonType.RightPanel:
                            RightPanelContent.Content = valuePage;
                            break;
                        case SmartButtonType.BottomPanel:
                            BottomPanelContent.Content = valuePage;
                            break;
                        case SmartButtonType.Modal:
                            await ShowModalAsync(valuePage, smartButton.Name);
                            break;
                    }
                }
                else if (value is ContentDialog)
                {
                    ContentDialog valueDialog = (ContentDialog)value;

                    if (valueDialog is null)
                    {
                        Debug.WriteLine($"[Warning] Dialog is null for type {key}");
                        continue;
                    }

                    switch (key)
                    {
                        case SmartButtonType.Modal:
                            if (valueDialog.XamlRoot == null)
                                valueDialog.XamlRoot = this.Content.XamlRoot;
                            await valueDialog.ShowAsync();
                            break;
                    }
                }
                else if (value is Window)
                {
                    Window valueWindow = new Window();
                    Window testWindow = (Window)value;
                    valueWindow.Content = testWindow.Content;


                    Debug.WriteLine($"[x] Loading Window: {valueWindow}");

                    if (valueWindow is null)
                    {
                        Debug.WriteLine($"[Warning] Window is null for type {key}");
                        continue;
                    }

                    switch (key)
                    {
                        case SmartButtonType.Window:
                            valueWindow.Activate();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the request for the window handle and returns it via the event.
        /// </summary>
        /// <param name="e">The event containing the request for the window handle.</param>
        private void OnRequestWindowHandle(RequestWindowHandleEvent e)
        {
            var handle = WindowNative.GetWindowHandle(this);
            Debug.WriteLine($"/SENDING Window Handle: {handle}");
            e.OnHandleReceived?.Invoke(handle);
        }

        /// <summary>
        /// Handles toast notification event by displaying a toast notification.
        /// </summary>
        /// <param name="e">The event containing the toast notification data.</param>
        private void OnToastNotificationHandle(ToastNotificationEvent e)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode(e.Message));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier($"Luxoria - {e.Title}").Show(toast);
        }
    }
}
