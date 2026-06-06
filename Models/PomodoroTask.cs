using System;
using System.Text.Json.Serialization;
using PomodoroWPF.Infrastructure;

namespace PomodoroWPF.Models
{
    public enum Priority
    {
        High = 1,
        Medium = 2,
        Low = 3
    }

    public class PomodoroTask : ViewModelBase
    {
        private string _id = Guid.NewGuid().ToString("N");
        private string _name = "";
        private int _estimatedPomodoros = 1;
        private int _actualPomodoros;
        private Priority _priority = Priority.Medium;
        private DateTime _createdAt = DateTime.Now;
        private DateTime? _completedAt;
        private bool _isCompleted;

        [JsonPropertyName("id")]
        public string Id { get => _id; set => SetProperty(ref _id, value); }

        [JsonPropertyName("name")]
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        [JsonPropertyName("estimated_pomodoros")]
        public int EstimatedPomodoros { get => _estimatedPomodoros; set => SetProperty(ref _estimatedPomodoros, value); }

        [JsonPropertyName("actual_pomodoros")]
        public int ActualPomodoros { get => _actualPomodoros; set => SetProperty(ref _actualPomodoros, value); }

        [JsonPropertyName("priority")]
        public Priority Priority { get => _priority; set => SetProperty(ref _priority, value); }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get => _completedAt; set => SetProperty(ref _completedAt, value); }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    if (value && !_completedAt.HasValue)
                        CompletedAt = DateTime.Now;
                    else if (!value)
                        CompletedAt = null;
                }
            }
        }

        public string PriorityLabel => Priority switch
        {
            Priority.High => "\u9ad8",
            Priority.Medium => "\u4e2d",
            Priority.Low => "\u4f4e",
            _ => "\u4e2d"
        };

        public string PomodoroDisplay => $"{ActualPomodoros}/{EstimatedPomodoros} \U0001F345";
    }
}
