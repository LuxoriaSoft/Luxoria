using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxFilter.Components
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilterToolBox : Page
    {
        private readonly FilterExplorer _fExplorer;
        public FilterToolBox(IEventBus eventBus, ILoggerService logger)
        {
            InitializeComponent();

            _fExplorer = new(eventBus, logger);

            FEGrid.Children.Add(_fExplorer);
        }
    }
}
