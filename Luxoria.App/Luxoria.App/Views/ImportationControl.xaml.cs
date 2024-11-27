using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
