using System;
using System.Collections.Generic;
using System.Linq;
using PomodoroWPF.Models;

namespace PomodoroWPF.Services
{
    public class StatsService
    {
        private readonly PersistenceService _persistence;
        private DailyStats _today;
        private List<DailyStats> _history;

        public DailyStats Today => _today;
        public List<DailyStats> History => _history;

        public StatsService(PersistenceService persistence)
        {
            _persistence = persistence;
            _today = persistence.LoadTodayStats();
            _history = persistence.LoadStatsHistory();
            ArchiveDayIfNeeded();
        }

        private void ArchiveDayIfNeeded()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_today.Date == today)
                return;

            // Archive the old day's data into history
            if (_today.CompletedPomodoros > 0 && !_history.Any(h => h.Date == _today.Date))
            {
                _history.Add(new DailyStats
                {
                    Date = _today.Date,
                    CompletedPomodoros = _today.CompletedPomodoros,
                    TotalFocusSeconds = _today.TotalFocusSeconds,
                });
                _persistence.SaveStatsHistory(_history);
            }

            // Reset for today
            _today = new DailyStats { Date = today };
            _persistence.SaveTodayStats(_today);
        }

        public void RecordPomodoro(int focusSeconds)
        {
            _today.AddPomodoro(focusSeconds);
            _persistence.SaveTodayStats(_today);

            // Update or add today in history
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            var existing = _history.FirstOrDefault(h => h.Date == today);
            if (existing != null)
            {
                existing.CompletedPomodoros = _today.CompletedPomodoros;
                existing.TotalFocusSeconds = _today.TotalFocusSeconds;
            }
            else
            {
                _history.Add(new DailyStats
                {
                    Date = today,
                    CompletedPomodoros = _today.CompletedPomodoros,
                    TotalFocusSeconds = _today.TotalFocusSeconds,
                });
            }
            _persistence.SaveStatsHistory(_history);
        }

        public List<DailyStats> GetWeeklyStats()
        {
            var cutoff = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            return _history.Where(h => string.Compare(h.Date, cutoff, StringComparison.Ordinal) >= 0)
                          .OrderBy(h => h.Date).ToList();
        }

        public List<DailyStats> GetMonthlyStats()
        {
            var cutoff = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            return _history.Where(h => string.Compare(h.Date, cutoff, StringComparison.Ordinal) >= 0)
                          .OrderBy(h => h.Date).ToList();
        }

        public List<DailyStats> GetYearlyStats()
        {
            var cutoff = DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd");
            return _history.Where(h => string.Compare(h.Date, cutoff, StringComparison.Ordinal) >= 0)
                          .OrderBy(h => h.Date).ToList();
        }

        public int GetCurrentStreak()
        {
            int streak = 0;
            var date = DateTime.Now;

            // If today has pomodoros, include it
            if (_today.CompletedPomodoros > 0)
            {
                streak = 1;
                date = date.AddDays(-1);
            }
            else
            {
                // Check yesterday - grace period
                date = date.AddDays(-1);
            }

            var historyDict = _history.ToDictionary(h => h.Date, h => h.CompletedPomodoros);
            while (true)
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                if (historyDict.TryGetValue(dateStr, out int count) && count > 0)
                {
                    streak++;
                    date = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public int GetLongestStreak()
        {
            if (_history.Count == 0) return 0;

            var sorted = _history.Where(h => h.CompletedPomodoros > 0)
                                .OrderBy(h => h.Date).ToList();
            if (sorted.Count == 0) return 0;

            int longest = 1, current = 1;
            for (int i = 1; i < sorted.Count; i++)
            {
                if (DateTime.TryParse(sorted[i].Date, out var d2) &&
                    DateTime.TryParse(sorted[i - 1].Date, out var d1) &&
                    (d2 - d1).TotalDays == 1)
                {
                    current++;
                    longest = Math.Max(longest, current);
                }
                else
                {
                    current = 1;
                }
            }

            return Math.Max(longest, current);
        }

        public int GetTotalPomodoros()
        {
            return _history.Sum(h => h.CompletedPomodoros) + 
                   (_history.Any(h => h.Date == _today.Date) ? 0 : _today.CompletedPomodoros);
        }

        public Dictionary<string, int> GetHeatmapData()
        {
            var result = new Dictionary<string, int>();
            foreach (var entry in _history)
                result[entry.Date] = entry.CompletedPomodoros;

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_today.CompletedPomodoros > 0)
                result[today] = _today.CompletedPomodoros;

            return result;
        }
    }
}
