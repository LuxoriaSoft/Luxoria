using LuxEditor.EditorUI.Interfaces;
using LuxEditor.Models;
using Luxoria.Algorithm.GrabCut;
using Luxoria.Algorithm.YoLoDetectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LuxEditor.EditorUI.Groups
{
    public class SubjectRecognition : UserControl, IEditorGroupItem
    {
        private StackPanel _root;
        private Button _startButton;
        private StackPanel _progressPanel;
        private ProgressRing _spinner;
        private TextBlock _statusText;

        private TextBlock _detectionStatusText;

        private ToggleSwitch _showOverlayToggle;
        private Button _resetDetectionButton;

        private ToggleSwitch _blurToggle;
        private TextBlock _blurIntensityLabel;
        private Slider _blurIntensitySlider;
        private TextBox _blurValueBox;
        private Grid _blurSliderGrid;

        private EditableImage? _selectedImage;
        private Lazy<YoLoDetectModelAPI>? _detectionAPI;
        private Lazy<GrabCut> _grabCut => new(() => new GrabCut());

        private List<DetectedSubject> _detectedSubjects = new();

        public event Action<List<DetectedSubject>> SubjectsDetectedEvent;
        public event Action FilterUpdateRequested;
        public event Action<bool> ShowOverlayToggled;

        public SubjectRecognition(Lazy<YoLoDetectModelAPI> detectionAPI)
        {
            _detectionAPI = detectionAPI;
            BuildUI();
        }

        private void BuildUI()
        {
            // Root container - simple vertical StackPanel like other controls
            _root = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };

            // Start button
            _startButton = new Button
            {
                Content = "Detect Subjects",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            _startButton.Click += OnStartClicked;

            // Progress panel
            _spinner = new ProgressRing
            {
                IsActive = false,
                Width = 20,
                Height = 20,
                Visibility = Visibility.Collapsed
            };

            _statusText = new TextBlock
            {
                Text = "Processing...",
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(8, 0, 0, 0),
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                FontSize = 12
            };

            _progressPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { _spinner, _statusText }
            };

            // Status text
            _detectionStatusText = new TextBlock
            {
                Text = "Click 'Detect Subjects' to find subjects.",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Visible,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150)),
                FontSize = 12
            };

            // Show overlay toggle
            _showOverlayToggle = new ToggleSwitch
            {
                Header = "Show Overlays",
                IsOn = true,
                Visibility = Visibility.Collapsed
            };
            _showOverlayToggle.Toggled += (s, e) => ShowOverlayToggled?.Invoke(_showOverlayToggle.IsOn);

            // Reset button
            _resetDetectionButton = new Button
            {
                Content = "Clear Detection",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Visibility = Visibility.Collapsed
            };
            _resetDetectionButton.Click += OnResetDetectionClicked;

            // Blur toggle
            _blurToggle = new ToggleSwitch
            {
                Header = "Background Blur",
                IsOn = false,
                Visibility = Visibility.Collapsed
            };
            _blurToggle.Toggled += OnBlurToggled;

            // Blur intensity label
            _blurIntensityLabel = new TextBlock
            {
                Text = "Blur Strength",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 2),
                Visibility = Visibility.Collapsed
            };

            // Blur slider with value box (like EditorSlider)
            _blurSliderGrid = new Grid();
            _blurSliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _blurSliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _blurSliderGrid.Visibility = Visibility.Collapsed;

            _blurIntensitySlider = new Slider
            {
                Minimum = 1,
                Maximum = 50,
                Value = 7,
                StepFrequency = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 5, 0)
            };
            _blurIntensitySlider.ValueChanged += OnBlurIntensityChanged;

            _blurValueBox = new TextBox
            {
                Text = "7",
                Width = 40,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                IsReadOnly = true
            };

            Grid.SetColumn(_blurIntensitySlider, 0);
            Grid.SetColumn(_blurValueBox, 1);
            _blurSliderGrid.Children.Add(_blurIntensitySlider);
            _blurSliderGrid.Children.Add(_blurValueBox);

            // Add all to root
            _root.Children.Add(_startButton);
            _root.Children.Add(_progressPanel);
            _root.Children.Add(_detectionStatusText);
            _root.Children.Add(_showOverlayToggle);
            _root.Children.Add(_resetDetectionButton);
            _root.Children.Add(_blurToggle);
            _root.Children.Add(_blurIntensityLabel);
            _root.Children.Add(_blurSliderGrid);
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

            // Clear previous subjects
            _detectedSubjects.Clear();

            // Create DetectedSubject objects from ROIs
            foreach (var roi in result ?? [])
            {
                var subject = new DetectedSubject
                {
                    ClassId = roi.ClassId,
                    Label = $"Subject {roi.ClassId}",
                    Confidence = roi.Confidence,
                    BoundingBox = roi.Box,
                    IsSelected = false
                };
                _detectedSubjects.Add(subject);
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                _spinner.IsActive = false;
                _spinner.Visibility = Visibility.Collapsed;
                _statusText.Visibility = Visibility.Collapsed;
                _progressPanel.Visibility = Visibility.Collapsed;
                _startButton.Visibility = Visibility.Collapsed;

                // Update status text
                if (_detectedSubjects.Count > 0)
                {
                    _detectionStatusText.Text = $"✓ {_detectedSubjects.Count} subject(s) found. Click them to select.";
                    _detectionStatusText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 200, 50));
                    _showOverlayToggle.Visibility = Visibility.Visible;
                    _resetDetectionButton.Visibility = Visibility.Visible;
                    _blurToggle.Visibility = Visibility.Visible;
                    _blurIntensityLabel.Visibility = Visibility.Visible;
                    _blurSliderGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    _detectionStatusText.Text = "No subjects detected";
                    _detectionStatusText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 100, 50));
                }
                _detectionStatusText.Visibility = Visibility.Visible;
            });

            // Notify PhotoViewer about detected subjects
            SubjectsDetectedEvent?.Invoke(_detectedSubjects);

            // Explicitly show the overlay after detection
            ShowOverlayToggled?.Invoke(true);

            Debug.WriteLine($"Detected {_detectedSubjects.Count} subjects, sent to PhotoViewer");

            // Save detected subjects to image settings for persistence
            if (_selectedImage != null)
            {
                _selectedImage.Settings["DetectedSubjects"] = _detectedSubjects;
            }
        }

        private void OnResetDetectionClicked(object sender, RoutedEventArgs e)
        {
            // Clear detected subjects
            _detectedSubjects.Clear();

            // Clear from settings
            if (_selectedImage?.Settings != null && _selectedImage.Settings.ContainsKey("DetectedSubjects"))
            {
                _selectedImage.Settings.Remove("DetectedSubjects");
            }

            // Notify PhotoViewer to clear overlay
            SubjectsDetectedEvent?.Invoke(_detectedSubjects);

            // Reset UI
            DispatcherQueue.TryEnqueue(() =>
            {
                _startButton.Visibility = Visibility.Visible;
                _detectionStatusText.Text = "Click 'Detect Subjects' to find subjects.";
                _detectionStatusText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150));
                _showOverlayToggle.Visibility = Visibility.Collapsed;
                _resetDetectionButton.Visibility = Visibility.Collapsed;
                _blurToggle.Visibility = Visibility.Collapsed;
                _blurIntensityLabel.Visibility = Visibility.Collapsed;
                _blurSliderGrid.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Applies GrabCut to extract mask for a specific subject
        /// Called when user selects a subject in PhotoViewer
        /// </summary>
        public async Task<SKBitmap?> ExtractSubjectMask(DetectedSubject subject)
        {
            if (_selectedImage == null) return null;

            string outputPath = Path.Combine(Path.GetTempPath(), $"{_selectedImage.Id}_grabcut_{subject.Id}.png");
            using (var data = _selectedImage.EditedBitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                if (data == null)
                    return null;

                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }

            string outPath = Path.Combine(Path.GetTempPath(), $"{_selectedImage.Id}_grabcut_{subject.Id}_ret.png");
            Debug.WriteLine($"Applying GrabCut for subject {subject.Id}...");

            await Task.Run(() =>
            {
                _grabCut.Value.Exec(
                    outputPath,
                    outPath,
                    subject.BoundingBox.X,
                    subject.BoundingBox.Y,
                    subject.BoundingBox.Width,
                    subject.BoundingBox.Height,
                    5,
                    false,
                    Color.White,
                    Color.Black
                );
            });

            Debug.WriteLine($"GrabCut completed for subject {subject.Id}");
            SKBitmap mask = SKBitmap.Decode(outPath);
            subject.Mask = mask;

            return mask;
        }

        public UIElement GetElement()
        {
            return _root;
        }

        public void SetImage(EditableImage image)
        {
            _selectedImage = image;

            // First, restore blur settings to sync toggles
            bool hasBlurSettings = false;
            if (_selectedImage?.Settings != null && _selectedImage.Settings.TryGetValue("Blur", out var blurObj) && blurObj is Dictionary<string, object> blurSettings)
            {
                hasBlurSettings = true;

                // Restore blur toggle state
                if (blurSettings.TryGetValue("State", out var stateObj) && stateObj is bool state)
                {
                    DispatcherQueue.TryEnqueue(() => _blurToggle.IsOn = state);
                }
                else
                {
                    DispatcherQueue.TryEnqueue(() => _blurToggle.IsOn = false);
                }

                // Restore blur intensity
                if (blurSettings.TryGetValue("Sigma", out var sigmaObj))
                {
                    try
                    {
                        float sigma = Convert.ToSingle(sigmaObj);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            _blurIntensitySlider.Value = sigma;
                            _blurValueBox.Text = ((int)sigma).ToString();
                        });
                    }
                    catch { }
                }
            }
            else
            {
                // No blur settings, reset to defaults
                DispatcherQueue.TryEnqueue(() =>
                {
                    _blurToggle.IsOn = false;
                    _blurIntensitySlider.Value = 7;
                    _blurValueBox.Text = "7";
                });
            }

            // Restore detected subjects from settings if available
            if (_selectedImage.Settings.TryGetValue("DetectedSubjects", out var detectedSubjectsObj)
                && detectedSubjectsObj is List<DetectedSubject> savedSubjects
                && savedSubjects.Count > 0)
            {
                _detectedSubjects = savedSubjects;

                // Notify PhotoViewer to show the subjects
                SubjectsDetectedEvent?.Invoke(_detectedSubjects);

                // Update UI to show detection is already done
                DispatcherQueue.TryEnqueue(() =>
                {
                    _startButton.Visibility = Visibility.Collapsed;
                    _detectionStatusText.Text = $"✓ {_detectedSubjects.Count} subject(s) found. Click them to select.";
                    _detectionStatusText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 200, 50));
                    _detectionStatusText.Visibility = Visibility.Visible;
                    _showOverlayToggle.Visibility = Visibility.Visible;
                    _showOverlayToggle.IsOn = true; // Always show overlays when switching to image with detections
                    _resetDetectionButton.Visibility = Visibility.Visible;
                    _blurToggle.Visibility = Visibility.Visible;
                    _blurIntensityLabel.Visibility = Visibility.Visible;
                    _blurSliderGrid.Visibility = Visibility.Visible;
                });
            }
            else
            {
                // Reset UI for new image without detection
                _detectedSubjects.Clear();
                SubjectsDetectedEvent?.Invoke(_detectedSubjects);

                DispatcherQueue.TryEnqueue(() =>
                {
                    _startButton.Visibility = Visibility.Visible;
                    _detectionStatusText.Text = "Click 'Detect Subjects' to find subjects.";
                    _detectionStatusText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150));
                    _detectionStatusText.Visibility = Visibility.Visible;
                    _showOverlayToggle.Visibility = Visibility.Collapsed;
                    _showOverlayToggle.IsOn = true; // Reset to default
                    _resetDetectionButton.Visibility = Visibility.Collapsed;
                    _blurToggle.Visibility = Visibility.Collapsed;
                    _blurIntensityLabel.Visibility = Visibility.Collapsed;
                    _blurSliderGrid.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void OnBlurToggled(object sender, RoutedEventArgs e)
        {
            if (_selectedImage?.Settings == null) return;

            if (_selectedImage.Settings.TryGetValue("Blur", out var blurObj) && blurObj is Dictionary<string, object> blurSettings)
            {
                blurSettings["State"] = _blurToggle.IsOn;
                FilterUpdateRequested?.Invoke();
            }
        }

        private void OnBlurIntensityChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_selectedImage?.Settings == null) return;

            float sigma = (float)e.NewValue;
            _blurValueBox.Text = ((int)sigma).ToString();

            if (_selectedImage.Settings.TryGetValue("Blur", out var blurObj) && blurObj is Dictionary<string, object> blurSettings)
            {
                blurSettings["Sigma"] = sigma;
                FilterUpdateRequested?.Invoke();
            }
        }

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
