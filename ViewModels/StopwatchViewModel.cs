using System;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class StopwatchViewModel : ViewModelBase
    {
        private readonly TimerService _timer;
        private int _elapsedSeconds;
        private bool _isRunning;

        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set => SetProperty(ref _elapsedSeconds, value);
        }

        public string TimeDisplay
        {
            get
            {
                int t = _elapsedSeconds;
                int h = t / 3600, m = (t % 3600) / 60, s = t % 60;
                return h > 0 ? $"{h:D2}:{m:D2}:{s:D2}" : $"{m:D2}:{s:D2}";
            }
        }

        public string StatusText => _isRunning ? "\u8ba1\u65f6\u4e2d" : (_elapsedSeconds > 0 ? "\u5df2\u6682\u505c" : "\u51c6\u5907\u5f00\u59cb");
        public bool IsRunning { get => _isRunning; private set { SetProperty(ref _isRunning, value); } }
        public double Progress => 1.0;
        public string RingColor => "#10b981";

        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand ResetCommand { get; }

        public StopwatchViewModel(TimerService timer)
        {
            _timer = timer;
            _timer.Tick += OnTick;

            StartCommand = new RelayCommand(Start, () => !_isRunning);
            PauseCommand = new RelayCommand(Pause, () => _isRunning);
            ResetCommand = new RelayCommand(Reset);
        }

        private void Start()
        {
            if (_isRunning) return;
            IsRunning = true;
            _timer.Start();
            RefreshDisplay();
        }

        private void Pause()
        {
            if (!_isRunning) return;
            IsRunning = false;
            _timer.Stop();
            RefreshDisplay();
        }

        private void Reset()
        {
            Pause();
            ElapsedSeconds = 0;
            RefreshDisplay();
        }

        private void OnTick()
        {
            if (!_isRunning) return;
            ElapsedSeconds++;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            RaisePropertyChanged(nameof(TimeDisplay));
            RaisePropertyChanged(nameof(StatusText));
            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
        }
    }
}
