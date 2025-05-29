using LuxEditor.Controls;
using LuxEditor.EditorUI;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Groups;
using LuxEditor.EditorUI.Interfaces;
using LuxEditor.EditorUI.Models;
using LuxEditor.Logic;
using LuxEditor.Models;
using LuxEditor.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace LuxEditor.Components
{
    public sealed partial class Editor : Page
    {
        private EditableImage? currentImage;
        private EditorPanelManager? _panelManager;
        private readonly Dictionary<string, EditorCategory> _categories = new();
        private readonly ConcurrentDictionary<string, EditorSlider> _sliderCache = new();
        private CancellationTokenSource? _cts;
        private int _renderRunning;
        private bool _pendingUpdate;
        public event Action<SKImage>? OnEditorImageUpdated;

        private readonly Dictionary<TreeViewNode, object> _nodeMap = new();
        
        /// <summary>
        /// Style for the temperature slider.
        /// </summary>
        private static readonly EditorStyle TempStyle = new()
        {
            GradientStart = Windows.UI.Color.FromArgb(255, 70, 130, 180),
            GradientEnd = Windows.UI.Color.FromArgb(255, 255, 140, 0)
        };

        /// <summary>
        /// Style for the tint slider.
        /// </summary>
        private static readonly EditorStyle TintStyle = new()
        {
            GradientStart = Windows.UI.Color.FromArgb(255, 130, 188, 86),
            GradientEnd = Windows.UI.Color.FromArgb(255, 174, 116, 193)
        };

        /// <summary>
        /// Initializes the Editor page and sets up the UI.
        /// </summary>
        public Editor(EditableImage? editableImage)
        {
            InitializeComponent();

            _panelManager = new EditorPanelManager(EditorStackPanel);
            ImageManager.Instance.OnSelectionChanged += SetEditableImage;

            var toolBar = new ToolSelectorBar();
            ToolBarHost.Content = toolBar;
            toolBar.SelectionChanged += OnToolSelectionChanged;

            AddLayerBtn.Click += OnAddLayerClicked;
            RemoveLayerBtn.Click += OnRemoveLayerClicked;
            LayerTreeView.RightTapped += OnLayerTreeRightTapped;
            LayerTreeView.SelectionChanged += OnLayerTreeSelectionChanged;

            currentImage = editableImage;

            if (currentImage != null)
            {
                currentImage.LayerManager.Layers.CollectionChanged += (_, __) => RefreshLayerTree();
                RefreshLayerTree();

                OnToolSelectionChanged(this, 0);
            }
        }

        private void OnAddLayerClicked(object sender, RoutedEventArgs e)
        {
            OpenBrushSelectionFlyout();
        }

        private void BrushButton_Click(BrushType type)
        {
            if (currentImage == null)
            {
                Debug.WriteLine("No image selected, cannot add layer.");
                return;
            }
            currentImage.LayerManager.AddLayer(type);
            RefreshLayerTree();
        }

        private void OpenBrushSelectionFlyout()
        {
            var flyout = new MenuFlyout();

            var brushButton = new MenuFlyoutItem
            {
                Text = "Brush",
            };
            brushButton.Click += (s, e) => { BrushButton_Click(BrushType.Brush); flyout.Hide(); };
            flyout.Items.Add(brushButton);

            var linearGradientButton = new MenuFlyoutItem
            {
                Text = "Linear Gradient",

            };
            linearGradientButton.Click += (s, e) => { BrushButton_Click(BrushType.LinearGradient); flyout.Hide(); };
            flyout.Items.Add(linearGradientButton);

            var radialGradientButton = new MenuFlyoutItem
            {
                Text = "Radial Gradient",

            };
            radialGradientButton.Click += (s, e) => { BrushButton_Click(BrushType.RadialGradient); flyout.Hide(); };
            flyout.Items.Add(radialGradientButton);

            var colorRangeButton = new MenuFlyoutItem
            {
                Text = "Color Range",

            };
            colorRangeButton.Click += (s, e) => { BrushButton_Click(BrushType.ColorRange); flyout.Hide(); };
            flyout.Items.Add(colorRangeButton);

            flyout.ShowAt(AddLayerBtn);
        }


        private void OnRemoveLayerClicked(object sender, RoutedEventArgs e)
        {
            if (currentImage == null || LayerTreeView.SelectedNodes.Count == 0)
            {
                Debug.WriteLine("No image or layer selected, cannot remove layer.");
                return;
            }
            currentImage.LayerManager.RemoveLayer();
            RefreshLayerTree();
        }

        private void OnLayerTreeSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
        {
            OperationDetailsHost.Visibility = Visibility.Collapsed;
            OperationDetailsHost.Content = null;

            if (e.AddedItems.Count == 0 || e.AddedItems[0] is not TreeViewNode node)
                return;

            OperationDetailsHost.Content = new LayersDetailsPanel();
            OperationDetailsHost.Visibility = Visibility.Visible;
        }

        private TreeViewNode? FindNodeByLayer(Layer target, IList<TreeViewNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Content is Border b && b.Tag == target)
                    return node;

                var found = FindNodeByLayer(target, node.Children);
                if (found != null)
                    return found;
            }
            return null;
        }

        private bool IsOperation(FrameworkElement element)
        {
            return element.DataContext is TreeViewNode node &&
                   _nodeMap.ContainsKey(node) &&
                   _nodeMap[node] is MaskOperation;
        }

        private bool IsLayer(FrameworkElement element)
        {
            return element.DataContext is TreeViewNode node &&
                   _nodeMap.ContainsKey(node) &&
                   _nodeMap[node] is Layer;
        }

        private void ChoseBrushForOperationFlyout(Layer layer, bool isAdded, FrameworkElement element)
        {
            if (currentImage == null)
            {
                Debug.WriteLine("No image selected, cannot add operation.");
                return;
            }
            var flyout = new MenuFlyout();

            var brushButton = new MenuFlyoutItem
            {
                Text = "Brush",
            };
            brushButton.Click += (s, e) =>
            {
                currentImage.LayerManager.AddOperation(layer.Id, new MaskOperation(BrushType.Brush, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract));
                RefreshLayerTree();
                flyout.Hide();
            };
            flyout.Items.Add(brushButton);
            var linearGradientButton = new MenuFlyoutItem
            {
                Text = "Linear Gradient",
            };
            linearGradientButton.Click += (s, e) =>
            {
                currentImage.LayerManager.AddOperation(layer.Id, new MaskOperation(BrushType.LinearGradient, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract));
                RefreshLayerTree();
                flyout.Hide();
            };
            flyout.Items.Add(linearGradientButton);
            var radialGradientButton = new MenuFlyoutItem
            {
                Text = "Radial Gradient",
            };
            radialGradientButton.Click += (s, e) =>
            {

                currentImage.LayerManager.AddOperation(layer.Id, new MaskOperation(BrushType.RadialGradient, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract)); RefreshLayerTree();
                flyout.Hide();
            };
            flyout.Items.Add(radialGradientButton);
            var colorRangeButton = new MenuFlyoutItem
            {
                Text = "Color Range",
            };
            colorRangeButton.Click += (s, e) =>
            {
                currentImage.LayerManager.AddOperation(layer.Id, new MaskOperation(BrushType.ColorRange, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract));
                RefreshLayerTree();
                flyout.Hide();
            };
            flyout.Items.Add(colorRangeButton);
            flyout.ShowAt(element);
        }

        private void SelectSubstractOrAddOperationFlyout(Layer layer, FrameworkElement element)
        {
            var flyout = new MenuFlyout();
            var addItem = new MenuFlyoutItem
            {
                Text = "Add Operation"
            };
            addItem.Click += (s, e) => ChoseBrushForOperationFlyout(layer, true, element);
            flyout.Items.Add(addItem);
            var substractItem = new MenuFlyoutItem
            {
                Text = "Substract Operation"
            };
            substractItem.Click += (s, e) => ChoseBrushForOperationFlyout(layer, false, element);
            flyout.Items.Add(substractItem);
            flyout.ShowAt(element);
        }

        private void OnLayerTreeRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Debug.WriteLine("Right-click on layer tree");

            if (currentImage == null)
            {
                Debug.WriteLine("No image selected, cannot show context menu.");
                return;
            }

            if (sender is not TreeView treeView || e.OriginalSource is not FrameworkElement element)
                return;

            if (element.DataContext is not TreeViewNode node)
                return;


            var flyout = new MenuFlyout();


            if (IsOperation(element))
            {
                Debug.WriteLine("Operation Detected");
                var op = _nodeMap[node] as MaskOperation;
                if (op == null) return;

                Debug.WriteLine($"Operation: {op.BrushType} ({op.Mode})");
                var deleteOpItem = new MenuFlyoutItem
                {
                    Text = "Delete Operation"
                };
                deleteOpItem.Click += (s, args) =>
                {
                    currentImage.LayerManager.RemoveOperation(op.Id);
                    RefreshLayerTree();
                };
                flyout.Items.Add(deleteOpItem);
                
                var setOperationModeItem = new MenuFlyoutItem
                {
                    Text = "Set Operation Mode"
                };
                setOperationModeItem.Click += (s, args) =>
                {
                    var modeFlyout = new MenuFlyout();
                    foreach (var mode in Enum.GetValues(typeof(BooleanOperationMode)).Cast<BooleanOperationMode>())
                    {
                        var modeItem = new MenuFlyoutItem
                        {
                            Text = mode.ToString()
                        };
                        modeItem.Click += (ss, aa) =>
                        {
                            op.Mode = mode;
                            RefreshLayerTree();
                        };
                        modeFlyout.Items.Add(modeItem);
                    }
                    modeFlyout.ShowAt(element);
                };
                flyout.Items.Add(setOperationModeItem);
            }
            else if (IsLayer(element))
            {
                Debug.WriteLine("Layer Detected");

                var layer = _nodeMap[node] as Layer;
                if (layer == null) return;

                Debug.WriteLine($"Layer: {layer.Name} ({layer.Operations.Count} operations)");
                var renameLayerItem = new MenuFlyoutItem
                {
                    Text = "Rename Layer"
                };
                renameLayerItem.Click += (s, args) =>
                {
                    var inputBox = new TextBox
                    {
                        Text = layer.Name,
                        Width = 200
                    };
                    var renameFlyout = new Flyout
                    {
                        Content = inputBox,
                        Placement = FlyoutPlacementMode.Bottom
                    };
                    inputBox.KeyDown += (ss, aa) =>
                    {
                        if (aa.Key == VirtualKey.Enter)
                        {
                            layer.Name = inputBox.Text;
                            RefreshLayerTree();
                            renameFlyout.Hide();
                        }
                    };
                    renameFlyout.ShowAt(element);
                };

                flyout.Items.Add(renameLayerItem);

                var operationFlyout = new MenuFlyoutItem
                {
                    Text = "Add Operation"
                };
                operationFlyout.Click += (s, args) =>
                {
                    SelectSubstractOrAddOperationFlyout(layer, element);
                };
                flyout.Items.Add(operationFlyout);
                
                var deleteLayer = new MenuFlyoutItem
                {
                    Text = "Delete Layer"
                };
                deleteLayer.Click += (s, args) =>
                {
                    currentImage.LayerManager.RemoveLayer(layer.Id);
                    RefreshLayerTree();
                };
                flyout.Items.Add(deleteLayer);
            }
            flyout.ShowAt(element);
        }

        private void RefreshLayerTree()
        {
            if (currentImage == null)
            {
                Debug.WriteLine("No image selected, cannot refresh layer tree.");
                return;
            }

            LayerTreeView.RootNodes.Clear();
            _nodeMap.Clear();

            foreach (var layer in currentImage.LayerManager.Layers)
            {
                var layerNode = new TreeViewNode
                {
                    Content = layer.Name,
                    IsExpanded = true
                };

                if (!layer.Operations.Any())
                {
                    currentImage.LayerManager.RemoveLayer(layer.Id);
                }
                _nodeMap[layerNode] = layer;

                foreach (var op in layer.Operations)
                {
                    var opNode = new TreeViewNode
                    {
                        Content = $"{op.BrushType} ({op.Mode})",
                    };
                    _nodeMap[opNode] = op;
                    layerNode.Children.Add(opNode);
                }

                LayerTreeView.RootNodes.Add(layerNode);
            }
        }

        private void OnToolSelectionChanged(object sender, int idx)
        {
            EditorScrollViewer.Visibility = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
            LayersUI.Visibility = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetEditableImage(EditableImage image)
        {
            currentImage = image;
            EditorStackPanel.Children.Clear();
            _categories.Clear();
            _sliderCache.Clear();
            BuildEditorUI();
            UpdateSliderUI();
            RequestFilterUpdate();
            UpdateResetButtonsVisibility();
            RefreshLayerTree();
        }

        /// <summary>
        /// Builds the editor UI with categories and sliders.
        /// </summary>
        private void BuildEditorUI()
        {
            var root = new EditorGroupExpander("Basic");

            AddCategory(root, "WhiteBalance", "White Balance", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Temperature", TempStyle),
                CreateSliderWithPreset("Tint",        TintStyle),
                CreateSeparator()
            });

            AddCategory(root, "Tone", "Tone", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Exposure"),
                CreateSliderWithPreset("Contrast"),
                CreateSeparator(),
                CreateSliderWithPreset("Highlights"),
                CreateSliderWithPreset("Shadows"),
                CreateSeparator(),
                CreateSliderWithPreset("Whites"),
                CreateSliderWithPreset("Blacks"),
                CreateSeparator()
            });

            AddCategory(root, "Presence", "Presence", new IEditorGroupItem[]
            {
                CreateSliderWithPreset("Texture"),
                CreateSliderWithPreset("Dehaze"),
                CreateSeparator(),
                CreateSliderWithPreset("Vibrance"),
                CreateSliderWithPreset("Saturation")
            });

            _panelManager!.AddCategory(root);

            var toneGroup = new EditorToneCurveGroup();
            toneGroup.CurveChanged += (key, lut) =>
            {
                if (currentImage == null) return;
                currentImage.Settings[key] = lut;
                RequestFilterUpdate();
            };

            var toneExpander = new EditorGroupExpander("Tone Curve");
            toneExpander.AddControl(toneGroup);
            _panelManager.AddCategory(toneExpander);
        }

        /// <summary>
        /// Creates a slider with preset values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private EditorSlider CreateSliderWithPreset(string key, EditorStyle? style = null)
        {
            var (min, max, def, dec, step) = GetSliderPreset(key);
            return CreateSlider(key, min, max, def, style, dec, step);
        }

        /// <summary>
        /// Gets the preset values for a slider based on its key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static (float min, float max, float def, int dec, float step) GetSliderPreset(string key) =>
            key switch
            {
                "Temperature" => (2000, 50000, 6500, 0, 100),
                "Exposure" => (-5, 5, 0, 2, 0.05f),
                "Contrast" => (-1, 1, 0, 2, 0.05f),
                "Tint" => (-150, 150, 0, 0, 1),
                _ => (-100, 100, 0, 0, 1)
            };

        /// <summary>
        /// Creates a slider with the specified parameters.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="def"></param>
        /// <param name="style"></param>
        /// <param name="decimals"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private EditorSlider CreateSlider(string key, float min, float max, float def,
                                          EditorStyle? style, int decimals, float step)
        {
            var slider = new EditorSlider(key, min, max, def, decimals, step);
            slider.OnValueChanged = v =>
            {
                if (currentImage == null) return;
                currentImage.Settings[key] = v;
                RequestFilterUpdate();
                UpdateResetButtonsVisibility();
            };

            style?.Let(slider.ApplyStyle);
            _panelManager!.RegisterControl(key, slider);
            _sliderCache[key] = slider;
            return slider;
        }

        /// <summary>
        /// Creates a separator for the UI.
        /// </summary>
        /// <returns></returns>
        private static EditorSeparator CreateSeparator() => new();

        /// <summary>
        /// Adds a category to the editor UI.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="key"></param>
        /// <param name="title"></param>
        /// <param name="items"></param>
        private void AddCategory(EditorGroupExpander parent, string key, string title, IEnumerable<IEditorGroupItem> items)
        {
            var cat = new EditorCategory(key, title);
            cat.OnResetClicked += ResetCategory;
            foreach (var x in items) cat.AddControl(x);

            _categories[key] = cat;
            parent.AddCategory(cat);
        }

        /// <summary>
        /// Resets the settings for a specific category.
        /// </summary>
        /// <param name="key"></param>
        private void ResetCategory(string key)
        {
            if (!_categories.TryGetValue(key, out var cat)) return;
            foreach (var sl in cat.GetItems().OfType<EditorSlider>())
            {
                sl.ResetToDefault();
                if (currentImage != null) currentImage.Settings[sl.Key] = sl.DefaultValue;
            }
            RequestFilterUpdate();
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void ResetAllClicked(object s, RoutedEventArgs e)
        {
            foreach (var sl in _sliderCache.Values)
            {
                sl.ResetToDefault();
                if (currentImage != null) currentImage.Settings[sl.Key] = sl.DefaultValue;
            }
            RequestFilterUpdate();
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Updates the UI of the sliders based on the current image settings.
        /// </summary>
        private void UpdateSliderUI()
        {
            if (currentImage == null) return;
            foreach (var (k, v) in currentImage.Settings)
                if (_sliderCache.TryGetValue(k, out var sl)) sl.SetValue((float)v);
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Updates the visibility of the reset buttons based on the current settings.
        /// </summary>
        private void UpdateResetButtonsVisibility()
        {
            if (currentImage == null) return;

            foreach (var cat in _categories.Values)
            {
                bool modified = cat.GetItems().OfType<EditorSlider>()
                                   .Any(sl => Math.Abs(sl.GetValue() - sl.DefaultValue) > 0.01f);
                cat.SetResetVisible(modified);
            }

            bool any = _sliderCache.Values
                       .Any(sl => Math.Abs(sl.GetValue() - sl.DefaultValue) > 0.01f);
            ResetAllButton.Visibility = any ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Requests an update of the filter settings.
        /// </summary>
        public void RequestFilterUpdate()
        {
            var old = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            old?.Cancel();
            old?.Dispose();

            if (Interlocked.CompareExchange(ref _renderRunning, 1, 0) != 0)
            {
                _pendingUpdate = true;
                return;
            }

            _ = RunPipelineAsync(_cts.Token);
        }

        /// <summary>
        /// Runs the image processing pipeline asynchronously.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task RunPipelineAsync(CancellationToken token)
        {
            try
            {
                if (currentImage == null) return;

                if (currentImage.PreviewBitmap != null)
                {
                    var prev = await ImageProcessingManager
                        .ApplyFiltersAsync(currentImage.PreviewBitmap,
                                           currentImage.Settings, token);
                    token.ThrowIfCancellationRequested();

                    currentImage.EditedPreviewBitmap = prev;
                    var up = ImageProcessingManager.Upscale(
                                prev, currentImage.OriginalBitmap.Height, true);
                    OnEditorImageUpdated?.Invoke(up);
                }

                var full = await ImageProcessingManager
                    .ApplyFiltersAsync(currentImage.OriginalBitmap,
                                       currentImage.Settings, token);
                token.ThrowIfCancellationRequested();

                currentImage.EditedBitmap = full;
                OnEditorImageUpdated?.Invoke(SKImage.FromBitmap(full));
            }
            catch (OperationCanceledException)
            {
                /// Interrupted : normal
            }
            finally
            {
                Interlocked.Exchange(ref _renderRunning, 0);

                if (Interlocked.Exchange(ref _pendingUpdate, false))
                    RequestFilterUpdate();
            }
        }


        /// <summary>
        /// Checks if the Control key is pressed.
        /// </summary>
        /// <returns></returns>
        private static bool IsCtrlDown()
        {
            var win = Microsoft.UI.Xaml.Window.Current;
            return win != null &&
                   win.CoreWindow.GetKeyState(VirtualKey.Control)
                      .HasFlag(CoreVirtualKeyStates.Down);
        }

        /// <summary>
        /// Handles the key down event for undo and redo operations.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object? sender, KeyRoutedEventArgs e)
        {
            if (!IsCtrlDown() || currentImage == null) return;

            switch (e.Key)
            {
                case VirtualKey.Z:
                    if (currentImage.Undo())
                    {
                        UpdateSliderUI();
                        RequestFilterUpdate();
                    }
                    break;

                case VirtualKey.Y:
                    if (currentImage.Redo())
                    {
                        UpdateSliderUI();
                        RequestFilterUpdate();
                    }
                    break;
            }
        }
    }

    internal static class Ext
    {
        public static void Let<T>(this T obj, Action<T> block) where T : class => block(obj);
    }
}
