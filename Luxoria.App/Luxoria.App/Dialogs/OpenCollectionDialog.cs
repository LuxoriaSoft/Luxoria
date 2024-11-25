using Luxoria.App.Views;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Luxoria.App.Components.Dialogs
{
    public static class OpenCollectionDialog
    {
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
                loggerService.Log($"Selected folder path: {selectedFolderPath}");

                await ImportationDialog.ShowAsync(eventBus, loggerService, collectionName, selectedFolderPath, xamlRoot);
            }
        }
    }
}
