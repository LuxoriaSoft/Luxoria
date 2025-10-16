using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace LuxImport.Views.v3
{
    public sealed partial class ResourcesExplorerView : Page
    {
        public ResourcesExplorerView()
        {
            this.InitializeComponent();

            InitialiseTreeView();
        }

        public class TreeItem
        {
            public BitmapImage? BitmapImage { get; set; } = null;
            public string? DisplayText { get; set; } = null;
            public string? Path { get; set; } = null;
        }

        private ObservableCollection<TreeItem> _items = [];

        private static readonly string[] specialFolders =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        private static readonly string[] ignoreFolders =
        {
            ".lux"
        };

        private bool CheckFolderPermission(string folderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            try
            {
                DirectorySecurity dirAC = dirInfo.GetAccessControl(AccessControlSections.All);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void InitialiseTreeView()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                if (!CheckFolderPermission(drive.RootDirectory.FullName)) continue;
                try
                {
                    StorageFolder folder = await StorageFolder
                        .GetFolderFromPathAsync(drive.RootDirectory.FullName);

                    if (!CheckFolderPermission(folder.Path) || ignoreFolders.Contains(folder.DisplayName)) continue;

                    StorageItemThumbnail itemIcon = await folder
                        .GetThumbnailAsync(ThumbnailMode.SingleItem, 24, ThumbnailOptions.UseCurrentScale);

                    TreeItem item = new TreeItem
                    {
                        BitmapImage = new BitmapImage()
                    };

                    item.BitmapImage.SetSource(itemIcon);

                    if (string.IsNullOrEmpty(drive.VolumeLabel))
                        item.DisplayText = $"Local Disk ({drive.Name})";
                    else item.DisplayText = drive.VolumeLabel;

                    item.Path = drive.Name;

                    _items.Add(item);
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
                catch {}
            }

            foreach (string fld in specialFolders)
            {
                if (ignoreFolders.Contains(fld)) continue;

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fld);
                if (!CheckFolderPermission(fld) || ignoreFolders.Contains(folder.DisplayName)) continue;

                StorageItemThumbnail itemIcon = await folder
                    .GetThumbnailAsync(ThumbnailMode.SingleItem, 32, ThumbnailOptions.UseCurrentScale);

                TreeItem item = new TreeItem
                {
                    BitmapImage = new BitmapImage()
                };

                item.BitmapImage.SetSource(itemIcon);

                item.DisplayText = folder.DisplayName;
                item.Path = fld;
                _items.Add(item);
            }

            ExplorerTree.ItemsSource = _items;
        }

        private void ExplorerTree_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.Content is TreeItem item && item.Path != null)
            {
                try
                {

                    string[] subFolders = Directory.GetDirectories(item.Path);

                    foreach (string subFolder in subFolders)
                    {
                        if (ignoreFolders.Contains(Path.GetFileName(subFolder))
                            ||
                            !CheckFolderPermission(subFolder)) continue;

                        TreeViewNode node = new();
                        TreeItem tItem = new();


                        tItem.DisplayText = Path.GetFileName(subFolder);
                        tItem.Path = subFolder;

                        node.Content = tItem;

                        args.Node.Children.Add(node);
                    }
                } catch { }
            } 
        }

        private void ExplorerTree_DoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (ExplorerTree.SelectedNode == null) return;

            if (!ExplorerTree.SelectedNode.IsExpanded)
            {
                ExplorerTree.Expand(ExplorerTree.SelectedNode);
            } else
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
    }
}
