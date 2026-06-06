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
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000);
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
