using System;
using System.Windows;
using System.Windows.Interop;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF
{
    public partial class App : Application
    {
        public static AppSettings CurrentSettings { get; private set; } = new();

        private TrayManager? _tray;
        private AmbientSoundService? _ambientSound;
        private HotkeyService? _hotkey;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Persistence
            var persistence = new PersistenceService();

            // 2. Settings
            CurrentSettings = persistence.LoadSettings();
            ThemeManager.Apply(CurrentSettings.Theme);

            // 3. Timer services
            var clockTimer = new TimerService(TimeSpan.FromSeconds(1));
            var countdownTimer = new TimerService(TimeSpan.FromSeconds(1));
            var stopwatchTimer = new TimerService(TimeSpan.FromSeconds(1));

            // 4. Sound
            SoundManager? sound = null;
            try { sound = new SoundManager(); } catch { }

            // 5. Ambient sound
            try { _ambientSound = new AmbientSoundService(); } catch { }

            // 6. Hotkey
            _hotkey = new HotkeyService();

            // 7. Data services
            var taskService = new TaskService(persistence);
            var statsService = new StatsService(persistence);
            var achievementService = new AchievementService(persistence, statsService);

            // 8. Register services in locator
            ServiceLocator.Register(persistence);
            ServiceLocator.Register(taskService);
            ServiceLocator.Register(statsService);
            ServiceLocator.Register(achievementService);
            if (sound != null) ServiceLocator.Register(sound);
            if (_ambientSound != null) ServiceLocator.Register(_ambientSound);

            // 9. Create ViewModels
            var homeVM = new HomeViewModel(clockTimer, statsService, taskService, CurrentSettings);
            var countdownVM = new CountdownViewModel(countdownTimer, sound, CurrentSettings);
            var stopwatchVM = new StopwatchViewModel(stopwatchTimer);
            var taskListVM = new TaskListViewModel(taskService);
            var statsVM = new StatsViewModel(statsService, achievementService);

            // 10. Create MainWindow (need reference for TrayManager)
            var mainWindow = new MainWindow();

            // 11. TrayManager (needs Window reference)
            try { _tray = new TrayManager(mainWindow); } catch { }

            // 12. Create MainViewModel
            var mainVM = new MainViewModel(
                homeVM, countdownVM, stopwatchVM, taskListVM, statsVM,
                statsService, taskService, achievementService,
                persistence, _tray, _ambientSound, CurrentSettings);

            // 13. Initialize MainWindow
            mainWindow.Initialize(mainVM, _hotkey);

            // 14. Wire global hotkeys to countdown commands
            _hotkey.HotkeyPressed += (id) =>
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    if (mainVM.CurrentPage != PageType.Countdown) return;
                    switch (id)
                    {
                        case HotkeyService.HK_START_PAUSE:
                            if (countdownVM.IsRunning)
                                countdownVM.PauseCommand.Execute(null);
                            else
                                countdownVM.StartCommand.Execute(null);
                            break;
                        case HotkeyService.HK_RESET:
                            countdownVM.ResetCommand.Execute(null);
                            break;
                    }
                });
            };

            // 15. Show window
            mainWindow.Show();

            // 16. Initial achievement check
            achievementService.CheckAchievements();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkey?.Dispose();
            _ambientSound?.Dispose();
            _tray?.Dispose();
            base.OnExit(e);
        }
    }
}
