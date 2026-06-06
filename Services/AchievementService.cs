using System;
using System.Collections.Generic;
using System.Linq;
using PomodoroWPF.Models;

namespace PomodoroWPF.Services
{
    public class AchievementService
    {
        private readonly PersistenceService _persistence;
        private readonly StatsService _statsService;
        private List<Achievement> _achievements;

        public List<Achievement> Achievements => _achievements;

        private static readonly Achievement[] DefaultAchievements = new[]
        {
            new Achievement { Id = "first_pomodoro", Name = "\u521d\u7aa5\u95e8\u5f84", Description = "\u5b8c\u6210\u7b2c 1 \u4e2a\u756a\u8304", IconEmoji = "\u25cf" },
            new Achievement { Id = "four_in_day", Name = "\u65e5\u8fdb\u56db\u8304", Description = "\u5355\u65e5\u5b8c\u6210 4 \u4e2a\u756a\u8304", IconEmoji = "\u25cf\u25cf\u25cf\u25cf" },
            new Achievement { Id = "streak_7", Name = "\u4e00\u5468\u575a\u6301", Description = "\u8fde\u7eed 7 \u5929\u6709\u756a\u8304", IconEmoji = "\u25b2" },
            new Achievement { Id = "streak_30", Name = "\u6708\u4e0d\u95f4\u65ad", Description = "\u8fde\u7eed 30 \u5929\u6709\u756a\u8304", IconEmoji = "\u25b2\u25b2" },
            new Achievement { Id = "total_100", Name = "\u767e\u8304\u8fbe\u6210", Description = "\u7d2f\u8ba1 100 \u4e2a\u756a\u8304", IconEmoji = "\u2605" },
        };

        public AchievementService(PersistenceService persistence, StatsService statsService)
        {
            _persistence = persistence;
            _statsService = statsService;
            _achievements = persistence.LoadAchievements();

            // Ensure all default achievements exist
            foreach (var def in DefaultAchievements)
            {
                if (!_achievements.Any(a => a.Id == def.Id))
                    _achievements.Add(def);
            }
        }

        public string? CheckAchievements()
        {
            var newlyUnlocked = new List<string>();

            // First pomodoro
            if (TryUnlock("first_pomodoro",
                () => _statsService.GetTotalPomodoros() >= 1))
                newlyUnlocked.Add(FormatAchievementName(GetAchievement("first_pomodoro")));

            // 4 in a day
            if (TryUnlock("four_in_day",
                () => _statsService.Today.CompletedPomodoros >= 4))
                newlyUnlocked.Add(FormatAchievementName(GetAchievement("four_in_day")));

            // 7 day streak
            if (TryUnlock("streak_7",
                () => _statsService.GetCurrentStreak() >= 7))
                newlyUnlocked.Add(FormatAchievementName(GetAchievement("streak_7")));

            // 30 day streak
            if (TryUnlock("streak_30",
                () => _statsService.GetCurrentStreak() >= 30))
                newlyUnlocked.Add(FormatAchievementName(GetAchievement("streak_30")));

            // 100 total
            if (TryUnlock("total_100",
                () => _statsService.GetTotalPomodoros() >= 100))
                newlyUnlocked.Add(FormatAchievementName(GetAchievement("total_100")));

            if (newlyUnlocked.Count > 0)
                _persistence.SaveAchievements(_achievements);

            return newlyUnlocked.Count > 0 ? string.Join("\n", newlyUnlocked) : null;
        }

        /// <summary>
        /// 格式化成就名称用于通知显示（包含 Emoji）
        /// </summary>
        private static string FormatAchievementName(Achievement? a)
        {
            if (a == null) return string.Empty;
            return $"\u2605 {a.Name}";
        }

        private bool TryUnlock(string id, Func<bool> condition)
        {
            var a = GetAchievement(id);
            if (a == null || a.IsUnlocked) return false;

            if (condition())
            {
                a.IsUnlocked = true;
                a.UnlockedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        private Achievement? GetAchievement(string id) =>
            _achievements.FirstOrDefault(a => a.Id == id);
    }
}
