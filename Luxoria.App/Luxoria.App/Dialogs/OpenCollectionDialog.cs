using Luxoria.App.Views;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Luxoria.App.Components.Dialogs;

/// <summary>
/// Dialog for opening a collection.
/// </summary>
public static class OpenCollectionDialog
{
    /// <summary>
    /// Shows the Open Collection dialog.
    /// </summary>
    public static async Task ShowAsync(IEventBus eventBus, ILoggerService loggerService, Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        var openCollectionControl = new OpenCollectionControl();
        var dialog = new ContentDialog
        {
            Title = "Open Collection",
            Content = openCollectionControl,
            CloseButtonText = "Close",
            PrimaryButtonText = "Next",
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            string selectedFolderPath = openCollectionControl.SelectedFolderPath;
            string collectionName = openCollectionControl.CollectionName;
            await loggerService.LogAsync($"Selected folder path: {selectedFolderPath}");

            await ImportationDialog.ShowAsync(eventBus, loggerService, collectionName, selectedFolderPath, xamlRoot);
        }
    }
}
