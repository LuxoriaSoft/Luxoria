using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Selectors;
using LuxEditor.Services;
using LuxEditor.ViewModels;
using Luxoria.App.Utils;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LuxEditor.Components
{

    public class KeyValueStringPair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public sealed partial class Infos : Page
    {
        public ObservableCollection<KeyValueStringPair> ExifData { get; } = new();
        public PresetsViewModel ViewModel { get; } = new();
        private IEventBus _eventBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="Infos"/> class.
        /// </summary>
        public Infos(IEventBus eventBus)
        {
            this.InitializeComponent();

            DataContext = ViewModel;
            ExifListView.ItemsSource = ExifData;
            ImageManager.Instance.OnSelectionChanged += (image) =>
            {
                DisplayExifData(image.Metadata);
                PresetTree.SelectedItem = null;
            };

            var selector = new PresetTemplateSelector
            {
                CategoryTemplate = (DataTemplate)Resources["CategoryTemplate"],
                PresetTemplate = (DataTemplate)Resources["PresetTemplate"]
            };
            PresetTree.ItemTemplateSelector = selector;
            _eventBus = eventBus;
            PresetTree.ItemInvoked += PresetTree_ItemInvoked;
        }

        /// <summary>
        /// Sets the EXIF data into the ListView for viewing.
        /// </summary>
        public void DisplayExifData(
            System.Collections.ObjectModel.ReadOnlyDictionary<string, string> metadata)
        {
            ExifData.Clear();

            foreach (var entry in metadata)
            {
                if (entry.Key != null && !entry.Key.ToLower().StartsWith("unknown"))
                {
                    ExifData.Add(new KeyValueStringPair
                    {
                        Key = entry.Key,
                        Value = entry.Value
                    });
                }
            }
        }

        private void PresetTree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is PresetViewModel pvm)
                PresetManager.Instance.ApplyPreset(pvm.Model);
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            btn.Flyout?.ShowAt(btn);
        }


        private async void ImportPreset_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".luxpreset");
            picker.FileTypeFilter.Add(".zip");

            var hwnd = await WindowHandleHelper.GetMainHwndAsync(_eventBus);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files is null || files.Count == 0) return;

            foreach (var file in files)
                PresetManager.Instance.Import(file.Path);
        }



        private async void CreatePreset_Click(object sender, RoutedEventArgs e)
        {
            var img = ImageManager.Instance.SelectedImage;
            if (img == null) return;

            var categoryBox = new TextBox { Header = "Category", PlaceholderText = "e.g. User Presets" };
            var nameBox = new TextBox { Header = "Preset Name", PlaceholderText = "Untitled Preset" };

            var expanderPanel = new StackPanel { Spacing = 8 };

            void AddStringExpander(string title, IEnumerable<string> items)
            {
                var exp = new Expander { Header = title, IsExpanded = true };
                var sub = new StackPanel { Spacing = 4, Padding = new Thickness(12, 0, 0, 0) };
                foreach (var k in items)
                    sub.Children.Add(new CheckBox { Content = k, IsChecked = true, Tag = k });
                exp.Content = sub;
                expanderPanel.Children.Add(exp);
            }

            AddStringExpander("Basic", new[] {
                "Exposure","Contrast","Highlights","Shadows","Whites","Blacks",
                "Texture","Clarity","Dehaze","Vibrance","Saturation"
            });
            AddStringExpander("White Balance", new[] { "Temperature", "Tint" });

            var curves = new List<(string disp, string key)> {
                ("Parametric Curve","ToneCurve_Parametric"),
                ("Luminance Point Curve","ToneCurve_Point"),
                ("Red Channel Curve","ToneCurve_Red"),
                ("Green Channel Curve","ToneCurve_Green"),
                ("Blue Channel Curve","ToneCurve_Blue"),
            };
            var curvesExp = new Expander { Header = "Curves", IsExpanded = true };
            var curvesPanel = new StackPanel { Spacing = 4, Padding = new Thickness(12, 0, 0, 0) };
            foreach (var (d, k) in curves)
                curvesPanel.Children.Add(new CheckBox { Content = d, IsChecked = true, Tag = k });
            curvesExp.Content = curvesPanel;
            expanderPanel.Children.Add(curvesExp);

            var maskExp = new Expander { Header = "Masks", IsExpanded = true };
            var maskRoot = new StackPanel { Spacing = 4, Padding = new Thickness(12, 0, 0, 0) };
            foreach (var layer in img.LayerManager.Layers)
            {
                var layerExp = new Expander { Header = layer.Name, IsExpanded = false, Margin = new Thickness(0, 4, 0, 0) };
                var layerPanel = new StackPanel { Spacing = 4, Padding = new Thickness(12, 0, 0, 0) };
                foreach (var op in layer.Operations.OfType<MaskOperation>())
                {
                    layerPanel.Children.Add(new CheckBox
                    {
                        Content = $"{op.Tool.ToolType} ({op.Mode})",
                        IsChecked = true,
                        Tag = op.Id
                    });
                }
                layerExp.Content = layerPanel;
                maskRoot.Children.Add(layerExp);
            }
            maskExp.Content = maskRoot;
            expanderPanel.Children.Add(maskExp);

            var dlgContent = new StackPanel { Spacing = 12 };
            dlgContent.Children.Add(categoryBox);
            dlgContent.Children.Add(nameBox);
            dlgContent.Children.Add(new TextBlock { Text = "Include Settings:", FontWeight = FontWeights.SemiBold });
            dlgContent.Children.Add(new ScrollViewer { Content = expanderPanel, Height = 300 });

            var dlg = new ContentDialog
            {
                Title = "Create preset from current settings",
                Content = dlgContent,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = ((FrameworkElement)sender).XamlRoot
            };
            if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;

            var category = categoryBox.Text.Trim();
            var name = nameBox.Text.Trim();
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(name)) return;

            var selSettings = new HashSet<string>();
            var selMaskIds = new HashSet<uint>();

            foreach (var cb in FindVisualChildren<CheckBox>(expanderPanel))
            {
                if (cb.IsChecked != true || cb.Tag == null) continue;
                switch (cb.Tag)
                {
                    case string s: selSettings.Add(s); break;
                    case uint id: selMaskIds.Add(id); break;
                }
            }

            var snap = img.CaptureSnapshot();

            snap.Settings = snap.Settings
                .Where(kv => selSettings.Any(key => kv.Key == key || kv.Key.StartsWith(key + "_")))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var lmClone = snap.LayerManager.Clone();
            snap.LayerManager = lmClone;

            PresetManager.Instance.CreatePresetFromSnapshot(snap, category, name);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null) yield break;
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t) yield return t;
                foreach (var desc in FindVisualChildren<T>(child))
                    yield return desc;
            }
        }


    }
}
