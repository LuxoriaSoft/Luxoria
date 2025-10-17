using LuxImport.Logic.Services;
using LuxImport.Models.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace LuxImport.Views.v3
{
    public sealed partial class ResourcesExplorerView : Page
    {
        public ResourcesExplorerView()
        {
            this.InitializeComponent();

            Task.Run(InitialiseTreeView);
        }

        private static readonly string[] ignoreFolders =
        {
            ".lux"
        };

        private async void InitialiseTreeView()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            ObservableCollection<TreeItem> items = [];

            foreach (DriveInfo drive in drives)
            {
                if (!IOService.CheckFolderPermission(drive.RootDirectory.FullName)) continue;
                try
                {
                    StorageFolder folder = await StorageFolder
                        .GetFolderFromPathAsync(drive.RootDirectory.FullName);

                    if (!IOService.CheckFolderPermission(folder.Path) || ignoreFolders.Contains(folder.DisplayName)) continue;

                    StorageItemThumbnail itemIcon = await folder
                        .GetThumbnailAsync(ThumbnailMode.SingleItem, 14, ThumbnailOptions.UseCurrentScale);

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        TreeItem item = new()
                        {
                            BitmapImage = new BitmapImage()
                        };

                        item.BitmapImage.SetSource(itemIcon);

                        if (string.IsNullOrEmpty(drive.VolumeLabel))
                            item.DisplayText = $"Local Disk ({drive.Name})";
                        else item.DisplayText = drive.VolumeLabel;

                        item.Path = drive.Name;
                        items.Add(item);
                    });
                }
                catch (FileNotFoundException)
                {
                    Debug.WriteLine("Error: File not found");
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("Error: Application does not have access right to the specified folder");
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    Debug.WriteLine("Error: System failure");
                }
                catch { }
            }

            foreach (string fld in SourceService.SpecialFolders)
            {
                if (ignoreFolders.Contains(fld)) continue;

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fld);
                if (!IOService.CheckFolderPermission(fld) || ignoreFolders.Contains(folder.DisplayName)) continue;

                StorageItemThumbnail itemIcon = await folder
                    .GetThumbnailAsync(ThumbnailMode.SingleItem, 14, ThumbnailOptions.UseCurrentScale);

                DispatcherQueue.TryEnqueue(() =>
                {
                    TreeItem item = new()
                    {
                        BitmapImage = new BitmapImage()
                    };

                    item.BitmapImage.SetSource(itemIcon);

                    item.DisplayText = folder.DisplayName;
                    item.Path = fld;
                    items.Add(item);
                });
            }

            DispatcherQueue.TryEnqueue(() => ExplorerTree.ItemsSource = items);
        }

        private void ExplorerTree_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.Content is TreeItem item && item.Path != null)
            {
                try
                {

                    IEnumerable<IOService.Source> subFolders = IOService.GetDirectories(item.Path);

                    foreach (IOService.Source subFolder in subFolders)
                    {
                        if (ignoreFolders.Contains(subFolder.DisplayName)
                            ||
                            !subFolder.HasAccess) continue;

                        TreeViewNode node = new();
                        TreeItem tItem = new()
                        {
                            DisplayText = subFolder.DisplayName,
                            Path = subFolder.Path,
                        };

                        node.Content = tItem;

                        args.Node.Children.Add(node);
                    }
                }
                catch { }
            }
        }

        private void ExplorerTree_DoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (ExplorerTree.SelectedNode == null) return;

            if (!ExplorerTree.SelectedNode.IsExpanded)
            {
                ExplorerTree.Expand(ExplorerTree.SelectedNode);
            }
            else
            {
                ExplorerTree.Collapse(ExplorerTree.SelectedNode);
            }
        }

        private void ExplorerTree_Collapsed(object sender, TreeViewCollapsedEventArgs args)
        {
            if (args.Node.HasChildren)
            {
                args.Node.Children.Clear();
                args.Node.HasUnrealizedChildren = true;
            }
        }

        private void TI_DragStarting(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs args)
        {

        }
    }
}
