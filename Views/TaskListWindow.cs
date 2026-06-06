using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PomodoroWPF.Models;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF.Views
{
    public partial class TaskListWindow : Window
    {
        private readonly TaskListViewModel _vm;

        public TaskListWindow(TaskListViewModel vm)
        {
            _vm = vm;
            var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);

            Title = "\u4efb\u52a1\u5217\u8868";
            Width = 560;
            Height = 500;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Card));
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            BuildUI(tc);
        }

        private void BuildUI(ThemeManager.ThemeColors tc)
        {
            var root = new Grid { Margin = new Thickness(24) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Content = root;

            // Header
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            header.Children.Add(new TextBlock
            {
                Text = "\U0001F4CB \u4efb\u52a1\u5217\u8868",
                FontFamily = new FontFamily("Segoe UI"), FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                VerticalAlignment = VerticalAlignment.Center,
            });

            var summaryText = new TextBlock
            {
                Text = _vm.SummaryText,
                FontFamily = new FontFamily("Segoe UI"), FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
            };
            header.Children.Add(summaryText);

            var addBtn = CreateButton("+ \u6dfb\u52a0", tc.Accent, "#000000");
            addBtn.HorizontalAlignment = HorizontalAlignment.Right;
            addBtn.Margin = new Thickness(0, 0, 0, 0);
            addBtn.MouseLeftButtonDown += (_, _) =>
            {
                var addDlg = new AddTaskWindow();
                addDlg.Owner = this;
                if (addDlg.ShowDialog() == true)
                {
                    var (name, est, pri) = addDlg.GetResult();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        _vm.AddNewTask(name, est, pri);
                        RefreshList();
                        summaryText.Text = _vm.SummaryText;
                    }
                }
            };
            header.Children.Add(addBtn);

            Grid.SetRow(header, 0);
            root.Children.Add(header);

            // Task list
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };
            var listPanel = new StackPanel { Name = "TaskListPanel" };
            scrollViewer.Content = listPanel;
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            PopulateTasks(listPanel, tc, summaryText);

            // Close button
            var closeBtn = CreateButton("\u5173 \u95ed", tc.TextMuted, tc.TextDim);
            closeBtn.HorizontalAlignment = HorizontalAlignment.Center;
            closeBtn.Margin = new Thickness(0, 12, 0, 0);
            closeBtn.MouseLeftButtonDown += (_, _) => Close();
            Grid.SetRow(closeBtn, 2);
            root.Children.Add(closeBtn);

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void PopulateTasks(StackPanel panel, ThemeManager.ThemeColors tc, TextBlock summaryText)
        {
            panel.Children.Clear();

            foreach (var task in _vm.Tasks)
            {
                var row = CreateTaskRow(task, tc, panel, summaryText);
                panel.Children.Add(row);
            }

            if (_vm.Tasks.Count == 0)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "\u6682\u65e0\u4efb\u52a1\uff0c\u70b9\u51fb\u201c+ \u6dfb\u52a0\u201d\u5f00\u59cb",
                    FontFamily = new FontFamily("Segoe UI"), FontSize = 13,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0),
                });
            }
        }

        private Border CreateTaskRow(PomodoroTask task, ThemeManager.ThemeColors tc, StackPanel parentPanel, TextBlock summaryText)
        {
            bool isCurrent = task.Id == _vm.Tasks.OfType<PomodoroTask>()
                .FirstOrDefault(t => t.Id == task.Id)?.Id &&
                task == _vm.Tasks.FirstOrDefault(t =>
                {
                    var svc = Infrastructure.ServiceLocator.TryResolve<Services.TaskService>();
                    return svc?.CurrentTaskId == task.Id;
                });

            var priorityColor = task.Priority switch
            {
                Priority.High => "#ef4444",
                Priority.Medium => tc.Accent,
                Priority.Low => "#6b7280",
                _ => tc.Accent
            };

            var row = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    task.IsCompleted ? tc.Bg : tc.Card)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(isCurrent ? 1 : 0),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    isCurrent ? tc.Accent : "Transparent")),
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) }); // priority bar
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // content
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // actions
            row.Child = grid;

            // Priority bar
            var pBar = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(priorityColor)),
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 10, 0),
                Width = 4,
            };
            Grid.SetColumn(pBar, 0);
            grid.Children.Add(pBar);

            // Content
            var content = new StackPanel { Margin = new Thickness(4, 0, 0, 0) };
            content.Children.Add(new TextBlock
            {
                Text = task.IsCompleted ? $"\u2713 {task.Name}" : task.Name,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                FontWeight = task.IsCompleted ? FontWeights.Normal : FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    task.IsCompleted ? tc.TextDim : tc.Text)),
                TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null,
            });
            content.Children.Add(new TextBlock
            {
                Text = $"{task.PomodoroDisplay}  \u00b7  {task.PriorityLabel}",
                FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 2, 0, 0),
            });
            Grid.SetColumn(content, 1);
            grid.Children.Add(content);

            // Action buttons
            var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            if (!task.IsCompleted)
            {
                var selectBtn = CreateSmallButton("\u2605", isCurrent ? tc.Accent : tc.TextDim);
                selectBtn.ToolTip = "\u8bbe\u4e3a\u5f53\u524d\u4efb\u52a1";
                selectBtn.MouseLeftButtonDown += (_, _) =>
                {
                    var svc = Infrastructure.ServiceLocator.TryResolve<Services.TaskService>();
                    svc?.SetCurrentTask(task.Id);
                    RefreshList();
                    summaryText.Text = _vm.SummaryText;
                };
                actions.Children.Add(selectBtn);
            }

            var completeBtn = CreateSmallButton(task.IsCompleted ? "\u21a9" : "\u2713", tc.Success);
            completeBtn.ToolTip = task.IsCompleted ? "\u53d6\u6d88\u5b8c\u6210" : "\u5b8c\u6210";
            completeBtn.MouseLeftButtonDown += (_, _) =>
            {
                if (!task.IsCompleted)
                {
                    var svc = Infrastructure.ServiceLocator.TryResolve<Services.TaskService>();
                    svc?.MarkComplete(task.Id);
                }
                else
                {
                    task.IsCompleted = false;
                }
                RefreshList();
                summaryText.Text = _vm.SummaryText;
            };
            actions.Children.Add(completeBtn);

            var deleteBtn = CreateSmallButton("\u2715", "#ef4444");
            deleteBtn.ToolTip = "\u5220\u9664";
            deleteBtn.MouseLeftButtonDown += (_, _) =>
            {
                _vm.DeleteTaskCommand.Execute(task);
                RefreshList();
                summaryText.Text = _vm.SummaryText;
            };
            actions.Children.Add(deleteBtn);

            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);

            return row;
        }

        private void RefreshList()
        {
            var root = (Grid)Content;
            var sv = root.Children.OfType<ScrollViewer>().FirstOrDefault();
            if (sv?.Content is StackPanel panel)
            {
                var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);
                var summary = root.Children.OfType<StackPanel>().FirstOrDefault()
                    ?.Children.OfType<TextBlock>().FirstOrDefault(t => t.Text.Contains("/"));
                PopulateTasks(panel, tc, summary!);
            }
        }

        private Border CreateSmallButton(string text, string color)
        {
            var btn = new Border
            {
                Background = new SolidColorBrush(Colors.Transparent),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(2, 0, 2, 0),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Segoe UI"), FontSize = 13,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
            btn.MouseEnter += (_, _) => btn.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
            btn.MouseLeave += (_, _) => btn.Background = new SolidColorBrush(Colors.Transparent);
            return btn;
        }

        private Border CreateButton(string text, string bg, string fg)
        {
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(20, 6, 20, 6),
                Margin = new Thickness(4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Segoe UI"), FontSize = 12, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
        }
    }
}
