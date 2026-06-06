using System;
using System.Collections.ObjectModel;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private readonly PersistenceService _persistence;

        private string _selectedTheme;
        private bool _tickSoundEnabled;
        private bool _autoBreak;
        private int _breakDurationMinutes;
        private int _longBreakDurationMinutes;
        private int _pomodorosBeforeLongBreak;
        private int _workDurationMinutes;
        private int _dailyGoalPomodoros;

        public string SelectedTheme { get => _selectedTheme; set => SetProperty(ref _selectedTheme, value); }
        public bool TickSoundEnabled { get => _tickSoundEnabled; set => SetProperty(ref _tickSoundEnabled, value); }
        public bool AutoBreak { get => _autoBreak; set => SetProperty(ref _autoBreak, value); }
        public int BreakDurationMinutes { get => _breakDurationMinutes; set => SetProperty(ref _breakDurationMinutes, value); }
        public int LongBreakDurationMinutes { get => _longBreakDurationMinutes; set => SetProperty(ref _longBreakDurationMinutes, value); }
        public int PomodorosBeforeLongBreak { get => _pomodorosBeforeLongBreak; set => SetProperty(ref _pomodorosBeforeLongBreak, value); }
        public int WorkDurationMinutes { get => _workDurationMinutes; set => SetProperty(ref _workDurationMinutes, value); }
        public int DailyGoalPomodoros { get => _dailyGoalPomodoros; set => SetProperty(ref _dailyGoalPomodoros, value); }

        public ObservableCollection<ThemeOption> AvailableThemes { get; } = new();

        public RelayCommand SaveCommand { get; }
        public RelayCommand<string> ApplyThemeCommand { get; }
        public RelayCommand CloseCommand { get; }

        private bool? _dialogResult;
        public bool? DialogResult { get => _dialogResult; set => SetProperty(ref _dialogResult, value); }

        public event Action? ThemeChanged;

        public SettingsViewModel(AppSettings settings, PersistenceService persistence)
        {
            _settings = settings;
            _persistence = persistence;

            // Load current values
            _selectedTheme = settings.Theme;
            _tickSoundEnabled = settings.TickSoundEnabled;
            _autoBreak = settings.AutoBreak;
            _breakDurationMinutes = settings.BreakDurationMinutes;
            _longBreakDurationMinutes = settings.LongBreakDurationMinutes;
            _pomodorosBeforeLongBreak = settings.PomodorosBeforeLongBreak;
            _workDurationMinutes = settings.WorkDurationMinutes;
            _dailyGoalPomodoros = settings.DailyGoalPomodoros;

            // Build theme list
            foreach (var kvp in ThemeManager.Themes)
            {
                AvailableThemes.Add(new ThemeOption
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                    AccentColor = kvp.Value.Accent,
                });
            }

            SaveCommand = new RelayCommand(Save);
            ApplyThemeCommand = new RelayCommand<string>(ApplyTheme);
            CloseCommand = new RelayCommand(() => DialogResult = true);
        }

        private void Save()
        {
            _settings.Theme = _selectedTheme;
            _settings.TickSoundEnabled = _tickSoundEnabled;
            _settings.AutoBreak = _autoBreak;
            _settings.BreakDurationMinutes = Math.Max(1, _breakDurationMinutes);
            _settings.LongBreakDurationMinutes = Math.Max(1, _longBreakDurationMinutes);
            _settings.PomodorosBeforeLongBreak = Math.Max(1, _pomodorosBeforeLongBreak);
            _settings.WorkDurationMinutes = Math.Max(1, _workDurationMinutes);
            _settings.DailyGoalPomodoros = Math.Max(0, _dailyGoalPomodoros);
            _persistence.SaveSettings(_settings);
            DialogResult = true;
        }

        private void ApplyTheme(string? themeId)
        {
            if (themeId == null) return;
            SelectedTheme = themeId;
            ThemeManager.Apply(themeId);
            ThemeChanged?.Invoke();
        }
    }

    public class ThemeOption
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string AccentColor { get; set; } = "";
    }
}
