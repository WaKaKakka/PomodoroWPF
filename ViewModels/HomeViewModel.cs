using System;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly TimerService _clockTimer;
        private readonly StatsService _statsService;
        private readonly TaskService _taskService;
        private readonly AppSettings _settings;

        private string _currentTime = "";
        private string _currentDate = "";
        private string _todayStatsText = "";
        private string _taskSummaryText = "";
        private string _dailyGoalText = "";
        private double _dailyGoalProgress;
        private string _streakText = "";

        public string CurrentTime { get => _currentTime; set => SetProperty(ref _currentTime, value); }
        public string CurrentDate { get => _currentDate; set => SetProperty(ref _currentDate, value); }
        public string TodayStatsText { get => _todayStatsText; set => SetProperty(ref _todayStatsText, value); }
        public string TaskSummaryText { get => _taskSummaryText; set => SetProperty(ref _taskSummaryText, value); }
        public string DailyGoalText { get => _dailyGoalText; set => SetProperty(ref _dailyGoalText, value); }
        public double DailyGoalProgress { get => _dailyGoalProgress; set => SetProperty(ref _dailyGoalProgress, value); }
        public string StreakText { get => _streakText; set => SetProperty(ref _streakText, value); }

        public HomeViewModel(TimerService clockTimer, StatsService statsService, TaskService taskService, AppSettings settings)
        {
            _clockTimer = clockTimer;
            _statsService = statsService;
            _taskService = taskService;
            _settings = settings;

            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();

            UpdateAll();
        }

        private void OnClockTick()
        {
            UpdateClock();
        }

        public void UpdateAll()
        {
            UpdateClock();
            UpdateStats();
            UpdateTaskSummary();
            UpdateDailyGoal();
            UpdateStreak();
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");

            var dayNames = new[] { "\u661f\u671f\u4e00", "\u661f\u671f\u4e8c", "\u661f\u671f\u4e09", "\u661f\u671f\u56db", "\u661f\u671f\u4e94", "\u661f\u671f\u516d", "\u661f\u671f\u65e5" };
            CurrentDate = $"{now:yyyy-MM-dd}  {dayNames[(int)now.DayOfWeek]}";
        }

        public void UpdateStats()
        {
            var today = _statsService.Today;
            TodayStatsText = $"\u4eca\u65e5\uff1a{today.CompletedPomodoros} \u4e2a\u756a\u8304  \u00b7  \u4e13\u6ce8 {today.GetFormattedFocusTime()}";
        }

        public void UpdateTaskSummary()
        {
            TaskSummaryText = _taskService.SummaryText;
        }

        public void UpdateDailyGoal()
        {
            int goal = _settings.DailyGoalPomodoros;
            int today = _statsService.Today.CompletedPomodoros;
            if (goal <= 0)
            {
                DailyGoalText = "";
                DailyGoalProgress = 0;
                return;
            }
            DailyGoalProgress = Math.Min(1.0, (double)today / goal);
            if (today >= goal)
                DailyGoalText = $"\U0001F3AF \u76ee\u6807\u8fbe\u6210\uff01{today}/{goal} \U0001F345";
            else
                DailyGoalText = $"\u76ee\u6807\uff1a{today}/{goal} \U0001F345";
        }

        public void UpdateStreak()
        {
            int streak = _statsService.GetCurrentStreak();
            StreakText = streak > 0 ? $"\U0001F525 \u8fde\u7eed {streak} \u5929" : "";
        }
    }
}
