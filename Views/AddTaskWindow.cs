using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PomodoroWPF.Models;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF.Views
{
    public partial class AddTaskWindow : Window
    {
        private readonly PomodoroTask? _editTask;
        private string _result = "";
        private int _estimated = 1;
        private Priority _priority = Priority.Medium;

        public AddTaskWindow(PomodoroTask? editTask = null)
        {
            _editTask = editTask;
            var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);

            Title = editTask != null ? "\u7f16\u8f91\u4efb\u52a1" : "\u6dfb\u52a0\u4efb\u52a1";
            Width = 440;
            Height = 320;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Card));
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var panel = new StackPanel { Margin = new Thickness(30, 24, 30, 24) };
            Content = panel;

            // Title
            panel.Children.Add(new TextBlock
            {
                Text = editTask != null ? "\u7f16\u8f91\u4efb\u52a1" : "\u6dfb\u52a0\u65b0\u4efb\u52a1",
                FontFamily = new FontFamily("Microsoft YaHei"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
            });

            // Name input
            panel.Children.Add(new TextBlock
            {
                Text = "\u4efb\u52a1\u540d\u79f0",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 0, 0, 4),
            });

            var nameInput = new TextBox
            {
                Text = editTask?.Name ?? "",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 16,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Bg)),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 0, 12),
            };
            panel.Children.Add(nameInput);

            // Estimated pomodoros
            panel.Children.Add(new TextBlock
            {
                Text = "\u9884\u4f30\u756a\u8304\u6570",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 0, 0, 4),
            });

            var estInput = new TextBox
            {
                Text = (editTask?.EstimatedPomodoros ?? 1).ToString(),
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 16,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Bg)),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8, 4, 8, 4),
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
            };
            panel.Children.Add(estInput);

            // Priority
            panel.Children.Add(new TextBlock
            {
                Text = "\u4f18\u5148\u7ea7",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 0, 0, 4),
            });

            var priorityPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
            var highBtn = CreatePriorityButton("\u9ad8", "#ef4444", editTask?.Priority == Priority.High);
            var medBtn = CreatePriorityButton("\u4e2d", tc.Accent, editTask?.Priority != Priority.High && editTask?.Priority != Priority.Low);
            var lowBtn = CreatePriorityButton("\u4f4e", "#6b7280", editTask?.Priority == Priority.Low);

            highBtn.MouseLeftButtonDown += (_, _) => { _priority = Priority.High; UpdatePriorityColors(highBtn, medBtn, lowBtn, tc); };
            medBtn.MouseLeftButtonDown += (_, _) => { _priority = Priority.Medium; UpdatePriorityColors(highBtn, medBtn, lowBtn, tc); };
            lowBtn.MouseLeftButtonDown += (_, _) => { _priority = Priority.Low; UpdatePriorityColors(highBtn, medBtn, lowBtn, tc); };

            if (editTask != null) _priority = editTask.Priority;

            priorityPanel.Children.Add(highBtn);
            priorityPanel.Children.Add(medBtn);
            priorityPanel.Children.Add(lowBtn);
            panel.Children.Add(priorityPanel);

            // Buttons
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            var confirmBtn = CreateButton("\u786e \u5b9a", tc.Accent, "#000000");
            confirmBtn.MouseLeftButtonDown += (_, _) =>
            {
                _result = nameInput.Text;
                if (!int.TryParse(estInput.Text, out _estimated) || _estimated < 1) _estimated = 1;
                DialogResult = true;
                Close();
            };
            btnPanel.Children.Add(confirmBtn);

            var cancelBtn = CreateButton("\u53d6 \u6d88", tc.TextMuted, tc.TextDim);
            cancelBtn.MouseLeftButtonDown += (_, _) => Close();
            btnPanel.Children.Add(cancelBtn);

            panel.Children.Add(btnPanel);

            nameInput.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    _result = nameInput.Text;
                    if (!int.TryParse(estInput.Text, out _estimated) || _estimated < 1) _estimated = 1;
                    DialogResult = true;
                    Close();
                }
            };

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            Loaded += (_, _) => { nameInput.Focus(); nameInput.SelectAll(); };
        }

        public (string Name, int Estimated, Priority Priority) GetResult() => (_result, _estimated, _priority);

        private Border CreatePriorityButton(string text, string color, bool isSelected)
        {
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isSelected ? color : "#44403c")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(16, 4, 16, 4),
                Margin = new Thickness(4, 0, 4, 0),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                Tag = color,
            };
        }

        private void UpdatePriorityColors(Border high, Border med, Border low, ThemeManager.ThemeColors tc)
        {
            high.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_priority == Priority.High ? "#ef4444" : "#44403c"));
            med.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_priority == Priority.Medium ? tc.Accent : "#44403c"));
            low.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_priority == Priority.Low ? "#6b7280" : "#44403c"));
        }

        private Border CreateButton(string text, string bg, string fg)
        {
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(36, 7, 36, 7),
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
