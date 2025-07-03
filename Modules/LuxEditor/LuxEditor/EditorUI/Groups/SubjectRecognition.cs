using LuxEditor.EditorUI.Interfaces;
using LuxEditor.Models;
using Luxoria.Algorithm.GrabCut;
using Luxoria.Algorithm.YoLoDetectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LuxEditor.EditorUI.Groups
{
    public class SubjectRecognition : UserControl, IEditorGroupItem
    {
        private Grid _root;
        private Button _startButton;
        private StackPanel _progressPanel;
        private ProgressRing _spinner;
        private TextBlock _statusText;

        private StackPanel _ROIPanel;
        private ListView _ROIView;
        
        private EditableImage? _selectedImage;
        private Lazy<YoLoDetectModelAPI>? _detectionAPI;
        private Lazy<GrabCut> _grabCut => new(() => new GrabCut());

        public event Action<SKBitmap> BlurAppliedEvent;

        public SubjectRecognition(Lazy<YoLoDetectModelAPI> detectionAPI)
        {
            _detectionAPI = detectionAPI;
            BuildUI();
        }

        private void BuildUI()
        {
            // Root container
            _root = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Start button
            _startButton = new Button
            {
                Content = "Start Recognition",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _startButton.Click += OnStartClicked;

            _spinner = new ProgressRing
            {
                IsActive = false,
                Width = 24,
                Height = 24,
                Visibility = Visibility.Collapsed
            };

            _statusText = new TextBlock
            {
                Text = "Extracting...",
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            _progressPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { _spinner, _statusText }
            };

            _ROIView = new();

            _ROIPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { _ROIView },
                Visibility = Visibility.Collapsed
            };

            _root.Children.Add(_startButton);
            _root.Children.Add(_progressPanel);
            _root.Children.Add(_ROIPanel);
        }

        private async void OnStartClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedImage == null)
            {
                Debug.WriteLine("No image selected for recognition.");
                return;
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                _startButton.Visibility = Visibility.Collapsed;
                _spinner.IsActive = true;
                _spinner.Visibility = Visibility.Visible;
                _statusText.Visibility = Visibility.Visible;
                _progressPanel.Visibility = Visibility.Visible;
            });

            Debug.WriteLine($"Starting for : {_selectedImage.Id}");

            DispatcherQueue.TryEnqueue(() => _statusText.Text = "Converting bitmap...");

            Debug.WriteLine($"Converting bitmap to image...");
            string outputPath = Path.Combine(Path.GetTempPath(), $"{_selectedImage.Id}.png");
            using (var data = _selectedImage.EditedBitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                if (data == null)
                    throw new InvalidOperationException("Failed to encode bitmap as PNG");

                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }

            DispatcherQueue.TryEnqueue(() => _statusText.Text = "Running detection...");

            var result = await Task.Run(() =>
            {
                if (_detectionAPI == null)
                    throw new InvalidOperationException("Detection API is not initialized");
                return _detectionAPI.Value.Detect(outputPath);
            });
            DispatcherQueue.TryEnqueue(() => _statusText.Text = "Detection completed");

            Debug.WriteLine($"Detection completed, Found {result?.Count} ROI(s)");

            DispatcherQueue.TryEnqueue(() =>
            {
                _spinner.IsActive = false;
                _spinner.Visibility = Visibility.Collapsed;
                _statusText.Visibility = Visibility.Collapsed;
                _progressPanel.Visibility = Visibility.Collapsed;
                _startButton.Visibility = Visibility.Collapsed;
                _ROIPanel.Visibility = Visibility.Visible;
            });

            foreach (var roi in result ?? [])
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var roiItem = new TextBlock
                    {
                        Text = $"ID: {roi.ClassId} ({roi.Confidence * 100:#.##}%)"
                    };

                    var flyout = new MenuFlyout();

                    var highlight = new MenuFlyoutItem { Text = "Highlight" };
                    highlight.Click += (s, e) =>
                    {
                        Debug.WriteLine($"Highlight clicked for {roi.ClassId}");
                    };
                    flyout.Items.Add(highlight);

                    var viewDetails = new MenuFlyoutItem { Text = "Details" };
                    viewDetails.Click += (s, e) =>
                    {
                        Debug.WriteLine($"View Details clicked for {roi.ClassId}");
                    };
                    flyout.Items.Add(viewDetails);

                    flyout.Items.Add(new MenuFlyoutSeparator());
                    
                    var blur = new MenuFlyoutItem { Text = "Apply Blur effect" };
                    blur.Click += async (s, e) =>
                    {
                        Debug.WriteLine($"Blur clicked for {roi.ClassId}");
                        if (_selectedImage != null)
                        {
                            string outputPath = Path.Combine(Path.GetTempPath(), $"{_selectedImage.Id}_grabcut.png");
                            using (var data = _selectedImage.EditedBitmap.Encode(SKEncodedImageFormat.Png, 100))
                            {
                                if (data == null)
                                    throw new InvalidOperationException("Failed to encode bitmap as PNG");

                                using (var stream = File.OpenWrite(outputPath))
                                {
                                    data.SaveTo(stream);
                                }
                            }
                            string outPath = Path.Combine(Path.GetTempPath(), $"{_selectedImage.Id}_grabcut_ret.png");
                            Debug.WriteLine("Applying GrabCut...");
                            await Task.Run(() =>
                            {
                                if (_grabCut == null)
                                    throw new InvalidOperationException("GrabCut is not initialized");
                                _grabCut.Value.Exec(outputPath, outPath, roi.Box.X, roi.Box.Y, roi.Box.Width, roi.Box.Height, 5, false, Color.White, Color.Black);
                            });
                            Debug.WriteLine("GrabCut applied, applying blur...");
                            SKBitmap mask = SKBitmap.Decode(outPath);
                            //SKBitmap updatedWithBlur = BlurBackground(_selectedImage.EditedBitmap, mask, 10.0f);

                            BlurAppliedEvent.Invoke(mask);
                        }
                    };
                    flyout.Items.Add(blur);

                    flyout.Items.Add(new MenuFlyoutSeparator());

                    var remove = new MenuFlyoutItem { Text = "Remove" };
                    remove.Click += (s, e) =>
                    {
                        Debug.WriteLine($"Remove clicked for {roi.ClassId}");
                        _ROIView.Items.Remove(roiItem);
                    };
                    flyout.Items.Add(remove);


                    roiItem.Tapped += (s, args) =>
                    {
                        flyout.ShowAt(roiItem);
                    };

                    _ROIView.Items.Add(roiItem);
                });
            }
        }

        public UIElement GetElement()
        {
            return _root;
        }

        public void SetImage(EditableImage image) => _selectedImage = image;

        /// <summary>
        /// Extracts an embedded resource and writes it to a temporary file.
        /// </summary>
        /// <param name="resourceName">The resource name</param>
        /// <returns>Path to the extracted file</returns>
        public static string ExtractEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource {resourceName} not found");

            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(resourceName));
            using FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);

            return tempFile;
        }
    }
}
