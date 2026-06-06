using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PomodoroWPF
{
    /// <summary>
    /// 圆角按钮 — 极简精致，支持主题感知
    /// </summary>
    public class RoundButton : Border
    {
        /// <summary>
        /// 全局主题感知按钮列表 — 主题切换时自动更新
        /// </summary>
        private static readonly List<WeakReference<RoundButton>> _themeAwareButtons = new();

        static RoundButton()
        {
            ThemeManager.ThemeChanged += OnGlobalThemeChanged;
        }

        private static void OnGlobalThemeChanged(string themeId)
        {
            var tc = ThemeManager.GetCurrent(themeId);
            // Clean up dead references while updating
            for (int i = _themeAwareButtons.Count - 1; i >= 0; i--)
            {
                if (_themeAwareButtons[i].TryGetTarget(out var btn))
                    btn.ApplyThemeRole(tc);
                else
                    _themeAwareButtons.RemoveAt(i);
            }
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(RoundButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RoundButton),
                new PropertyMetadata("", OnTextOrStyleChanged));

        public static readonly DependencyProperty BgColorProperty =
            DependencyProperty.Register("BgColor", typeof(string), typeof(RoundButton),
                new PropertyMetadata("#0a0a0a", OnTextOrStyleChanged));

        public static readonly DependencyProperty FgColorProperty =
            DependencyProperty.Register("FgColor", typeof(string), typeof(RoundButton),
                new PropertyMetadata("#fafaf9", OnTextOrStyleChanged));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(RoundButton),
                new PropertyMetadata(14.0, OnTextOrStyleChanged));

        public static readonly DependencyProperty BtnWidthProperty =
            DependencyProperty.Register("BtnWidth", typeof(double), typeof(RoundButton),
                new PropertyMetadata(160.0, OnTextOrStyleChanged));

        public static readonly DependencyProperty BtnHeightProperty =
            DependencyProperty.Register("BtnHeight", typeof(double), typeof(RoundButton),
                new PropertyMetadata(50.0, OnTextOrStyleChanged));

        public static readonly DependencyProperty ThemeRoleProperty =
            DependencyProperty.Register("ThemeRole", typeof(string), typeof(RoundButton),
                new PropertyMetadata("None", OnThemeRoleChanged));

        private static void OnThemeRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RoundButton btn && (string)e.NewValue != "None")
            {
                _themeAwareButtons.Add(new WeakReference<RoundButton>(btn));
                // Apply current theme immediately
                var tc = ThemeManager.GetCurrent(App.CurrentSettings.Theme);
                btn.ApplyThemeRole(tc);
            }
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string BgColor
        {
            get => (string)GetValue(BgColorProperty);
            set => SetValue(BgColorProperty, value);
        }

        public string FgColor
        {
            get => (string)GetValue(FgColorProperty);
            set => SetValue(FgColorProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public double BtnWidth
        {
            get => (double)GetValue(BtnWidthProperty);
            set => SetValue(BtnWidthProperty, value);
        }

        public double BtnHeight
        {
            get => (double)GetValue(BtnHeightProperty);
            set => SetValue(BtnHeightProperty, value);
        }

        /// <summary>
        /// 主题角色 — 设为 Card/Accent/Muted/Success 后按钮颜色随主题自动切换。
        /// 默认 "None" 表示使用显式 BgColor/FgColor。
        /// </summary>
        public string ThemeRole
        {
            get => (string)GetValue(ThemeRoleProperty);
            set => SetValue(ThemeRoleProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(RoundButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(RoundButton),
                new PropertyMetadata(null));

        private TextBlock? _label;
        private Brush? _normalBg;
        private Brush? _hoverBg;

        public RoundButton()
        {
            Cursor = Cursors.Hand;
            MouseLeftButtonDown += OnClick;
            MouseEnter += OnHoverEnter;
            MouseLeave += OnHoverLeave;
            Loaded += (_, _) => ApplyStyle();
        }

        private static void OnTextOrStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RoundButton btn && btn.IsLoaded)
                btn.ApplyStyle();
        }

        private void ApplyThemeRole(ThemeManager.ThemeColors tc)
        {
            switch (ThemeRole)
            {
                case "Card":
                    BgColor = tc.Card;
                    FgColor = tc.TextDim;
                    break;
                case "Accent":
                    BgColor = tc.Accent;
                    FgColor = "#000000";
                    break;
                case "Muted":
                    BgColor = tc.TextMuted;
                    FgColor = tc.TextDim;
                    break;
                case "Success":
                    BgColor = tc.Success;
                    FgColor = "#000000";
                    break;
            }
        }

        private void ApplyStyle()
        {
            var bgColor = (Color)ColorConverter.ConvertFromString(BgColor);
            _normalBg = new SolidColorBrush(bgColor);
            _hoverBg = new SolidColorBrush(Lighten(bgColor, 25));

            Background = _normalBg;
            CornerRadius = new CornerRadius(8);
            Width = BtnWidth;
            Height = BtnHeight;

            if (_label == null)
            {
                _label = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Microsoft YaHei"),
                    FontWeight = FontWeights.Bold,
                };
                Child = _label;
            }

            _label.Text = Text;
            _label.FontSize = FontSize;
            _label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(FgColor));
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }

        private void OnHoverEnter(object sender, MouseEventArgs e)
        {
            if (_hoverBg != null) Background = _hoverBg;
        }

        private void OnHoverLeave(object sender, MouseEventArgs e)
        {
            if (_normalBg != null) Background = _normalBg;
        }

        private static Color Lighten(Color c, int amount)
        {
            return Color.FromRgb(
                (byte)Math.Min(255, c.R + amount),
                (byte)Math.Min(255, c.G + amount),
                (byte)Math.Min(255, c.B + amount));
        }
    }
}
