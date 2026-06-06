using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class HeatmapCell
    {
        public string Date { get; set; } = "";
        public int Count { get; set; }
        public int IntensityLevel { get; set; }
    }

    public class StatsViewModel : ViewModelBase
    {
        private readonly StatsService _statsService;
        private readonly AchievementService _achievementService;

        private string _todayPomodoroCount = "0";
        private string _todayFocusTime = "0\u5206\u949f";
        private string _weeklyTotal = "0";
        private string _weeklyFocusTime = "0\u5206\u949f";
        private string _monthlyTotal = "0";
        private string _monthlyFocusTime = "0\u5206\u949f";
        private string _allTimeTotal = "0";
        private int _currentStreak;
        private int _longestStreak;

        public string TodayPomodoroCount { get => _todayPomodoroCount; set => SetProperty(ref _todayPomodoroCount, value); }
        public string TodayFocusTime { get => _todayFocusTime; set => SetProperty(ref _todayFocusTime, value); }
        public string WeeklyTotal { get => _weeklyTotal; set => SetProperty(ref _weeklyTotal, value); }
        public string WeeklyFocusTime { get => _weeklyFocusTime; set => SetProperty(ref _weeklyFocusTime, value); }
        public string MonthlyTotal { get => _monthlyTotal; set => SetProperty(ref _monthlyTotal, value); }
        public string MonthlyFocusTime { get => _monthlyFocusTime; set => SetProperty(ref _monthlyFocusTime, value); }
        public string AllTimeTotal { get => _allTimeTotal; set => SetProperty(ref _allTimeTotal, value); }
        public int CurrentStreak { get => _currentStreak; set => SetProperty(ref _currentStreak, value); }
        public int LongestStreak { get => _longestStreak; set => SetProperty(ref _longestStreak, value); }

        public ObservableCollection<HeatmapCell> HeatmapCells { get; } = new();
        public ObservableCollection<Achievement> Achievements { get; } = new();

        public RelayCommand RefreshCommand { get; }

        public StatsViewModel(StatsService statsService, AchievementService achievementService)
        {
            _statsService = statsService;
            _achievementService = achievementService;

            RefreshCommand = new RelayCommand(Refresh);
            Refresh();
        }

        public void Refresh()
        {
            var today = _statsService.Today;
            TodayPomodoroCount = today.CompletedPomodoros.ToString();
            TodayFocusTime = today.GetFormattedFocusTime();

            var weekly = _statsService.GetWeeklyStats();
            WeeklyTotal = weekly.Sum(s => s.CompletedPomodoros).ToString();
            WeeklyFocusTime = FormatSeconds(weekly.Sum(s => s.TotalFocusSeconds));

            var monthly = _statsService.GetMonthlyStats();
            MonthlyTotal = monthly.Sum(s => s.CompletedPomodoros).ToString();
            MonthlyFocusTime = FormatSeconds(monthly.Sum(s => s.TotalFocusSeconds));

            AllTimeTotal = _statsService.GetTotalPomodoros().ToString();
            CurrentStreak = _statsService.GetCurrentStreak();
            LongestStreak = _statsService.GetLongestStreak();

            BuildHeatmap();
            LoadAchievements();
        }

        private void BuildHeatmap()
        {
            HeatmapCells.Clear();
            var data = _statsService.GetHeatmapData();

            // Calculate thresholds for intensity levels
            int maxCount = data.Count > 0 ? data.Values.Max() : 1;
            if (maxCount < 1) maxCount = 1;

            // Generate cells for last 16 weeks
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-16 * 7);
            // Align to Monday
            while (startDate.DayOfWeek != DayOfWeek.Monday)
                startDate = startDate.AddDays(-1);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                int count = data.TryGetValue(dateStr, out int c) ? c : 0;
                int level = 0;
                if (count > 0)
                {
                    double ratio = (double)count / maxCount;
                    if (ratio <= 0.25) level = 1;
                    else if (ratio <= 0.5) level = 2;
                    else if (ratio <= 0.75) level = 3;
                    else level = 4;
                }

                HeatmapCells.Add(new HeatmapCell
                {
                    Date = dateStr,
                    Count = count,
                    IntensityLevel = level,
                });
            }
        }

        private void LoadAchievements()
        {
            Achievements.Clear();
            foreach (var a in _achievementService.Achievements)
                Achievements.Add(a);
        }

        private static string FormatSeconds(int totalSeconds)
        {
            int h = totalSeconds / 3600;
            int m = (totalSeconds % 3600) / 60;
            if (h > 0) return $"{h}\u5c0f\u65f6{m}\u5206\u949f";
            return $"{m}\u5206\u949f";
        }
    }
}
