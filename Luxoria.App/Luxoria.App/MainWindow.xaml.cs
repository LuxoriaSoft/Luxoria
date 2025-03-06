using Luxoria.App.EventHandlers;
using Luxoria.App.Interfaces;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Luxoria.App;

public sealed partial class MainWindow : Window
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
    public MainWindow(IEventBus eventBus, ILoggerService loggerService, IModuleService moduleService, IModuleUIService uiService)
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

        // Load App Icon
        LoadIcon("Luxoria_icon");

        // Subscribe handlers to the event bus
        InitializeEventBus();

        // LoadComponents
        LoadComponents();

        // Load the default collection
        LoadDefaultCollection();
    }

    /// <summary>
    /// Load the application icon from the specified file path.
    /// </summary>
    /// <param name="iconName">Icon Name (Looking on Assets folder)</param>
    private void LoadIcon(string iconName) => AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, $"Assets/{iconName}.ico"));


    /// <summary>
    /// Initialize the event bus and subscribe handlers to events.
    /// </summary>
    private void InitializeEventBus()
    {
        // Subscribe to events that will be published through the event bus
        _eventBus.Subscribe<CollectionUpdatedEvent>(_collectionUpdatedHandler.OnCollectionUpdated);

        // Subscribe to the window handle request event
        _eventBus.Subscribe<RequestWindowHandleEvent>(OnRequestWindowHandle);
    }

    /// <summmary>
    /// </summmary>
    public void EnableEasyLoader()
    {

    }

    /// <summary>
    /// Displays a modal dialog with the specified content and title.
    /// Ensures the content is properly detached when the dialog closes 
    /// to prevent reusing issues in future dialogs.
    /// </summary>
    /// <param name="content">The UI element to display inside the modal.</param>
    /// <param name="title">The title of the modal dialog.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ShowModalAsync(UIElement content, string title)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = this.Content.XamlRoot
        };

        dialog.Closed += (_, _) =>
        {
            dialog.Content = null; // Manually remove content before reusing it
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

            Action act = async () =>
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
                                await ShowModalAsync(modalContent, item.Name);
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

    private void OnRequestWindowHandle(RequestWindowHandleEvent e)
    {
        var handle = WindowNative.GetWindowHandle(this);
        Debug.WriteLine($"/SENDING Window Handle: {handle}");
        e.OnHandleReceived?.Invoke(handle); // Send back the handle
    }

    private void LoadDefaultCollection()
    {
        /*
        var openCollectionEvt = new OpenCollectionEvent("testCollection", "C:\\Users\\pastcque\\source\\repos\\LuxoriaSoft\\Luxoria\\assets\\BaseCollection");

        openCollectionEvt.OnEventCompleted += (_, _) =>
        {
            _loggerService.Log("Collection import completed successfully.");
        };

        Task.Run(async () =>
        {
            await _eventBus.Publish(openCollectionEvt);
        });
        */
    }
}
