using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _vm;

        public SettingsWindow(SettingsViewModel vm)
        {
            _vm = vm;
            DataContext = vm;

            var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);

            Title = "\u8bbe\u7f6e";
            Width = 520;
            Height = 560;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Card));
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            BuildUI(tc);
        }

        private void BuildUI(ThemeManager.ThemeColors tc)
        {
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            var panel = new StackPanel { Margin = new Thickness(30, 24, 30, 24) };
            scroll.Content = panel;
            Content = scroll;

            // Title
            panel.Children.Add(new TextBlock
            {
                Text = "\u2699 \u8bbe\u7f6e",
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
            });

            // Theme selection
            AddSection(panel, tc, "\u989c\u8272\u4e3b\u9898", () =>
            {
                var themePanel = new StackPanel { Orientation = Orientation.Horizontal };
                foreach (var theme in _vm.AvailableThemes)
                {
                    var btn = CreateButton(theme.Name, theme.AccentColor, "#000000");
                    btn.Margin = new Thickness(4);
                    btn.MouseLeftButtonDown += (_, _) =>
                    {
                        _vm.ApplyThemeCommand.Execute(theme.Id);
                        // Rebuild UI with new theme
                        var newTc = ThemeManager.GetCurrent(_vm.SelectedTheme);
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(newTc.Card));
                        panel.Children.Clear();
                        BuildUI(newTc);
                        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = panel };
                    };
                    themePanel.Children.Add(btn);
                }
                panel.Children.Add(themePanel);
            });

            // Toggles
            AddToggle(panel, tc, "\u5012\u8ba1\u65f6\u6700\u540e 10 \u79d2\u6ef4\u7b54\u58f0",
                () => _vm.TickSoundEnabled, v => _vm.TickSoundEnabled = v);

            AddToggle(panel, tc, "\u756a\u8304\u7ed3\u675f\u540e\u81ea\u52a8\u5f00\u59cb\u4f11\u606f",
                () => _vm.AutoBreak, v => _vm.AutoBreak = v);

            AddToggle(panel, tc, "\u767d\u566a\u97f3\uff08\u4e13\u6ce8\u65f6\u64ad\u653e\uff09",
                () => _vm.AmbientSoundEnabled, v => _vm.AmbientSoundEnabled = v);

            // Ambient sound type
            AddSection(panel, tc, "\u767d\u566a\u97f3\u7c7b\u578b", () =>
            {
                var typePanel = new StackPanel { Orientation = Orientation.Horizontal };
                foreach (var (type, name) in new (string, string)[] { ("rain", "\u2248 \u96e8\u58f0"), ("wind", "\u223c \u98ce\u58f0"), ("cafe", "\u2615 \u5496\u5561\u5385") })
                {
                    bool isSelected = _vm.AmbientSoundType == type;
                    var btn = CreateButton(name, isSelected ? tc.Accent : tc.TextMuted, isSelected ? "#000000" : tc.TextDim);
                    btn.Margin = new Thickness(4);
                    btn.MouseLeftButtonDown += (_, _) =>
                    {
                        _vm.AmbientSoundType = type;
                        RebuildSelf(tc);
                    };
                    typePanel.Children.Add(btn);
                }
                panel.Children.Add(typePanel);
            });

            // Numeric settings
            AddNumericInput(panel, tc, "\u5de5\u4f5c\u65f6\u957f\uff08\u5206\u949f\uff09",
                () => _vm.WorkDurationMinutes, v => _vm.WorkDurationMinutes = v, 1, 120);

            AddNumericInput(panel, tc, "\u77ed\u4f11\u606f\u65f6\u957f\uff08\u5206\u949f\uff09",
                () => _vm.BreakDurationMinutes, v => _vm.BreakDurationMinutes = v, 1, 30);

            AddNumericInput(panel, tc, "\u957f\u4f11\u606f\u65f6\u957f\uff08\u5206\u949f\uff09",
                () => _vm.LongBreakDurationMinutes, v => _vm.LongBreakDurationMinutes = v, 1, 60);

            AddNumericInput(panel, tc, "\u6bcf N \u4e2a\u756a\u8304\u540e\u957f\u4f11\u606f",
                () => _vm.PomodorosBeforeLongBreak, v => _vm.PomodorosBeforeLongBreak = v, 1, 10);

            AddNumericInput(panel, tc, "\u6bcf\u65e5\u76ee\u6807\u756a\u8304\u6570",
                () => _vm.DailyGoalPomodoros, v => _vm.DailyGoalPomodoros = v, 0, 50);

            // Buttons
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 16, 0, 0) };

            var saveBtn = CreateButton("\u4fdd \u5b58", tc.Accent, "#000000");
            saveBtn.MouseLeftButtonDown += (_, _) => _vm.SaveCommand.Execute(null);
            btnPanel.Children.Add(saveBtn);

            var closeBtn = CreateButton("\u5173 \u95ed", tc.TextMuted, tc.TextDim);
            closeBtn.MouseLeftButtonDown += (_, _) => Close();
            btnPanel.Children.Add(closeBtn);

            panel.Children.Add(btnPanel);

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SettingsViewModel.DialogResult) && _vm.DialogResult == true)
                    Close();
            };
        }

        private void RebuildSelf(ThemeManager.ThemeColors oldTc)
        {
            var newTc = ThemeManager.GetCurrent(_vm.SelectedTheme);
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(newTc.Card));
            var newPanel = new StackPanel { Margin = new Thickness(30, 24, 30, 24) };
            BuildUI(newTc);
        }

        private void AddSection(StackPanel panel, ThemeManager.ThemeColors tc, string label, Action buildContent)
        {
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                Margin = new Thickness(0, 4, 0, 6),
            });
            buildContent();
        }

        private void AddToggle(StackPanel panel, ThemeManager.ThemeColors tc, string label, Func<bool> getter, Action<bool> setter)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 12) };
            row.Children.Add(new TextBlock
            {
                Text = label,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 15,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            });

            bool val = getter();
            var toggle = CreateButton(val ? "\u5f00" : "\u5173", val ? tc.Accent : tc.TextMuted, val ? "#000000" : tc.TextDim);
            toggle.MouseLeftButtonDown += (_, _) =>
            {
                setter(!getter());
                bool newVal = getter();
                toggle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(newVal ? tc.Accent : tc.TextMuted));
                ((TextBlock)toggle.Child).Text = newVal ? "\u5f00" : "\u5173";
            };
            row.Children.Add(toggle);
            panel.Children.Add(row);
        }

        private void AddNumericInput(StackPanel panel, ThemeManager.ThemeColors tc, string label,
            Func<int> getter, Action<int> setter, int min, int max)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 12) };
            row.Children.Add(new TextBlock
            {
                Text = label,
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 15,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            });

            var input = new TextBox
            {
                Text = getter().ToString(),
                FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 15,
                Width = 50,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Bg)),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Accent)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.CardBorder)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4, 2, 4, 2),
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            input.TextChanged += (_, _) =>
            {
                if (int.TryParse(input.Text, out int v))
                    setter(Math.Max(min, Math.Min(max, v)));
            };
            row.Children.Add(input);
            panel.Children.Add(row);
        }

        private Border CreateButton(string text, string bg, string fg)
        {
            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(24, 6, 24, 6),
                Margin = new Thickness(4),
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Microsoft YaHei"), FontSize = 14, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
        }
    }
}
