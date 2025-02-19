using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Luxoria.Modules.Interfaces;

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
