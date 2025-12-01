using Luxoria.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Luxoria.App.Components
{
    /// <summary>
    /// Onboarding overlay component that guides users through initial setup
    /// </summary>
    public sealed partial class OnboardingOverlay : UserControl
    {
        private readonly OnboardingService _onboardingService;
        private Window? _parentWindow;

        public event EventHandler? OnboardingCompleted;
        public event EventHandler? OnboardingSkipped;
        public event EventHandler<OnboardingStep>? StepChanged;

        public OnboardingOverlay(OnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
            InitializeComponent();

            Loaded += OnboardingOverlay_Loaded;
        }

        private void OnboardingOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            // Find parent window by traversing up the visual tree
            DependencyObject current = this;
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.Parent != null)
                {
                    current = fe.Parent;
                }
                else if (current is FrameworkElement fe2)
                {
                    // Try to get visual parent
                    current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
                }
                else
                {
                    break;
                }
            }

            // If we couldn't find the window, use XamlRoot.Content as the root
            UpdateStep();
        }

        /// <summary>
        /// Updates the UI to show the current onboarding step
        /// </summary>
        public void UpdateStep()
        {
            // Don't update if onboarding is completed or skipped
            if (!_onboardingService.ShouldShowOnboarding())
            {
                return;
            }

            var step = _onboardingService.GetCurrentStep();
            if (step == null)
            {
                // No more steps, notify completion
                OnboardingCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Update progress text
            var currentIndex = _onboardingService.State.CurrentStepIndex + 1;
            var totalSteps = _onboardingService.Config.Steps.Count;
            ProgressText.Text = $"Step {currentIndex} of {totalSteps}";

            // Update content
            TitleText.Text = step.Title;
            DescriptionText.Text = step.Description;

            // Update button visibility
            BackButton.Visibility = currentIndex > 1 ? Visibility.Visible : Visibility.Collapsed;
            NextButton.Visibility = currentIndex < totalSteps ? Visibility.Visible : Visibility.Collapsed;
            FinishButton.Visibility = currentIndex == totalSteps ? Visibility.Visible : Visibility.Collapsed;

            // Position highlight and tooltip
            PositionElements(step);

            // Notify listeners
            StepChanged?.Invoke(this, step);
        }

        /// <summary>
        /// Positions the highlight border and tooltip based on the current step
        /// </summary>
        private void PositionElements(OnboardingStep step)
        {
            if (string.IsNullOrEmpty(step.TargetElementName))
            {
                // Center mode - no highlight, center tooltip
                HighlightPopup.IsOpen = false;
                TooltipBorder.Visibility = Visibility.Visible;
                CenterTooltip();
                return;
            }

            FrameworkElement? targetElement = null;

            // Try to find the target element using XamlRoot
            if (this.XamlRoot?.Content is UIElement rootElement)
            {
                targetElement = FindElementByName(rootElement, step.TargetElementName);
            }

            // If not found in root, search in open popups
            if (targetElement == null && this.XamlRoot != null)
            {
                var popups = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(this.XamlRoot);
                foreach (var popup in popups)
                {
                    if (popup.Child is UIElement popupChild)
                    {
                        targetElement = FindElementByName(popupChild, step.TargetElementName);
                        if (targetElement != null)
                            break;
                    }
                }
            }

            if (targetElement != null && step.HighlightTarget)
            {
                PositionHighlight(targetElement);
                PositionTooltip(targetElement, step.Position);
                HighlightPopup.IsOpen = true;
                TooltipBorder.Visibility = Visibility.Visible;
                return;
            }

            // Fallback to center mode if element not found
            HighlightPopup.IsOpen = false;
            TooltipBorder.Visibility = Visibility.Visible;
            CenterTooltip();
        }

        /// <summary>
        /// Positions the highlight border around the target element
        /// </summary>
        private void PositionHighlight(FrameworkElement targetElement)
        {
            try
            {
                // Get position relative to window
                var transform = targetElement.TransformToVisual(null);
                var position = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

                // Configure popup
                HighlightPopup.XamlRoot = this.XamlRoot;
                HighlightPopup.HorizontalOffset = position.X - 8;
                HighlightPopup.VerticalOffset = position.Y - 8;

                // Set size
                HighlightBorder.Width = targetElement.ActualWidth + 16;
                HighlightBorder.Height = targetElement.ActualHeight + 16;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to position highlight: {ex.Message}");
            }
        }

        /// <summary>
        /// Positions the tooltip relative to the target element
        /// </summary>
        private void PositionTooltip(FrameworkElement targetElement, TooltipPosition position)
        {
            try
            {
                var transform = targetElement.TransformToVisual(RootGrid);
                var targetPos = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

                // Force measure tooltip to get its size
                TooltipBorder.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
                var tooltipWidth = TooltipBorder.DesiredSize.Width;
                var tooltipHeight = TooltipBorder.DesiredSize.Height;

                double left = 0;
                double top = 0;

                const double spacing = 16;

                switch (position)
                {
                    case TooltipPosition.BottomRight:
                        left = targetPos.X + targetElement.ActualWidth + spacing;
                        top = targetPos.Y + targetElement.ActualHeight + spacing;
                        break;
                    case TooltipPosition.Right:
                        left = targetPos.X + targetElement.ActualWidth + spacing;
                        top = targetPos.Y + (targetElement.ActualHeight / 2) - (tooltipHeight / 2);
                        break;
                    case TooltipPosition.Bottom:
                        left = targetPos.X + (targetElement.ActualWidth / 2) - (tooltipWidth / 2);
                        top = targetPos.Y + targetElement.ActualHeight + spacing;
                        break;
                    case TooltipPosition.Left:
                        left = targetPos.X - tooltipWidth - spacing;
                        top = targetPos.Y + (targetElement.ActualHeight / 2) - (tooltipHeight / 2);
                        break;
                    case TooltipPosition.Top:
                        left = targetPos.X + (targetElement.ActualWidth / 2) - (tooltipWidth / 2);
                        top = targetPos.Y - tooltipHeight - spacing;
                        break;
                    case TooltipPosition.TopLeft:
                        left = targetPos.X - tooltipWidth - spacing;
                        top = targetPos.Y - spacing;
                        break;
                    case TooltipPosition.TopRight:
                        left = targetPos.X + targetElement.ActualWidth + spacing;
                        top = targetPos.Y - spacing;
                        break;
                    case TooltipPosition.BottomLeft:
                        left = targetPos.X - tooltipWidth - spacing;
                        top = targetPos.Y + targetElement.ActualHeight + spacing;
                        break;
                    default:
                        CenterTooltip();
                        return;
                }

                // Clamp to viewport
                left = Math.Max(16, Math.Min(left, RootGrid.ActualWidth - tooltipWidth - 16));
                top = Math.Max(16, Math.Min(top, RootGrid.ActualHeight - tooltipHeight - 16));

                Canvas.SetLeft(TooltipBorder, left);
                Canvas.SetTop(TooltipBorder, top);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to position tooltip: {ex.Message}");
                CenterTooltip();
            }
        }

        /// <summary>
        /// Centers the tooltip in the viewport
        /// </summary>
        private void CenterTooltip()
        {
            TooltipBorder.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            var tooltipWidth = TooltipBorder.DesiredSize.Width;
            var tooltipHeight = TooltipBorder.DesiredSize.Height;

            var left = (RootGrid.ActualWidth - tooltipWidth) / 2;
            var top = (RootGrid.ActualHeight - tooltipHeight) / 2;

            Canvas.SetLeft(TooltipBorder, left);
            Canvas.SetTop(TooltipBorder, top);
        }

        /// <summary>
        /// Recursively finds an element by name in the visual tree
        /// </summary>
        private FrameworkElement? FindElementByName(UIElement parent, string name)
        {
            if (parent is FrameworkElement fe && fe.Name == name)
            {
                return fe;
            }

            var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is UIElement childElement)
                {
                    var result = FindElementByName(childElement, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _onboardingService.PreviousStep();
            UpdateStep();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _onboardingService.NextStep();
            UpdateStep();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            _onboardingService.CompleteOnboarding();
            HighlightPopup.IsOpen = false;
            OnboardingCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            _onboardingService.SkipOnboarding();
            HighlightPopup.IsOpen = false;
            OnboardingSkipped?.Invoke(this, EventArgs.Empty);
        }
    }
}
