using LuxBenchmark.Components;
using LuxBenchmark.Models;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;

namespace LuxBenchmark
{
    public class LuxBenchmark : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        // UI Panels
        private DashboardMain? _dashboardMain;
        private SessionSelector? _sessionSelector;
        private MetricDetails? _metricDetails;
        private SummaryBar? _summaryBar;

        public string Name => "Lux Benchmark";
        public string Description => "Benchmark module for luxoria.";
        public string Version => "1.0.1";

        public List<ILuxMenuBarItem> Items { get; set; } = [];

        /// <summary>
        /// Initializes the module and sets up the UI panels and event handlers.
        /// </summary>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize LuxBenchmark: EventBus or Context is null", "LuxBenchmark", LogLevel.Error);
                return;
            }

            // Create UI panels
            _dashboardMain = new DashboardMain();
            _sessionSelector = new SessionSelector();
            _metricDetails = new MetricDetails();
            _summaryBar = new SummaryBar();

            // Wire up events between panels
            WireUpEvents();

            List<ISmartButton> smartButtons = new();
            Dictionary<SmartButtonType, object> mainPage = new();

            // Add panels to the main page layout
            mainPage.Add(SmartButtonType.MainPanel, _dashboardMain);
            mainPage.Add(SmartButtonType.BottomPanel, _summaryBar);
            mainPage.Add(SmartButtonType.RightPanel, _metricDetails);
            mainPage.Add(SmartButtonType.LeftPanel, _sessionSelector);

            smartButtons.Add(new SmartButton("Benchmark", "Performance benchmark dashboard", mainPage));
            Items.Add(new LuxMenuBarItem("Benchmark", false, Guid.NewGuid(), smartButtons));

            _logger?.Log($"{Name} initialized", "LuxBenchmark", LogLevel.Info);
        }

        /// <summary>
        /// Wires up events between the UI panels for data synchronization.
        /// </summary>
        private void WireUpEvents()
        {
            if (_sessionSelector == null || _dashboardMain == null ||
                _metricDetails == null || _summaryBar == null || _eventBus == null)
                return;

            // Pass EventBus to SessionSelector for window handle requests
            _sessionSelector.SetEventBus(_eventBus);

            // When session selection changes, update all panels
            _sessionSelector.SelectionChanged += OnSessionSelectionChanged;

            // When an operation is selected in the dashboard, show details
            _dashboardMain.OperationSelected += OnOperationSelected;
        }

        /// <summary>
        /// Handles session selection changes from the SessionSelector panel.
        /// </summary>
        private void OnSessionSelectionChanged(BenchmarkSession? sessionA, BenchmarkSession? sessionB)
        {
            _logger?.Log($"Session selection changed: A={sessionA?.DisplayName ?? "none"}, B={sessionB?.DisplayName ?? "none"}",
                "LuxBenchmark", LogLevel.Debug);

            // Update dashboard with selected sessions
            _dashboardMain?.SetSessions(sessionA, sessionB);

            // Update metric details panel
            _metricDetails?.SetSessions(sessionA, sessionB);

            // Update summary bar
            _summaryBar?.SetSessions(sessionA, sessionB);
        }

        /// <summary>
        /// Handles operation selection from the dashboard charts.
        /// </summary>
        private void OnOperationSelected(string operationKey)
        {
            _logger?.Log($"Operation selected: {operationKey}", "LuxBenchmark", LogLevel.Debug);

            // Show detailed metrics for the selected operation
            _metricDetails?.ShowOperationDetails(operationKey);
        }

        /// <summary>
        /// Executes the module logic manually.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "LuxBenchmark", LogLevel.Info);
        }

        /// <summary>
        /// Cleans up the module and unsubscribes from events.
        /// </summary>
        public void Shutdown()
        {
            // Unsubscribe from events
            if (_sessionSelector != null)
            {
                _sessionSelector.SelectionChanged -= OnSessionSelectionChanged;
            }

            if (_dashboardMain != null)
            {
                _dashboardMain.OperationSelected -= OnOperationSelected;
            }

            _logger?.Log($"{Name} shut down", "LuxBenchmark", LogLevel.Info);
        }
    }
}
