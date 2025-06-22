using LuxEditor.Controls;
using LuxEditor.EditorUI;
using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Controls.ToolControls;
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
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        public EditableImage? CurrentImage;
        private EditorPanelManager? _panelManager;
        private readonly Dictionary<string, EditorCategory> _categories = new();
        private readonly ConcurrentDictionary<string, EditorSlider> _sliderCache = new();
        private CancellationTokenSource? _cts;
        private int _renderRunning;
        private bool _pendingUpdate;
        public event Action<SKImage>? OnEditorImageUpdated;
        public event Action<bool>? IsCropModeChanged;
        public event Action? InvalidateCrop;
        private CropController _crop;
        public event Action<CropController.CropBox>? CropBoxChanged;
        private bool _isCropEditing;

        public bool LockAspectToggleIsOn => LockAspectToggle.IsOn;

        private readonly Dictionary<TreeViewNode, object> _nodeMap = new();
        private readonly List<Layer> _observedLayers = new();

        private EditorToneCurveGroup _toneGroup;
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
            LayerTreeView.CanDragItems = false;
            LayerTreeView.CanReorderItems = false;

            CurrentImage = editableImage;

            if (CurrentImage != null)
            {
                CurrentImage.LayerManager.Layers.CollectionChanged += (_, __) => RefreshLayerTree();
                RefreshLayerTree();

                OnToolSelectionChanged(this, 0);
            }

        }

        /// <summary>Called by PhotoViewer when the user enters crop mode.</summary>
        public void BeginCropEditing()
        {
            if (_isCropEditing) return;
            _isCropEditing = true;
            RequestFilterUpdate();
        }

        /// <summary>Called when the user valide ou annule le crop.</summary>
        public void EndCropEditing()
        {
            if (!_isCropEditing) return;
            _isCropEditing = false;
            RequestFilterUpdate();
        }

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            LockAspectToggle.Toggled += (_, __) =>
            {
                _crop.LockAspectRatio = LockAspectToggle.IsOn;
                CropChanged();
            };

            AspectPresetCombo.SelectionChanged += (_, __) =>
            {
                switch (AspectPresetCombo.SelectedIndex)
                {
                    case 0: _crop.Reset(); break;
                    case 1: _crop.ApplyPresetRatio(4f / 3f); break;
                    case 2: _crop.ApplyPresetRatio(16f / 9f); break;
                    case 3: _crop.ApplyPresetRatio(16f / 10f); break;
                    case 4: _crop.ApplyPresetRatio(1f); break;
                    case 5: _crop.ApplyPresetRatio(4f / 5f); break;
                    case 6: EnableCustomInputs(true); return;
                }
                EnableCustomInputs(false);
                CropChanged();
            };

            CustomWidthInput.ValueChanged += (_, e) =>
                _crop.SetSize((float)e.NewValue,
                              _crop.LockAspectRatio ? _crop.Box.Height
                                                    : (float)CustomHeightInput.Value);

            CustomHeightInput.ValueChanged += (_, e) =>
            {
                if (!_crop.LockAspectRatio)
                    _crop.SetSize((float)CustomWidthInput.Value, (float)e.NewValue);
            };

            RotateAngleInput.ValueChanged += (_, e) =>
            { _crop.SetAngle((float)e.NewValue); CropChanged(); };
        }

        private void EnableCustomInputs(bool on)
        {
            CustomWidthInput.IsEnabled = on;
            CustomHeightInput.IsEnabled = on;
        }

        private void CropChanged()
        {
            RefreshCropInputs();
            CropBoxChanged?.Invoke(_crop.Box);
        }

        private void RefreshCropInputs()
        {
            CustomWidthInput.Value = _crop.Box.Width;
            CustomHeightInput.Value = _crop.Box.Height;
            RotateAngleInput.Value = _crop.Box.Angle;
            LockAspectToggle.IsOn = _crop.LockAspectRatio;
        }

        public void AttachCropController(CropController ctl)
        {
            _crop = ctl;
            _crop.BoxChanged += RefreshCropInputs;
        }


        private void OnAddLayerClicked(object sender, RoutedEventArgs e)
        {
            OpenBrushSelectionFlyout();
        }

        private void BrushButton_Click(ToolType type)
        {
            if (CurrentImage == null)
            {
                Debug.WriteLine("No image selected, cannot add layer.");
                return;
            }
            CurrentImage.LayerManager.AddLayer(type);
            RefreshLayerTree();
        }

        private void OpenBrushSelectionFlyout()
        {
            var flyout = new MenuFlyout();

            var brushButton = new MenuFlyoutItem
            {
                Text = "Brush",
            };
            brushButton.Click += (s, e) => { BrushButton_Click(ToolType.Brush); flyout.Hide(); };
            flyout.Items.Add(brushButton);

            var linearGradientButton = new MenuFlyoutItem
            {
                Text = "Linear Gradient",

            };
            linearGradientButton.Click += (s, e) => { BrushButton_Click(ToolType.LinearGradient); flyout.Hide(); };
            flyout.Items.Add(linearGradientButton);

            var radialGradientButton = new MenuFlyoutItem
            {
                Text = "Radial Gradient",

            };
            radialGradientButton.Click += (s, e) => { BrushButton_Click(ToolType.RadialGradient); flyout.Hide(); };
            flyout.Items.Add(radialGradientButton);

            flyout.ShowAt(AddLayerBtn);
        }


        private void OnRemoveLayerClicked(object sender, RoutedEventArgs e)
        {
            if (CurrentImage == null || LayerTreeView.SelectedNodes.Count == 0)
            {
                Debug.WriteLine("No image or layer selected, cannot remove layer.");
                return;
            }
            CurrentImage.LayerManager.RemoveLayer();
            RefreshLayerTree();
        }

        private void OnLayerTreeSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
        {
            OperationDetailsHost.Visibility = Visibility.Collapsed;
            OperationDetailsHost.Content = null;

            if (e.AddedItems.Count == 0 || e.AddedItems[0] is not TreeViewNode node)
                return;

            if (_nodeMap[node] is Layer layer)
            {
                layer.DetailsPanel.SetLayer(layer);
                OperationDetailsHost.Content = layer.DetailsPanel;
                OperationDetailsHost.Visibility = Visibility.Visible;
                CurrentImage.LayerManager.SelectedLayer = layer;
                CurrentImage.LayerManager.OnLayerChanged?.Invoke();
            }
            if (_nodeMap[node] is MaskOperation op)
            {
                CurrentImage.LayerManager.SelectedLayer = CurrentImage.LayerManager.GetLayerByOperation(op.Id);
                CurrentImage.LayerManager.SelectedLayer.SelectedOperation = op;
                CurrentImage.LayerManager.OnOperationChanged?.Invoke();
            }
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
            if (CurrentImage == null)
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
                CurrentImage.LayerManager.AddOperation(layer.Id, CurrentImage.LayerManager.CreateMaskOperation(ToolType.Brush, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract));
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
                CurrentImage.LayerManager.AddOperation(layer.Id, CurrentImage.LayerManager.CreateMaskOperation(ToolType.LinearGradient, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract));
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

                CurrentImage.LayerManager.AddOperation(layer.Id, CurrentImage.LayerManager.CreateMaskOperation(ToolType.RadialGradient, isAdded ? BooleanOperationMode.Add : BooleanOperationMode.Subtract)); RefreshLayerTree();
                flyout.Hide();
            };
            flyout.Items.Add(radialGradientButton);
            flyout.ShowAt(element);

            CurrentImage.LayerManager.OnOperationChanged?.Invoke();
            
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

            if (CurrentImage == null)
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

                var layer = CurrentImage.LayerManager.GetLayerByOperation(op.Id);
                if (layer == null)
                {
                    Debug.WriteLine("Operation is not associated with any layer.");
                    return;
                }
                layer.SelectedOperation = op;

                Debug.WriteLine($"Operation: {op.Tool.ToolType} ({op.Mode})");
                var deleteOpItem = new MenuFlyoutItem
                {
                    Text = "Delete Operation"
                };
                deleteOpItem.Click += (s, args) =>
                {
                    CurrentImage.LayerManager.RemoveOperation(op.Id);
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
                            op.Tool.booleanOperationMode = mode;
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
                    CurrentImage.LayerManager.RemoveLayer(layer.Id);
                    RefreshLayerTree();
                };
                flyout.Items.Add(deleteLayer);
            }
            flyout.ShowAt(element);
        }

        private void RefreshLayerTree()
        {
            if (CurrentImage == null) return;
            LayerTreeView.RootNodes.Clear();
            _nodeMap.Clear();

            foreach (var layer in CurrentImage.LayerManager.Layers.ToArray())
            {
                if (!layer.Operations.Any())
                {
                    CurrentImage.LayerManager.RemoveLayer(layer.Id);
                    continue;
                }

                var layerNode = new TreeViewNode { Content = layer.Name, IsExpanded = true };
                _nodeMap[layerNode] = layer;

                foreach (var op in layer.Operations.ToArray())
                {
                    var opNode = new TreeViewNode { Content = $"{op.Tool.ToolType} ({op.Mode})" };
                    _nodeMap[opNode] = op;
                    layerNode.Children.Add(opNode);
                }
                LayerTreeView.RootNodes.Add(layerNode);
            }
        }

        private void OnToolSelectionChanged(object sender, int idx)
        {
            IsCropModeChanged?.Invoke(idx == 1);
            EditorScrollViewer.Visibility = (idx == 0) ? Visibility.Visible : Visibility.Collapsed;
            CropUI.Visibility = (idx == 1) ? Visibility.Visible : Visibility.Collapsed;
            LayersUI.Visibility = (idx == 2) ? Visibility.Visible : Visibility.Collapsed;
            ResetAllButton.Visibility = (idx == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetEditableImage(EditableImage image)
        {
            CurrentImage = image;

            foreach (var l in _observedLayers) l.PropertyChanged -= OnLayerModified;
            _observedLayers.Clear();
            image.LayerManager.OnOperationChanged += RequestFilterUpdate;
            image.LayerManager.OnLayerChanged += RequestFilterUpdate;
            foreach (var l in image.LayerManager.Layers)
            {
                l.PropertyChanged += OnLayerModified;
                _observedLayers.Add(l);
            }
            image.LayerManager.Layers.CollectionChanged += (_, __) =>
            {
                foreach (var lay in _observedLayers)
                    lay.PropertyChanged -= OnLayerPropertyChanged;
                _observedLayers.Clear();
                foreach (var lay in image.LayerManager.Layers)
                {
                    lay.PropertyChanged += OnLayerPropertyChanged;
                    _observedLayers.Add(lay);
                }
                RequestFilterUpdate();
            };

            EditorStackPanel.Children.Clear();
            _categories.Clear();
            _sliderCache.Clear();
            BuildEditorUI();
            UpdateSliderUI();
            _toneGroup.RefreshCurves(CurrentImage.Settings);
            RequestFilterUpdate();
            UpdateResetButtonsVisibility();
            RefreshLayerTree();
        }

        private void OnLayerModified(object? s, PropertyChangedEventArgs e) => RequestFilterUpdate();

        private void OnLayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layer.Filters) ||
                e.PropertyName == nameof(Layer.Strength) ||
                e.PropertyName == nameof(Layer.Invert) ||
                e.PropertyName == nameof(Layer.Visible))
                RequestFilterUpdate();
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

            _toneGroup = new EditorToneCurveGroup(false);
            _toneGroup.CurveChanged += (key, lut) =>
            {
                if (CurrentImage == null) return;
                CurrentImage.Settings[key] = lut;
                RequestFilterUpdate();
            };

            var toneExpander = new EditorGroupExpander("Tone Curve");
            toneExpander.AddControl(_toneGroup);
            _panelManager.AddCategory(toneExpander);
            CurrentImage.SaveState();
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
                if (CurrentImage == null) return;
                CurrentImage.Settings[key] = v;
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
                if (CurrentImage != null) CurrentImage.Settings[sl.Key] = sl.DefaultValue;
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
                if (CurrentImage != null) CurrentImage.Settings[sl.Key] = sl.DefaultValue;
            }
            RequestFilterUpdate();
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Updates the UI of the sliders based on the current image settings.
        /// </summary>
        private void UpdateSliderUI()
        {
            if (CurrentImage == null) return;
            foreach (var (k, v) in CurrentImage.Settings)
                if (_sliderCache.TryGetValue(k, out var sl)) sl.SetValue((float)v);
            UpdateResetButtonsVisibility();
        }

        /// <summary>
        /// Updates the visibility of the reset buttons based on the current settings.
        /// </summary>
        private void UpdateResetButtonsVisibility()
        {
            if (CurrentImage == null) return;

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

        private async Task RunPipelineAsync(CancellationToken token)
        {
            _pendingUpdate = false;
            try
            {
                if (CurrentImage == null) return;

                async Task<SKBitmap> RenderAsync(SKBitmap src)
                {
                    var srcForFilters = _isCropEditing ? src : ApplyCrop(src);

                    var baseBmp = await ImageProcessingManager
                                         .ApplyFiltersAsync(srcForFilters, CurrentImage.Settings, token);

                    using var surf = SKSurface.Create(new SKImageInfo(baseBmp.Width, baseBmp.Height));
                    var can = surf.Canvas;
                    can.DrawBitmap(baseBmp, 0, 0);

                    var result = new SKBitmap(baseBmp.Width, baseBmp.Height);
                    surf.ReadPixels(result.Info, result.GetPixels(), result.RowBytes, 0, 0);

                    var layers = CurrentImage.LayerManager.Layers.ToArray();
                    foreach (var layer in layers.Where(l => l.Visible))
                    {
                        using var mask = BuildLayerMask(layer, baseBmp.Width, baseBmp.Height);
                        if (mask == null) continue;

                        if (layer.HasActiveFilters())
                        {
                            using var filtered = await ImageProcessingManager
                                                       .ApplyFiltersAsync(result, layer.Filters, token);
                            DrawMasked(can, filtered, mask, layer);
                        }
                        can.Flush();
                        surf.ReadPixels(result.Info, result.GetPixels(), result.RowBytes, 0, 0);
                    }

                    var outBmp = new SKBitmap(baseBmp.Width, baseBmp.Height);
                    surf.ReadPixels(outBmp.Info, outBmp.GetPixels(), outBmp.RowBytes, 0, 0);
                    return outBmp;
                }

                if (CurrentImage.PreviewBitmap != null)
                {
                    var prev = await RenderAsync(CurrentImage.PreviewBitmap);
                    CurrentImage.EditedPreviewBitmap = prev;
                    var upscaled = ImageProcessingManager.Upscale(prev,
                                                                  CurrentImage.OriginalBitmap.Height,
                                                                  true);
                    OnEditorImageUpdated?.Invoke(upscaled);
                }

                var full = await RenderAsync(CurrentImage.OriginalBitmap);
                CurrentImage.EditedBitmap = full;
                OnEditorImageUpdated?.Invoke(SKImage.FromBitmap(full));
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Interlocked.Exchange(ref _renderRunning, 0);
                if (_pendingUpdate)
                {
                    _pendingUpdate = false;
                    RequestFilterUpdate();
                }
            }
        }

        private static void DrawMasked(SKCanvas c, SKBitmap content, SKBitmap mask, Layer lay)
        {
            float k = Math.Clamp((float)lay.Strength / 100f, 0f, 2f);

            using var contSh = SKShader.CreateBitmap(content, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
            using var maskSh = SKShader.CreateBitmap(mask, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

            using var compSh = lay.Invert
                ? SKShader.CreateCompose(contSh, maskSh, SKBlendMode.DstOut)
                : SKShader.CreateCompose(maskSh, contSh, SKBlendMode.SrcIn);

            void Paint(float alpha)
            {
                if (alpha <= 0f) return;
                using var p = new SKPaint
                {
                    Shader = compSh,
                    Color = SKColors.White.WithAlpha((byte)(alpha * 255))
                };
                c.DrawRect(SKRect.Create(mask.Width, mask.Height), p);
            }

            if (k <= 1f)
            {
                Paint(k);
            }
            else
            {
                Paint(1f);
                Paint(k - 1f);
            }
        }

        private static SKBitmap? BuildLayerMask(Layer lay, int w, int h)
        {
            if (lay.Operations.Count == 0) return null;

            var bmp = new SKBitmap(w, h);
            using var surf = SKSurface.Create(new SKImageInfo(w, h));
            var can = surf.Canvas;

            foreach (var op in lay.Operations.ToArray())
            {
                var m = op.Tool?.GetResult();
                if (m == null) continue;

                using var p = new SKPaint
                {
                    BlendMode = op.Mode == BooleanOperationMode.Add
                                    ? SKBlendMode.SrcOver
                                    : SKBlendMode.DstOut,
                    FilterQuality = SKFilterQuality.High
                };
                can.DrawBitmap(m, new SKRect(0, 0, w, h), p);
            }

            can.Flush();
            surf.ReadPixels(bmp.Info, bmp.GetPixels(), bmp.RowBytes, 0, 0);
            return bmp;
        }



        private void Undo_Invoked(KeyboardAccelerator sender,
                                  KeyboardAcceleratorInvokedEventArgs e)
        {
            if (CurrentImage?.Undo() == true) AfterHistoryJump();
            e.Handled = true;
        }

        private void Redo_Invoked(KeyboardAccelerator sender,
                                  KeyboardAcceleratorInvokedEventArgs e)
        {
            if (CurrentImage?.Redo() == true) AfterHistoryJump();
            e.Handled = true;
        }

        private void AfterHistoryJump()
        {
            UpdateSliderUI();
            RefreshLayerTree();
            RequestFilterUpdate();
            _toneGroup.RefreshCurves(CurrentImage.Settings);
        }

        /// <summary>Return the bitmap after applying the current crop.</summary>
        private SKBitmap ApplyCrop(SKBitmap src)
        {
            if (CurrentImage == null) return src;
            var box = CurrentImage.Crop;
            return CropProcessor.Apply(src, box);
        }


    }

    internal static class Ext
    {
        public static void Let<T>(this T obj, Action<T> block) where T : class => block(obj);
    }
}
