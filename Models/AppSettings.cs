using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PomodoroWPF.Models
{
    public class AppSettings
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark_gold";

        [JsonPropertyName("tick_sound_enabled")]
        public bool TickSoundEnabled { get; set; } = true;

        [JsonPropertyName("break_duration_minutes")]
        public int BreakDurationMinutes { get; set; } = 5;

        [JsonPropertyName("long_break_duration_minutes")]
        public int LongBreakDurationMinutes { get; set; } = 15;

        [JsonPropertyName("auto_break")]
        public bool AutoBreak { get; set; } = true;

        [JsonPropertyName("pomodoros_before_long_break")]
        public int PomodorosBeforeLongBreak { get; set; } = 4;

        [JsonPropertyName("cycle_position")]
        public int CyclePosition { get; set; } = 0;

        [JsonPropertyName("work_duration_minutes")]
        public int WorkDurationMinutes { get; set; } = 25;

        [JsonPropertyName("daily_goal_pomodoros")]
        public int DailyGoalPomodoros { get; set; } = 8;

        [JsonPropertyName("ambient_sound_enabled")]
        public bool AmbientSoundEnabled { get; set; } = false;

        [JsonPropertyName("ambient_sound_type")]
        public string AmbientSoundType { get; set; } = "rain";
    }
}
