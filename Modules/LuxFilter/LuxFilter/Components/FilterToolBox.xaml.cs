using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

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

        public event Action<(string, Guid, double)>? OnScoreUpdated;

        public FilterToolBox(IEventBus eventBus, ILoggerService logger)
        {
            InitializeComponent();

            _fExplorer = new(eventBus, logger);
            _fExplorer.OnScoreUpdated += ((string, Guid, double)x) => OnScoreUpdated?.Invoke(x);

            FEGrid.Children.Add(_fExplorer);
        }

        public void SetImages(ICollection<LuxAsset> assets)
        {
            _fExplorer.SetImages(assets);
        }
    }
}
