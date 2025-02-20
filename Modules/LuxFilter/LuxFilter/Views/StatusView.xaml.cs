using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxFilter.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StatusView : Page
{
    private readonly IEventBus _eventBus;
    private readonly MainFilterView _parent;
    private readonly IPipelineService _pipeline;

    public StatusView(IEventBus eventBus, MainFilterView parent, IPipelineService pipeline)
    {
        _eventBus = eventBus;
        _parent = parent;
        _pipeline = pipeline;

        this.InitializeComponent();
    }
}
