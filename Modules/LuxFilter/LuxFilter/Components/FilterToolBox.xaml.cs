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
        public event Action? OnSaveClicked;

        public FilterToolBox(IEventBus eventBus, ILoggerService logger)
        {
            InitializeComponent();

            _fExplorer = new(eventBus, logger);
            _fExplorer.OnScoreUpdated += ((string, Guid, double) x) => OnScoreUpdated?.Invoke(x);

            FEGrid.Children.Add(_fExplorer);

            // Button with save & icon
            var saveButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new SymbolIcon(Symbol.Save),
                        new TextBlock
                        {
                            Text = "Save / Sync",
                            Margin = new Microsoft.UI.Xaml.Thickness(5, 0, 0, 0),
                            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                        }
                    }
                }
            };

            // Attach click event handler
            saveButton.Click += (sender, e) =>
            {
                OnSaveClicked?.Invoke();
            };
            F1Grid.Children.Add(saveButton);
        }

        public void SetImages(ICollection<LuxAsset> assets)
        {
            _fExplorer.SetImages(assets);
        }
    }
}
