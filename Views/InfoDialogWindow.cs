using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PomodoroWPF.Views
{
    public partial class InfoDialogWindow : Window
    {
        public InfoDialogWindow(string title, string message)
        {
            var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);

            Title = title;
            Width = 380;
            Height = 180;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Card));
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var mainPanel = new System.Windows.Controls.StackPanel();
            Content = mainPanel;

            mainPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = title,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.Text)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 28, 0, 6),
            });

            mainPanel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tc.TextDim)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
                TextWrapping = TextWrapping.Wrap,
            });

            var okBtn = CreateButton("\u786e \u5b9a", tc.Accent, "#000000");
            okBtn.MouseLeftButtonDown += (_, _) => Close();
            okBtn.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.Children.Add(okBtn);

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private System.Windows.Controls.Border CreateButton(string text, string bg, string fg)
        {
            return new System.Windows.Controls.Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(36, 7, 36, 7),
                Margin = new Thickness(4),
                Cursor = Cursors.Hand,
                Child = new System.Windows.Controls.TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
        }
    }
}
