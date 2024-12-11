using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App.Views
{
    public sealed partial class ImportationControl : UserControl
    {
        public ImportationControl()
        {
            this.InitializeComponent();
        }

        public void UpdateProgress(string message)
        {
            // Update the TextBlock with the message
            ImportationLog.Text = message;
        }

        public void UpdateProgress(string message, int progress)
        {
            // Update the TextBlock with the message
            ImportationLog.Text = message;

            // Clamp the progress value between 0 and 100 to ensure it's within bounds
            progress = Math.Max(0, Math.Min(100, progress));

            // Update the ProgressBar value
            ImportationProgressBar.Value = progress;
        }

        public void UpdateOnlyProgressBar(int progress)
        {
            // Clamp the progress value between 0 and 100 to ensure it's within bounds
            progress = Math.Max(0, Math.Min(100, progress));

            // Update the ProgressBar value
            ImportationProgressBar.Value = progress;
        }
    }
}
