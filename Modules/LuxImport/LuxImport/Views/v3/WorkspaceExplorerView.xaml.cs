using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxImport.Views.v3;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WorkspaceExplorerView : Page
{
    public WorkspaceExplorerView()
    {
        InitializeComponent();
    }

    private void WE_DragOver(object _, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
    }

    private void WE_Drop(object sender, DragEventArgs e)
    {
        Debug.WriteLine("sender:");
        Debug.WriteLine(sender);

        Debug.WriteLine("\nEvent:");
        Debug.WriteLine(e);

        Debug.WriteLine("\nDataView:");
        Debug.WriteLine(e.DataView.Properties);
        foreach (KeyValuePair<string, object> item in e.DataView.Properties)
        {
            Debug.WriteLine(item.Key, item.Value);
        }

        Debug.WriteLine("\nData:");
        Debug.WriteLine(e.Data.Properties);
        foreach (KeyValuePair<string, object> item in e.Data.Properties)
        {
            Debug.WriteLine(item.Key, item.Value);
        }
    }
}
