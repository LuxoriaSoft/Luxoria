using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxFilter.Views
{
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
        /// Constructor
        /// </summary>
        /// <param name="eventBus">Communication system (IPC)</param>
        public MainFilterView(IEventBus eventBus)
        {
            _eventBus = eventBus;

            this.InitializeComponent();

            // Load the default view
            EntryPoint();
        }

        /// <summary>
        /// Set the filter view
        /// </summary>
        public void EntryPoint() => SetFilterMenu();

        /// <summary>
        /// Set the filter menu
        /// </summary>
        public void SetFilterMenu()
        {
            ModalContent.Content = new FilterView(_eventBus, this);
        }
    }
}
