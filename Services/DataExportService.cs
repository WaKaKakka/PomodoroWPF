using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using PomodoroWPF.Models;

namespace PomodoroWPF.Services
{
    public class DataExportService
    {
        public void ExportStatsToCsv(List<DailyStats> stats, string filePath)
        {
            var sb = new StringBuilder();
            // UTF-8 BOM for Excel compatibility
            sb.Append('\ufeff');
            sb.AppendLine("\u65e5\u671f,\u5b8c\u6210\u756a\u8304\u6570,\u4e13\u6ce8\u65f6\u957f(\u79d2),\u4e13\u6ce8\u65f6\u957f(\u5c0f\u65f6)");

            foreach (var s in stats.OrderBy(x => x.Date))
            {
                double hours = Math.Round(s.TotalFocusSeconds / 3600.0, 2);
                sb.AppendLine($"{s.Date},{s.CompletedPomodoros},{s.TotalFocusSeconds},{hours}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public void ExportStatsToJson(List<DailyStats> stats, string filePath)
        {
            var json = JsonSerializer.Serialize(stats.OrderBy(x => x.Date).ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public void ExportTasksToJson(List<PomodoroTask> tasks, string filePath)
        {
            var json = JsonSerializer.Serialize(tasks,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
    }
}
