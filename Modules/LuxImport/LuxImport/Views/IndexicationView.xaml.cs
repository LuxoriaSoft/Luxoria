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
    public sealed partial class IndexicationView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainImportView _Parent;
        private readonly string _collectionName;
        private readonly string _collectionPath;

        public IndexicationView(IEventBus eventBus, MainImportView parent, string collectionName, string collectionPath)
        {
            _eventBus = eventBus;
            _Parent = parent;
            _collectionName = collectionName;
            _collectionPath = collectionPath;

            this.InitializeComponent();
        }
    }
}
