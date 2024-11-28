using Luxoria.App.Views;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Luxoria.App.Components.Dialogs;

/// <summary>
/// Dialog for importing a collection.
/// </summary>
public static class ImportationDialog
{
    /// <summary>
    /// Shows the Importation dialog.
    /// </summary>
    public static async Task ShowAsync(IEventBus eventBus, ILoggerService loggerService, string collectionName, string folderPath, Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        var importationControl = new ImportationControl();
        var dialog = new ContentDialog
        {
            Title = "Importation",
            Content = importationControl,
            XamlRoot = xamlRoot
        };

        var openCollectionEvt = new OpenCollectionEvent(collectionName, folderPath);
        openCollectionEvt.ProgressMessage += (message, progress) =>
        {
            if (progress.HasValue)
                importationControl.UpdateProgress(message, progress.Value);
            else
                importationControl.UpdateProgress(message);
        };

        openCollectionEvt.OnEventCompleted += (_, _) =>
        {
            dialog.Hide();
            loggerService.Log("Collection import completed successfully.");
        };

        openCollectionEvt.OnImportFailed += (_, _) =>
        {
            dialog.Hide();
            loggerService.Log("Collection import failed.");
        };

        var dialogTask = dialog.ShowAsync();

        try
        {
            await eventBus.Publish(openCollectionEvt);
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error publishing event: {ex.Message}");
            importationControl.UpdateProgress("Error: " + ex.Message);
        }

        await dialogTask;
    }
}
