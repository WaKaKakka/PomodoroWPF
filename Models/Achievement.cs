using System;
using System.Text.Json.Serialization;

namespace PomodoroWPF.Models
{
    public class Achievement
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("icon_emoji")]
        public string IconEmoji { get; set; } = "";

        [JsonPropertyName("is_unlocked")]
        public bool IsUnlocked { get; set; }

        [JsonPropertyName("unlocked_at")]
        public DateTime? UnlockedAt { get; set; }
    }
}
