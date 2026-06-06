using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PomodoroWPF.Controls
{
    public class HeatmapCellData
    {
        public string Date { get; set; } = "";
        public int Count { get; set; }
        public int IntensityLevel { get; set; }
    }

    public class HeatmapControl : ContentControl
    {
        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(HeatmapControl),
                new PropertyMetadata(14.0, OnPropertyChanged));

        public static readonly DependencyProperty CellSpacingProperty =
            DependencyProperty.Register(nameof(CellSpacing), typeof(double), typeof(HeatmapControl),
                new PropertyMetadata(2.0, OnPropertyChanged));

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register(nameof(AccentColor), typeof(string), typeof(HeatmapControl),
                new PropertyMetadata("#f59e0b", OnPropertyChanged));

        public static readonly DependencyProperty MutedColorProperty =
            DependencyProperty.Register(nameof(MutedColor), typeof(string), typeof(HeatmapControl),
                new PropertyMetadata("#292524", OnPropertyChanged));

        public double CellSize { get => (double)GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }
        public double CellSpacing { get => (double)GetValue(CellSpacingProperty); set => SetValue(CellSpacingProperty, value); }
        public string AccentColor { get => (string)GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
        public string MutedColor { get => (string)GetValue(MutedColorProperty); set => SetValue(MutedColorProperty, value); }

        private List<HeatmapCellData>? _cells;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HeatmapControl hc) hc.Render();
        }

        public HeatmapControl()
        {
            Loaded += (_, _) => Render();
        }

        public void SetCells(List<HeatmapCellData> cells)
        {
            _cells = cells;
            Render();
        }

        private void Render()
        {
            if (_cells == null || _cells.Count == 0) return;

            var canvas = new Canvas { Background = Brushes.Transparent };

            // Organize cells into weeks (7 rows)
            var sorted = new List<HeatmapCellData>(_cells);
            sorted.Sort((a, b) => string.Compare(a.Date, b.Date, StringComparison.Ordinal));

            // Parse accent color for intensity levels
            var accent = (Color)ColorConverter.ConvertFromString(AccentColor);
            var muted = (Color)ColorConverter.ConvertFromString(MutedColor);

            double cs = CellSize;
            double sp = CellSpacing;
            double cellTotal = cs + sp;

            // Month labels
            string? lastMonth = null;
            int weekIndex = 0;

            foreach (var cell in sorted)
            {
                if (!DateTime.TryParse(cell.Date, out var date)) continue;

                int dayOfWeek = ((int)date.DayOfWeek + 6) % 7; // Monday=0, Sunday=6
                int col = weekIndex;

                // Detect new week
                if (dayOfWeek == 0 && col > 0)
                    weekIndex++;
                else if (dayOfWeek == 0 && col == 0)
                    weekIndex = 0;
                else if (sorted.IndexOf(cell) == 0)
                    weekIndex = 0;

                col = weekIndex;

                double x = col * cellTotal;
                double y = dayOfWeek * cellTotal + 20; // 20px for month labels

                // Determine color
                Color cellColor;
                if (cell.IntensityLevel == 0)
                    cellColor = muted;
                else
                {
                    double alpha = 0.2 + cell.IntensityLevel * 0.2;
                    cellColor = Color.FromArgb(
                        (byte)(alpha * 255),
                        accent.R, accent.G, accent.B);
                }

                var rect = new Rectangle
                {
                    Width = cs,
                    Height = cs,
                    Fill = new SolidColorBrush(cellColor),
                    RadiusX = 2,
                    RadiusY = 2,
                    ToolTip = $"{cell.Date}: {cell.Count} \u4e2a\u756a\u8304",
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvas.Children.Add(rect);

                // Month label
                string month = date.ToString("yyyy-MM");
                if (month != lastMonth && dayOfWeek == 0)
                {
                    var label = new TextBlock
                    {
                        Text = date.ToString("M\u6708"),
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 9,
                        Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#57534e")),
                    };
                    Canvas.SetLeft(label, x);
                    Canvas.SetTop(label, 0);
                    canvas.Children.Add(label);
                    lastMonth = month;
                }

                // Track week changes
                if (dayOfWeek == 6)
                    weekIndex++;
            }

            double totalWidth = (weekIndex + 1) * cellTotal;
            double totalHeight = 7 * cellTotal + 20;
            canvas.Width = totalWidth;
            canvas.Height = totalHeight;

            Content = canvas;
        }
    }
}
