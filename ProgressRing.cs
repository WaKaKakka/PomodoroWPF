using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PomodoroWPF
{
    /// <summary>
    /// 进度环 — 带发光效果的圆环进度指示器
    /// </summary>
    public class ProgressRing : ContentControl
    {
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(double), typeof(ProgressRing),
                new PropertyMetadata(360.0, OnPropertyChanged));

        public static readonly DependencyProperty RingWidthProperty =
            DependencyProperty.Register(nameof(RingWidth), typeof(double), typeof(ProgressRing),
                new PropertyMetadata(8.0, OnPropertyChanged));

        public static readonly DependencyProperty RingColorProperty =
            DependencyProperty.Register(nameof(RingColor), typeof(string), typeof(ProgressRing),
                new PropertyMetadata("#f59e0b", OnPropertyChanged));

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public double RingWidth
        {
            get => (double)GetValue(RingWidthProperty);
            set => SetValue(RingWidthProperty, value);
        }

        public string RingColor
        {
            get => (string)GetValue(RingColorProperty);
            set => SetValue(RingColorProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressRing ring)
                ring.Setup();
        }

        private Canvas? _canvas;
        private Path? _bgRing;
        private Path? _arcPath;
        private TextBlock? _mainText;
        private TextBlock? _subText;

        // 缓存 Set() 参数，确保加载前调用不丢失
        private double _lastFraction;
        private string? _lastText = "25:00";
        private string? _lastSub = "准备开始";
        private string? _lastColor;

        public ProgressRing()
        {
            Loaded += (_, _) => Setup();
        }

        private void Setup()
        {
            if (_canvas == null)
            {
                _canvas = new Canvas
                {
                    Background = Brushes.Transparent,
                    ClipToBounds = true,
                };

                _bgRing = new Path { StrokeThickness = RingWidth };
                _arcPath = new Path { StrokeThickness = RingWidth };
                _mainText = new TextBlock
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 56,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetBrush("#fafaf9"),
                };
                _subText = new TextBlock
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 14,
                    Foreground = GetBrush("#57534e"),
                };

                _canvas.Children.Add(_bgRing);
                _canvas.Children.Add(_arcPath);
                _canvas.Children.Add(_mainText);
                _canvas.Children.Add(_subText);

                Content = _canvas;
            }

            double s = Size;
            double rw = RingWidth;
            double cx = s / 2, cy = s / 2;
            double r = (s - rw) / 2;

            _canvas.Width = s;
            _canvas.Height = s;

            // 底色环（完整圆）
            _bgRing!.Stroke = GetBrush("#292524");
            _bgRing.Data = new EllipseGeometry(new Point(cx, cy), r, r);

            // 进度弧（初始为空）
            _arcPath!.Stroke = GetBrush(RingColor);
            _arcPath.Data = null;

            // 主文字
            _mainText!.Text = _lastText ?? "25:00";
            _mainText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(_mainText, cx - _mainText.DesiredSize.Width / 2);
            Canvas.SetTop(_mainText, cy - 18 - _mainText.DesiredSize.Height / 2);

            // 副文字
            _subText!.Text = _lastSub ?? "准备开始";
            _subText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(_subText, cx - _subText.DesiredSize.Width / 2);
            Canvas.SetTop(_subText, cy + 42 - _subText.DesiredSize.Height / 2);

            // 应用缓存的进度和颜色
            ApplyArc(_lastFraction, cx, cy, r);
            if (_lastColor != null)
                _arcPath.Stroke = GetBrush(_lastColor);
        }

        /// <summary>
        /// 更新进度环显示
        /// </summary>
        /// <param name="fraction">进度 0.0~1.0</param>
        /// <param name="text">主文字（如 "25:00"）</param>
        /// <param name="sub">副文字（如 "专注中"）</param>
        /// <param name="color">进度弧颜色（十六进制）</param>
        public void Set(double fraction, string? text = null, string? sub = null, string? color = null)
        {
            // 始终缓存参数
            _lastFraction = Math.Max(0, Math.Min(1, fraction));
            if (text != null) _lastText = text;
            if (sub != null) _lastSub = sub;
            if (color != null) _lastColor = color;

            if (_canvas == null || _arcPath == null || _mainText == null || _subText == null)
                return;

            double s = Size;
            double rw = RingWidth;
            double cx = s / 2, cy = s / 2;
            double r = (s - rw) / 2;

            // 更新进度弧
            ApplyArc(_lastFraction, cx, cy, r);

            // 更新颜色
            if (color != null)
                _arcPath.Stroke = GetBrush(color);

            // 更新主文字
            if (text != null)
            {
                _mainText.Text = text;
                _mainText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(_mainText, cx - _mainText.DesiredSize.Width / 2);
                Canvas.SetTop(_mainText, cy - 18 - _mainText.DesiredSize.Height / 2);
            }

            // 更新副文字
            if (sub != null)
            {
                _subText.Text = sub;
                _subText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(_subText, cx - _subText.DesiredSize.Width / 2);
                Canvas.SetTop(_subText, cy + 42 - _subText.DesiredSize.Height / 2);
            }
        }

        private void ApplyArc(double fraction, double cx, double cy, double r)
        {
            if (_arcPath == null) return;

            if (fraction >= 0.999)
            {
                _arcPath.Data = new EllipseGeometry(new Point(cx, cy), r, r);
            }
            else if (fraction > 0.001)
            {
                double startAngle = -Math.PI / 2;
                double endAngle = startAngle + fraction * 2 * Math.PI;

                double sx = cx + r * Math.Cos(startAngle);
                double sy = cy + r * Math.Sin(startAngle);
                double ex = cx + r * Math.Cos(endAngle);
                double ey = cy + r * Math.Sin(endAngle);

                var figure = new PathFigure
                {
                    StartPoint = new Point(sx, sy),
                    IsClosed = false,
                };
                figure.Segments.Add(new ArcSegment(
                    new Point(ex, ey),
                    new Size(r, r),
                    0,
                    fraction > 0.5,
                    SweepDirection.Clockwise,
                    true));

                var geo = new PathGeometry();
                geo.Figures.Add(figure);
                _arcPath.Data = geo;
            }
            else
            {
                _arcPath.Data = null;
            }
        }

        private static SolidColorBrush GetBrush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }
}
