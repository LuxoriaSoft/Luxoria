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

namespace LuxImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainImportView : Page
    {
        private readonly IEventBus _eventBus;

        public MainImportView(IEventBus eventBus)
        {
            _eventBus = eventBus;

            this.InitializeComponent();

            // Load ImportView by default
            EntryPoint();
        }

        public void EntryPoint() => SetImportView();

        // Switch to ImportView
        public void SetImportView()
        {
            ModalContent.Content = new ImportView(_eventBus, this);
            UpdateProgress(1);
        }

        public void SetPropertiesView(string collectionPath)
        {
            ModalContent.Content = new PropertiesView(_eventBus, this, collectionPath);
            UpdateProgress(2);
        }

        public void SetIndexicationView(string collectionName, string collectionPath)
        {
            ModalContent.Content = new IndexicationView(_eventBus, this, collectionName, collectionPath);
            UpdateProgress(3);
        }

        private void UpdateProgress(int step)
        {
            StepProgressBar.Value = step;
        }

    }
}
