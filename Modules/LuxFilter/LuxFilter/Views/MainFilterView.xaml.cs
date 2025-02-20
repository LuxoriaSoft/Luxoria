using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxFilter.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainFilterView : Page
{
    /// <summary>
    /// Event Bus
    /// </summary>
    private readonly IEventBus _eventBus;

    /// <summary>
    /// Logger Service
    /// </summary>
    private readonly ILoggerService _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventBus">Communication system (IPC)</param>
    public MainFilterView(IEventBus eventBus, ILoggerService logger)
    {
        _eventBus = eventBus;
        _logger = logger;

        this.InitializeComponent();

        // Load the default view
        EntryPoint();
    }

    /// <summary>
    /// Set the filter view
    /// </summary>
    public void EntryPoint() => SetFilterView();

    /// <summary>
    /// Set the filter view
    /// </summary>
    public void SetFilterView() => ModalContent.Content = new FilterView(_eventBus, _logger, this);

    /// <summary>
    /// Set the status view
    /// </summary>
    /// <param name="pipeline">Pipeline to be executed</param>
    public void SetStatusView(IPipelineService pipeline) => ModalContent.Content = new StatusView(_eventBus, this, pipeline);
}
