using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using PomodoroWPF.Services;
using PomodoroWPF.ViewModels;

namespace PomodoroWPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _vm;
        private HotkeyService? _hotkey;
        private bool _isFullscreen = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Initialize(MainViewModel vm, HotkeyService? hotkey)
        {
            _vm = vm;
            _hotkey = hotkey;
            DataContext = vm;
            vm.MainWindow = this;

            // Bridge ProgressRing updates from ViewModel
            vm.Countdown.PropertyChanged += OnCountdownPropertyChanged;
            vm.Stopwatch.PropertyChanged += OnStopwatchPropertyChanged;

            // Initial ring display
            CdRing.Set(1.0, text: $"{vm.Countdown.InputMinutes:D2}:00", sub: "\u51c6\u5907\u5f00\u59cb", color: "#f59e0b");
            SwRing.Set(1.0, text: "00:00", sub: "\u51c6\u5907\u5f00\u59cb", color: "#10b981");

            // Wire fullscreen toggle
            vm.ToggleFullscreenCommand = new Infrastructure.RelayCommand(ToggleFullscreen);

            // Wire quit
            vm.QuitRequested += OnQuit;
        }

        private void OnCountdownPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm == null) return;
            var cd = _vm.Countdown;
            if (e.PropertyName is nameof(CountdownViewModel.Progress) or
                nameof(CountdownViewModel.TimeDisplay) or
                nameof(CountdownViewModel.StatusText) or
                nameof(CountdownViewModel.RingColor))
            {
                CdRing.Set(cd.Progress, text: cd.TimeDisplay, sub: cd.StatusText, color: cd.RingColor);
            }
        }

        private void OnStopwatchPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm == null) return;
            var sw = _vm.Stopwatch;
            if (e.PropertyName is nameof(StopwatchViewModel.TimeDisplay) or
                nameof(StopwatchViewModel.StatusText))
            {
                SwRing.Set(1.0, text: sw.TimeDisplay, sub: sw.StatusText, color: sw.RingColor);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _hotkey?.Register(new WindowInteropHelper(this).Handle);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _vm?.SaveAll();
            _hotkey?.Dispose();
        }

        private void OnQuit()
        {
            _hotkey?.Dispose();
            _hotkey = null;
            Close();
        }

        private void ToggleFullscreen()
        {
            _isFullscreen = !_isFullscreen;
            if (_isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.NoResize;
                Topmost = false;
                Width = 1000;
                Height = 700;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
            }
        }
    }
}
