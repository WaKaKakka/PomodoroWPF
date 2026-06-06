using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace PomodoroWPF
{
    /// <summary>
    /// 系统托盘管理器 — 最小化到托盘、双击恢复
    /// </summary>
    public class TrayManager : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private readonly Window _window;

        public TrayManager(Window window)
        {
            _window = window;
            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "番茄钟 — 点击显示",
                Visible = false,
                Icon = CreateTrayIcon(),
            };

            // 双击托盘图标 → 恢复窗口
            _notifyIcon.DoubleClick += (_, _) => ShowWindow();

            // 右键菜单
            var menu = new ContextMenuStrip();
            menu.Items.Add("显示窗口", null, (_, _) => ShowWindow());
            menu.Items.Add("开始专注", null, (_, _) =>
            {
                ShowWindow();
                // 通过窗口查找方式触发倒计时（可选）
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("退出", null, (_, _) =>
            {
                _notifyIcon.Visible = false;
                _window.Close();
            });
            _notifyIcon.ContextMenuStrip = menu;

            // 窗口最小化时自动隐藏到托盘
            _window.StateChanged += OnWindowStateChanged;
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                HideToTray();
            }
        }

        /// <summary>
        /// 隐藏窗口并显示托盘图标
        /// </summary>
        public void HideToTray()
        {
            _window.Hide();
            _window.WindowState = WindowState.Minimized;
            if (_notifyIcon != null)
                _notifyIcon.Visible = true;
        }

        /// <summary>
        /// 从托盘恢复窗口
        /// </summary>
        public void ShowWindow()
        {
            _window.Show();
            _window.WindowState = WindowState.Maximized;
            _window.Activate();
            _window.Focus();
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
        }

        /// <summary>
        /// 显示托盘气泡通知
        /// </summary>
        public void ShowNotification(string title, string message)
        {
            if (_notifyIcon == null) return;

            // 需要先让图标可见才能发通知
            _notifyIcon.Visible = true;
            _notifyIcon.BalloonTipTitle = title;
            // 过滤 Emoji 字符，因为托盘通知不支持彩色 Emoji 显示
            _notifyIcon.BalloonTipText = RemoveEmoji(message);
            _notifyIcon.ShowBalloonTip(3000);
        }

        /// <summary>
        /// 移除字符串中的 Emoji 字符（托盘通知兼容性）
        /// </summary>
        private static string RemoveEmoji(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var result = new System.Text.StringBuilder();
            foreach (char c in text)
            {
                // 保留基本多语言平面字符，过滤补充平面字符（Emoji 所在区域）
                if (!char.IsHighSurrogate(c) && !char.IsLowSurrogate(c))
                {
                    // 保留常规字符，但过滤常见 Emoji 范围
                    if (c < 0x2000 || c > 0x32FF)
                        result.Append(c);
                }
            }
            
            var cleaned = result.ToString().Trim();
            // 如果过滤后为空，返回原文
            return string.IsNullOrEmpty(cleaned) ? text : cleaned;
        }

        /// <summary>
        /// 生成一个简单的红色圆形托盘图标
        /// </summary>
        private static Icon CreateTrayIcon()
        {
            using var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using var brush = new SolidBrush(Color.FromArgb(220, 50, 47));
                g.FillEllipse(brush, 1, 1, 14, 14);
                // 画一个小叶子
                using var leaf = new SolidBrush(Color.FromArgb(34, 197, 94));
                g.FillEllipse(leaf, 6, 0, 5, 4);
            }

            return Icon.FromHandle(bmp.GetHicon());
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}
