using System;
using System.Text.Json.Serialization;

namespace PomodoroWPF.Models
{
    public class DailyStats
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [JsonPropertyName("completed_pomodoros")]
        public int CompletedPomodoros { get; set; }

        [JsonPropertyName("total_focus_seconds")]
        public int TotalFocusSeconds { get; set; }

        public void AddPomodoro(int focusSeconds)
        {
            CompletedPomodoros++;
            TotalFocusSeconds += focusSeconds;
        }

        public string GetFormattedFocusTime()
        {
            int h = TotalFocusSeconds / 3600;
            int m = (TotalFocusSeconds % 3600) / 60;
            if (h > 0)
                return $"{h}\u5c0f\u65f6{m}\u5206\u949f";
            return $"{m}\u5206\u949f";
        }
    }
}
