using System;
using System.Windows;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;
using PomodoroWPF.Views;

namespace PomodoroWPF.ViewModels
{
    public enum PageType { Home, Countdown, Stopwatch }

    public class MainViewModel : ViewModelBase
    {
        private readonly StatsService _statsService;
        private readonly TaskService _taskService;
        private readonly AchievementService _achievementService;
        private readonly PersistenceService _persistence;
        private readonly TrayManager? _tray;
        private readonly AmbientSoundService? _ambientSound;
        private readonly AppSettings _settings;

        private PageType _currentPage = PageType.Home;

        public PageType CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    RaisePropertyChanged(nameof(IsHomeVisible));
                    RaisePropertyChanged(nameof(IsCountdownVisible));
                    RaisePropertyChanged(nameof(IsStopwatchVisible));
                }
            }
        }

        public bool IsHomeVisible => _currentPage == PageType.Home;
        public bool IsCountdownVisible => _currentPage == PageType.Countdown;
        public bool IsStopwatchVisible => _currentPage == PageType.Stopwatch;

        public string CurrentTaskDisplay => _taskService.CurrentTaskDisplay;

        public HomeViewModel Home { get; }
        public CountdownViewModel Countdown { get; }
        public StopwatchViewModel Stopwatch { get; }
        public TaskListViewModel TaskList { get; }
        public StatsViewModel Stats { get; }

        public RelayCommand NavigateToHomeCommand { get; }
        public RelayCommand NavigateToCountdownCommand { get; }
        public RelayCommand NavigateToStopwatchCommand { get; }
        public RelayCommand OpenTaskListCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand OpenStatsCommand { get; }
        public RelayCommand QuitCommand { get; }
        public RelayCommand ToggleFullscreenCommand { get; set; }
        public RelayCommand ToggleAmbientSoundCommand { get; }

        public event Action? QuitRequested;
        public event Action? ToggleFullscreenRequested;

        public Window? MainWindow { get; set; }

        public MainViewModel(
            HomeViewModel home,
            CountdownViewModel countdown,
            StopwatchViewModel stopwatch,
            TaskListViewModel taskList,
            StatsViewModel stats,
            StatsService statsService,
            TaskService taskService,
            AchievementService achievementService,
            PersistenceService persistence,
            TrayManager? tray,
            AmbientSoundService? ambientSound,
            AppSettings settings)
        {
            Home = home;
            Countdown = countdown;
            Stopwatch = stopwatch;
            TaskList = taskList;
            Stats = stats;
            _statsService = statsService;
            _taskService = taskService;
            _achievementService = achievementService;
            _persistence = persistence;
            _tray = tray;
            _ambientSound = ambientSound;
            _settings = settings;

            NavigateToHomeCommand = new RelayCommand(() => { CurrentPage = PageType.Home; Home.UpdateAll(); });
            NavigateToCountdownCommand = new RelayCommand(() => CurrentPage = PageType.Countdown);
            NavigateToStopwatchCommand = new RelayCommand(() => CurrentPage = PageType.Stopwatch);
            OpenTaskListCommand = new RelayCommand(OpenTaskList);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenStatsCommand = new RelayCommand(OpenStats);
            QuitCommand = new RelayCommand(() => QuitRequested?.Invoke());
            ToggleAmbientSoundCommand = new RelayCommand(ToggleAmbientSound);
            ToggleFullscreenCommand = new RelayCommand(() => ToggleFullscreenRequested?.Invoke());

            // Wire pomodoro completion
            Countdown.PomodoroCompleted += OnPomodoroCompleted;
            Countdown.BreakCompleted += OnBreakCompleted;

            // Wire task changes
            TaskList.TasksChanged += OnTasksChanged;
            _taskService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TaskService.CurrentTaskDisplay))
                    RaisePropertyChanged(nameof(CurrentTaskDisplay));
            };
        }

        private void OnPomodoroCompleted(int focusSeconds)
        {
            _statsService.RecordPomodoro(focusSeconds);
            _taskService.IncrementPomodoroOnCurrentTask();

            Home.UpdateStats();
            Home.UpdateDailyGoal();
            Home.UpdateTaskSummary();
            Stats.Refresh();
            RaisePropertyChanged(nameof(CurrentTaskDisplay));

            // Check achievements
            string? achievement = _achievementService.CheckAchievements();
            if (achievement != null)
                _tray?.ShowNotification("\u2605 \u6210\u5c31\u89e3\u9501", $"\u606d\u559c\u83b7\u5f97\u6210\u5c31\uff1a{achievement}");

            // Tray notification
            _tray?.ShowNotification("\u756a\u8304\u949f",
                $"\u25cf \u7b2c {_statsService.Today.CompletedPomodoros} \u4e2a\u756a\u8304\u5b8c\u6210\uff01");

            // Stop ambient sound during break
            if (_settings.AmbientSoundEnabled)
                _ambientSound?.Stop();
        }

        private void OnBreakCompleted()
        {
            _tray?.ShowNotification("\u756a\u8304\u949f", "\u4f11\u606f\u7ed3\u675f\uff0c\u51c6\u5907\u5f00\u59cb\u4e0b\u4e00\u4e2a\u756a\u8304\uff01");

            if (MainWindow != null)
            {
                MainWindow.Dispatcher.Invoke(() =>
                {
                    var dlg = new InfoDialogWindow("\u4f11\u606f\u7ed3\u675f", "\u7cbe\u529b\u6062\u590d\uff01\u51c6\u5907\u5f00\u59cb\u4e0b\u4e00\u4e2a\u756a\u8304\u3002");
                    dlg.Owner = MainWindow;
                    dlg.ShowDialog();
                });
            }

            // Restart ambient sound when starting next work session
            if (_settings.AmbientSoundEnabled && Countdown.StartCommand.CanExecute(null))
                _ambientSound?.Start(_settings.AmbientSoundType);
        }

        private void OnTasksChanged()
        {
            RaisePropertyChanged(nameof(CurrentTaskDisplay));
            Home.UpdateTaskSummary();
        }

        private void OpenTaskList()
        {
            if (MainWindow == null) return;
            var dlg = new TaskListWindow(TaskList);
            dlg.Owner = MainWindow;
            dlg.ShowDialog();
        }

        private void OpenSettings()
        {
            if (MainWindow == null) return;
            var vm = new SettingsViewModel(_settings, _persistence);
            vm.ThemeChanged += () =>
            {
                // Refresh displays after theme change
                Home.UpdateAll();
            };
            var dlg = new SettingsWindow(vm);
            dlg.Owner = MainWindow;
            dlg.ShowDialog();

            // Apply any changes
            Home.UpdateDailyGoal();
            Countdown.RefreshCycleDots();
        }

        private void OpenStats()
        {
            if (MainWindow == null) return;
            Stats.Refresh();
            var dlg = new StatsWindow(Stats);
            dlg.Owner = MainWindow;
            dlg.ShowDialog();
        }

        private void ToggleAmbientSound()
        {
            if (!_settings.AmbientSoundEnabled || _ambientSound == null) return;

            if (_ambientSound.IsPlaying)
                _ambientSound.Stop();
            else if (Countdown.IsRunning && !Countdown.IsBreakMode)
                _ambientSound.Start(_settings.AmbientSoundType);
        }

        public void StartAmbientIfNeeded()
        {
            if (_settings.AmbientSoundEnabled && _ambientSound != null &&
                Countdown.IsRunning && !Countdown.IsBreakMode)
            {
                _ambientSound.Start(_settings.AmbientSoundType);
            }
        }

        public void StopAmbient()
        {
            _ambientSound?.Stop();
        }

        public void SaveAll()
        {
            _persistence.SaveSettings(_settings);
        }
    }
}
