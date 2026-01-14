using LuxBenchmark.Models;
using LuxBenchmark.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LuxBenchmark.Components
{
    public sealed partial class SessionSelector : Page
    {
        private readonly BenchmarkDataService _dataService;
        private IEventBus? _eventBus;
        private BenchmarkSession? _selectedSessionA;
        private BenchmarkSession? _selectedSessionB;

        public event Action<BenchmarkSession?, BenchmarkSession?>? SelectionChanged;

        public BenchmarkSession? SelectedSessionA => _selectedSessionA;
        public BenchmarkSession? SelectedSessionB => _selectedSessionB;

        public SessionSelector()
        {
            InitializeComponent();
            _dataService = BenchmarkDataService.Instance;
            _dataService.SessionsChanged += RefreshSessionList;

            RefreshSessionList();
        }

        /// <summary>
        /// Sets the event bus for window handle requests.
        /// </summary>
        public void SetEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private async void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading sessions...";
            var count = await _dataService.LoadAllSessionsAsync();
            StatusText.Text = $"Loaded {count} session(s)";
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_eventBus == null)
            {
                StatusText.Text = "Error: EventBus not initialized";
                return;
            }

            // Request window handle via EventBus
            var tcs = new TaskCompletionSource<nint>();
            await _eventBus.Publish(new RequestWindowHandleEvent(handle => tcs.SetResult(handle)));
            nint windowHandle = await tcs.Task;

            if (windowHandle == 0)
            {
                StatusText.Text = "Error: Could not get window handle";
                return;
            }

            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, windowHandle);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                StatusText.Text = $"Loading from {folder.Path}...";
                var count = await _dataService.LoadAllSessionsAsync(folder.Path);
                StatusText.Text = $"Loaded {count} session(s) from {folder.Name}";
            }
        }

        private void RefreshSessionList()
        {
            // Update combo boxes
            var sessions = _dataService.LoadedSessions.ToList();

            SessionACombo.Items.Clear();
            SessionBCombo.Items.Clear();

            foreach (var session in sessions)
            {
                SessionACombo.Items.Add(new ComboBoxItem
                {
                    Content = FormatSessionName(session),
                    Tag = session
                });
                SessionBCombo.Items.Add(new ComboBoxItem
                {
                    Content = FormatSessionName(session),
                    Tag = session
                });
            }

            // Update session list panel
            SessionListPanel.Children.Clear();

            foreach (var session in sessions)
            {
                var sessionCard = CreateSessionCard(session);
                SessionListPanel.Children.Add(sessionCard);
            }

            if (sessions.Count == 0)
            {
                var emptyText = new TextBlock
                {
                    Text = "No sessions loaded.\nUse the buttons above to load benchmark data.",
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 12
                };
                SessionListPanel.Children.Add(emptyText);
            }
        }

        private Border CreateSessionCard(BenchmarkSession session)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 45, 45)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 4)
            };

            var content = new StackPanel();

            // Session name
            content.Children.Add(new TextBlock
            {
                Text = session.DisplayName,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            // Version
            content.Children.Add(new TextBlock
            {
                Text = $"Version: {session.Version}",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 150, 150, 150)),
                FontSize = 10
            });

            // Date and samples
            content.Children.Add(new TextBlock
            {
                Text = $"{session.StartTime:yyyy-MM-dd HH:mm} | {session.TotalSamples} samples",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                FontSize = 10
            });

            // Quick stats
            if (session.Statistics.TryGetValue("Pipeline:Total", out var pipelineStats))
            {
                var avgText = $"Avg: {pipelineStats.AvgMs:F1}ms";
                var rating = BenchmarkDataService.GetRating(pipelineStats.AvgMs);
                var ratingColor = rating switch
                {
                    PerformanceRating.Excellent => Windows.UI.Color.FromArgb(255, 76, 175, 80),
                    PerformanceRating.Good => Windows.UI.Color.FromArgb(255, 139, 195, 74),
                    PerformanceRating.Acceptable => Windows.UI.Color.FromArgb(255, 255, 193, 7),
                    _ => Windows.UI.Color.FromArgb(255, 244, 67, 54)
                };

                content.Children.Add(new TextBlock
                {
                    Text = avgText,
                    Foreground = new SolidColorBrush(ratingColor),
                    FontSize = 11,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            // Action buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var setAButton = new Button
            {
                Content = "Set as A",
                FontSize = 10,
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212))
            };
            setAButton.Click += (s, e) =>
            {
                _selectedSessionA = session;
                SelectSession(SessionACombo, session);
                NotifySelectionChanged();
            };
            buttonPanel.Children.Add(setAButton);

            var setBButton = new Button
            {
                Content = "Set as B",
                FontSize = 10,
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 140, 0))
            };
            setBButton.Click += (s, e) =>
            {
                _selectedSessionB = session;
                SelectSession(SessionBCombo, session);
                NotifySelectionChanged();
            };
            buttonPanel.Children.Add(setBButton);

            content.Children.Add(buttonPanel);
            card.Child = content;

            return card;
        }

        private void SelectSession(ComboBox combo, BenchmarkSession session)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Tag == session)
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
        }

        private void SessionACombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionACombo.SelectedItem is ComboBoxItem item && item.Tag is BenchmarkSession session)
            {
                _selectedSessionA = session;
                NotifySelectionChanged();
            }
        }

        private void SessionBCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionBCombo.SelectedItem is ComboBoxItem item && item.Tag is BenchmarkSession session)
            {
                _selectedSessionB = session;
                NotifySelectionChanged();
            }
        }

        private void NotifySelectionChanged()
        {
            SelectionChanged?.Invoke(_selectedSessionA, _selectedSessionB);
        }

        private static string FormatSessionName(BenchmarkSession session)
        {
            var name = session.DisplayName;
            if (name.Length > 30)
                name = name.Substring(0, 27) + "...";

            return $"{name} ({session.StartTime:MM/dd HH:mm})";
        }
    }
}
