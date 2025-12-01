using Luxoria.Core.Interfaces;
using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;

namespace Luxoria.App.Services
{
    /// <summary>
    /// Configuration for the onboarding system
    /// </summary>
    public class OnboardingConfig
    {
        /// <summary>
        /// Steps to guide the user through the initial setup
        /// </summary>
        public List<OnboardingStep> Steps { get; set; } = new();

        /// <summary>
        /// Gets the default onboarding configuration
        /// </summary>
        public static OnboardingConfig GetDefault()
        {
            return new OnboardingConfig
            {
                Steps = new List<OnboardingStep>
                {
                    new OnboardingStep
                    {
                        Id = "welcome",
                        Title = "Welcome to Luxoria!",
                        Description = "Let's get you started with installing some modules to enhance your experience.",
                        TargetElementName = null, // No specific target, just a welcome message
                        Position = TooltipPosition.Center,
                        HighlightTarget = false
                    },
                    new OnboardingStep
                    {
                        Id = "luxoria-button",
                        Title = "Install Modules",
                        Description = "Click here to open the menu and access the Marketplace to install modules.",
                        TargetElementName = "LuxoriaMenuButton",
                        Position = TooltipPosition.BottomRight,
                        HighlightTarget = true
                    }
                }
            };
        }
    }

    /// <summary>
    /// Represents a single step in the onboarding process
    /// </summary>
    public class OnboardingStep
    {
        /// <summary>
        /// Unique identifier for this step
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title of the tooltip
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of what to do
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Name of the UI element to highlight (must have x:Name in XAML)
        /// </summary>
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Position of the tooltip relative to the target
        /// </summary>
        public TooltipPosition Position { get; set; }

        /// <summary>
        /// Whether to highlight the target element
        /// </summary>
        public bool HighlightTarget { get; set; }

        /// <summary>
        /// Whether this step requires the Luxoria menu to be open
        /// </summary>
        public bool RequiresMenuOpen { get; set; }
    }

    /// <summary>
    /// Position of the tooltip relative to target
    /// </summary>
    public enum TooltipPosition
    {
        Center,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Service to manage onboarding state and progression
    /// </summary>
    public class OnboardingService
    {
        private const string VaultName = "Luxoria.App.Onboarding";
        private const string StateKey = "OnboardingState";

        private readonly IStorageAPI _storage;
        private OnboardingState _state;
        private OnboardingConfig _config;

        public event EventHandler? OnboardingStateChanged;

        public OnboardingService(IVaultService vaultService)
        {
            _storage = vaultService.GetVault(VaultName);
            _config = OnboardingConfig.GetDefault();
            _state = LoadState();
        }

        /// <summary>
        /// Gets the current onboarding configuration
        /// </summary>
        public OnboardingConfig Config => _config;

        /// <summary>
        /// Gets the current onboarding state
        /// </summary>
        public OnboardingState State => _state;

        /// <summary>
        /// Checks if onboarding should be shown
        /// </summary>
        public bool ShouldShowOnboarding()
        {
            return !_state.IsCompleted && !_state.IsSkipped;
        }

        /// <summary>
        /// Checks if this is the first launch (no modules installed)
        /// </summary>
        public bool IsFirstLaunch(int moduleCount)
        {
            return moduleCount == 0 && !_state.HasSeenWelcome;
        }

        /// <summary>
        /// Marks the welcome screen as seen
        /// </summary>
        public void MarkWelcomeSeen()
        {
            _state.HasSeenWelcome = true;
            SaveState();
            OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Completes the onboarding process
        /// </summary>
        public void CompleteOnboarding()
        {
            _state.IsCompleted = true;
            _state.CurrentStepIndex = _config.Steps.Count;
            SaveState();
            OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Skips the onboarding process
        /// </summary>
        public void SkipOnboarding()
        {
            _state.IsSkipped = true;
            SaveState();
            OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Advances to the next onboarding step
        /// </summary>
        public void NextStep()
        {
            if (_state.CurrentStepIndex < _config.Steps.Count - 1)
            {
                _state.CurrentStepIndex++;
                SaveState();
                OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                CompleteOnboarding();
            }
        }

        /// <summary>
        /// Goes back to the previous onboarding step
        /// </summary>
        public void PreviousStep()
        {
            if (_state.CurrentStepIndex > 0)
            {
                _state.CurrentStepIndex--;
                SaveState();
                OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the current onboarding step
        /// </summary>
        public OnboardingStep? GetCurrentStep()
        {
            if (_state.CurrentStepIndex >= 0 && _state.CurrentStepIndex < _config.Steps.Count)
            {
                return _config.Steps[_state.CurrentStepIndex];
            }
            return null;
        }

        /// <summary>
        /// Resets the onboarding state (for testing)
        /// </summary>
        public void ResetOnboarding()
        {
            _state = new OnboardingState();
            SaveState();
            OnboardingStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private OnboardingState LoadState()
        {
            try
            {
                if (_storage.Contains(StateKey))
                {
                    return _storage.Get<OnboardingState>(StateKey);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load onboarding state: {ex.Message}");
            }

            return new OnboardingState();
        }

        private void SaveState()
        {
            try
            {
                _storage.Save(StateKey, _state);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save onboarding state: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents the current state of the onboarding process
    /// </summary>
    public class OnboardingState
    {
        /// <summary>
        /// Current step index in the onboarding flow
        /// </summary>
        public int CurrentStepIndex { get; set; } = 0;

        /// <summary>
        /// Whether the onboarding has been completed
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Whether the user has skipped onboarding
        /// </summary>
        public bool IsSkipped { get; set; } = false;

        /// <summary>
        /// Whether the user has seen the welcome screen
        /// </summary>
        public bool HasSeenWelcome { get; set; } = false;
    }
}
