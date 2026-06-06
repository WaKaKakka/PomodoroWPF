using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PomodoroWPF.Controls;
using PomodoroWPF.Services;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF.Views
{
    public partial class StatsWindow : Window
    {
        private readonly StatsViewModel _vm;

        public StatsWindow(StatsViewModel vm)
        {
            _vm = vm;
            var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);

            Title = "\u7edf\u8ba1";
            Width = 620;
            Height = 600;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Card));
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            BuildUI(tc);
        }

        private void BuildUI(ThemeManager.ThemeColors tc)
        {
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var panel = new StackPanel { Margin = new Thickness(24) };
            scroll.Content = panel;
            Content = scroll;

            // Title
            panel.Children.Add(new TextBlock
            {
                Text = "\u25a3 \u7edf\u8ba1\u6982\u89c8",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
            });

            // Summary cards
            var cardsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            cardsPanel.Children.Add(CreateCard(tc, "\u4eca\u65e5", _vm.TodayPomodoroCount + " \u25cf", _vm.TodayFocusTime));
            cardsPanel.Children.Add(CreateCard(tc, "\u672c\u5468", _vm.WeeklyTotal + " \u25cf", _vm.WeeklyFocusTime));
            cardsPanel.Children.Add(CreateCard(tc, "\u672c\u6708", _vm.MonthlyTotal + " \u25cf", _vm.MonthlyFocusTime));
            panel.Children.Add(cardsPanel);

            // Streak info
            var streakPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 16), HorizontalAlignment = HorizontalAlignment.Center };
            streakPanel.Children.Add(new TextBlock
            {
                Text = $"\u25b2 \u8fde\u7eed {_vm.CurrentStreak} \u5929",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                Margin = new Thickness(0, 0, 24, 0),
                VerticalAlignment = VerticalAlignment.Center,
            });
            streakPanel.Children.Add(new TextBlock
            {
                Text = $"\u6700\u957f {_vm.LongestStreak} \u5929  \u00b7  \u7d2f\u8ba1 {_vm.AllTimeTotal} \u25cf",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                VerticalAlignment = VerticalAlignment.Center,
            });
            panel.Children.Add(streakPanel);

            // Heatmap
            panel.Children.Add(new TextBlock
            {
                Text = "\u25a6 \u4e13\u6ce8\u70ed\u529b\u56fe\uff08\u8fd1 16 \u5468\uff09",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                Margin = new Thickness(0, 8, 0, 8),
            });

            var heatmap = new HeatmapControl
            {
                AccentColor = tc.Accent,
                MutedColor = tc.TextMuted,
                CellSize = 12,
                CellSpacing = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            var cells = _vm.HeatmapCells.Select(c => new HeatmapCellData
            {
                Date = c.Date,
                Count = c.Count,
                IntensityLevel = c.IntensityLevel,
            }).ToList();
            heatmap.SetCells(cells);

            var heatmapBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Child = heatmap,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            panel.Children.Add(heatmapBorder);

            // Achievements
            panel.Children.Add(new TextBlock
            {
                Text = "\u2605 \u6210\u5c31",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                Margin = new Thickness(0, 20, 0, 8),
            });

            var achievePanel = new WrapPanel();
            foreach (var a in _vm.Achievements)
            {
                achievePanel.Children.Add(CreateAchievementBadge(tc, a));
            }
            panel.Children.Add(achievePanel);

            // Export + Close buttons
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) };

            var exportBtn = CreateButton("\u5bfc\u51fa\u6570\u636e", tc.Accent, "#000000");
            exportBtn.MouseLeftButtonDown += (_, _) => ExportData();
            btnPanel.Children.Add(exportBtn);

            var closeBtn = CreateButton("\u5173 \u95ed", tc.TextMuted, tc.TextDim);
            closeBtn.MouseLeftButtonDown += (_, _) => Close();
            btnPanel.Children.Add(closeBtn);

            panel.Children.Add(btnPanel);

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private Border CreateCard(ThemeManager.ThemeColors tc, string title, string count, string time)
        {
            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(4),
                Width = 170,
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
            });
            stack.Children.Add(new TextBlock
            {
                Text = count,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 22, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                Margin = new Thickness(0, 4, 0, 0),
            });
            stack.Children.Add(new TextBlock
            {
                Text = time,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 2, 0, 0),
            });
            card.Child = stack;
            return card;
        }

        private Border CreateAchievementBadge(ThemeManager.ThemeColors tc, Models.Achievement a)
        {
            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    a.IsUnlocked ? tc.Accent + "20" : tc.Bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(4),
                Width = 190,
                BorderThickness = new Thickness(a.IsUnlocked ? 1 : 0),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    a.IsUnlocked ? tc.Accent : "Transparent")),
                ToolTip = a.Description,
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                // WPF 不支持彩色 Emoji 字体渲染，使用基础 Unicode 符号替代
                Text = a.IsUnlocked ? $"\u2605 {a.Name}" : $"\u25CB {a.Name}",
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Microsoft YaHei"), 
                FontSize = 14, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    a.IsUnlocked ? tc.Accent : tc.TextDim)),
            });
            stack.Children.Add(new TextBlock
            {
                Text = a.Description,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 2, 0, 0),
            });
            badge.Child = stack;
            return badge;
        }

        private void ExportData()
        {
            var statsService = Infrastructure.ServiceLocator.Resolve<StatsService>();
            var taskService = Infrastructure.ServiceLocator.Resolve<TaskService>();
            var exportService = new DataExportService();

            var dlg = new SaveFileDialog
            {
                Filter = "CSV \u6587\u4ef6|*.csv|JSON \u6587\u4ef6|*.json",
                FileName = $"\u756a\u8304\u949f\u7edf\u8ba1_{DateTime.Now:yyyyMMdd}.csv",
            };

            if (dlg.ShowDialog() == true)
            {
                var allStats = statsService.GetYearlyStats();
                if (dlg.FilterIndex == 1)
                    exportService.ExportStatsToCsv(allStats, dlg.FileName);
                else
                    exportService.ExportStatsToJson(allStats, dlg.FileName);

                MessageBox.Show($"\u6570\u636e\u5df2\u5bfc\u51fa\u5230\uff1a\n{dlg.FileName}", "\u5bfc\u51fa\u6210\u529f",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Border CreateButton(string text, string bg, string fg)
        {
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(24, 7, 24, 7),
                Margin = new Thickness(4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 15, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
        }
    }
}
