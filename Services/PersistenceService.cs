using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PomodoroWPF.Models;

namespace PomodoroWPF.Services
{
    public class PersistenceService
    {
        private readonly string _baseDir;

        private string SettingsFile => Path.Combine(_baseDir, "settings.json");
        private string StatsFile => Path.Combine(_baseDir, "stats.json");
        private string StatsHistoryFile => Path.Combine(_baseDir, "stats_history.json");
        private string TasksFile => Path.Combine(_baseDir, "tasks.json");
        private string AchievementsFile => Path.Combine(_baseDir, "achievements.json");

        private static readonly JsonSerializerOptions IndentedOptions = new()
        {
            WriteIndented = true,
        };

        public PersistenceService()
        {
            _baseDir = Path.GetDirectoryName(Environment.ProcessPath)
                ?? AppDomain.CurrentDomain.BaseDirectory;
        }

        // ===== Settings =====
        public AppSettings LoadSettings()
        {
            try
            {
                string json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, IndentedOptions);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveSettings] {ex.Message}");
            }
        }

        // ===== Today Stats =====
        public DailyStats LoadTodayStats()
        {
            try
            {
                string json = File.ReadAllText(StatsFile);
                return JsonSerializer.Deserialize<DailyStats>(json) ?? new DailyStats();
            }
            catch
            {
                return new DailyStats();
            }
        }

        public void SaveTodayStats(DailyStats stats)
        {
            try
            {
                string json = JsonSerializer.Serialize(stats, IndentedOptions);
                File.WriteAllText(StatsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTodayStats] {ex.Message}");
            }
        }

        // ===== Stats History =====
        public List<DailyStats> LoadStatsHistory()
        {
            try
            {
                string json = File.ReadAllText(StatsHistoryFile);
                return JsonSerializer.Deserialize<List<DailyStats>>(json) ?? new List<DailyStats>();
            }
            catch
            {
                return new List<DailyStats>();
            }
        }

        public void SaveStatsHistory(List<DailyStats> history)
        {
            try
            {
                string json = JsonSerializer.Serialize(history, IndentedOptions);
                File.WriteAllText(StatsHistoryFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveStatsHistory] {ex.Message}");
            }
        }

        // ===== Tasks =====
        public TaskStore LoadTasks()
        {
            try
            {
                string json = File.ReadAllText(TasksFile);

                // Try new format first
                var store = JsonSerializer.Deserialize<TaskStore>(json);
                if (store != null)
                    return store;

                return new TaskStore();
            }
            catch
            {
                // Try legacy format migration
                return TryMigrateLegacyTasks();
            }
        }

        private TaskStore TryMigrateLegacyTasks()
        {
            try
            {
                string json = File.ReadAllText(TasksFile);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("current_task", out var taskElement))
                {
                    string taskName = taskElement.GetString() ?? "";
                    var store = new TaskStore();
                    if (!string.IsNullOrWhiteSpace(taskName))
                    {
                        var task = new PomodoroTask
                        {
                            Name = taskName,
                            EstimatedPomodoros = 1,
                            Priority = Priority.Medium,
                        };
                        store.Tasks.Add(task);
                        store.CurrentTaskId = task.Id;
                    }
                    SaveTasks(store);
                    return store;
                }
            }
            catch { }

            return new TaskStore();
        }

        public void SaveTasks(TaskStore store)
        {
            try
            {
                string json = JsonSerializer.Serialize(store, IndentedOptions);
                File.WriteAllText(TasksFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTasks] {ex.Message}");
            }
        }

        // ===== Achievements =====
        public List<Achievement> LoadAchievements()
        {
            try
            {
                string json = File.ReadAllText(AchievementsFile);
                return JsonSerializer.Deserialize<List<Achievement>>(json) ?? new List<Achievement>();
            }
            catch
            {
                return new List<Achievement>();
            }
        }

        public void SaveAchievements(List<Achievement> achievements)
        {
            try
            {
                string json = JsonSerializer.Serialize(achievements, IndentedOptions);
                File.WriteAllText(AchievementsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveAchievements] {ex.Message}");
            }
        }
    }

    public class TaskStore
    {
        [System.Text.Json.Serialization.JsonPropertyName("tasks")]
        public List<PomodoroTask> Tasks { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("current_task_id")]
        public string? CurrentTaskId { get; set; }
    }
}
