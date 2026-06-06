using System;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class CountdownViewModel : ViewModelBase
    {
        private readonly TimerService _timer;
        private readonly SoundManager? _sound;
        private readonly AppSettings _settings;

        private int _remainingSeconds;
        private int _totalSeconds;
        private bool _isRunning;
        private bool _isBreakMode;
        private bool _isLongBreak;
        private int _workSessionSeconds;
        private int _inputMinutes;

        public event Action<int>? PomodoroCompleted;
        public event Action? BreakCompleted;

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            set => SetProperty(ref _remainingSeconds, value);
        }

        public int TotalSeconds
        {
            get => _totalSeconds;
            set => SetProperty(ref _totalSeconds, value);
        }

        public string TimeDisplay
        {
            get
            {
                int m = _remainingSeconds / 60;
                int s = _remainingSeconds % 60;
                return $"{m:D2}:{s:D2}";
            }
        }

        public string StatusText
        {
            get
            {
                if (_isBreakMode)
                    return _isRunning ? "\u4f11\u606f\u4e2d" : "\u4f11\u606f\u6682\u505c";
                return _isRunning ? "\u4e13\u6ce8\u4e2d" : (_remainingSeconds < _totalSeconds ? "\u5df2\u6682\u505c" : "\u51c6\u5907\u5f00\u59cb");
            }
        }

        public string ModeLabel
        {
            get
            {
                if (!_isBreakMode) return "";
                return _isLongBreak
                    ? $"\u25c6 \u957f\u4f11\u606f ({_settings.LongBreakDurationMinutes}\u5206\u949f)"
                    : $"\u2615 \u77ed\u4f11\u606f ({_settings.BreakDurationMinutes}\u5206\u949f)";
            }
        }

        public bool IsBreakMode => _isBreakMode;
        public bool IsLongBreak => _isLongBreak;
        public bool IsRunning { get => _isRunning; private set { SetProperty(ref _isRunning, value); } }
        public double Progress => _totalSeconds > 0 ? (double)_remainingSeconds / _totalSeconds : 0;
        public string RingColor => _isBreakMode ? "#10b981" : "#f59e0b";

        public int InputMinutes
        {
            get => _inputMinutes;
            set => SetProperty(ref _inputMinutes, value);
        }

        public string CycleDots
        {
            get
            {
                int pos = _settings.CyclePosition % _settings.PomodorosBeforeLongBreak;
                var dots = new char[_settings.PomodorosBeforeLongBreak * 2 - 1];
                for (int i = 0; i < _settings.PomodorosBeforeLongBreak; i++)
                {
                    dots[i * 2] = i < pos ? '\u25cf' : '\u25cb';
                    if (i * 2 + 1 < dots.Length) dots[i * 2 + 1] = ' ';
                }
                return new string(dots);
            }
        }

        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand ApplyMinutesCommand { get; }

        public CountdownViewModel(TimerService timer, SoundManager? sound, AppSettings settings)
        {
            _timer = timer;
            _sound = sound;
            _settings = settings;

            _inputMinutes = settings.WorkDurationMinutes;
            _remainingSeconds = _inputMinutes * 60;
            _totalSeconds = _inputMinutes * 60;

            _timer.Tick += OnTick;

            StartCommand = new RelayCommand(Start, () => !_isRunning && _remainingSeconds > 0);
            PauseCommand = new RelayCommand(Pause, () => _isRunning);
            ResetCommand = new RelayCommand(Reset);
            ApplyMinutesCommand = new RelayCommand(ApplyMinutes);
        }

        public void Start()
        {
            if (_isRunning || _remainingSeconds <= 0) return;
            IsRunning = true;
            _timer.Start();
            RefreshDisplay();
        }

        public void Pause()
        {
            if (!_isRunning) return;
            IsRunning = false;
            _timer.Stop();
            RefreshDisplay();
        }

        public void Reset()
        {
            Pause();
            if (_isBreakMode)
            {
                _isBreakMode = false;
                _isLongBreak = false;
                RaisePropertyChanged(nameof(IsBreakMode));
                RaisePropertyChanged(nameof(IsLongBreak));
            }
            _inputMinutes = _settings.WorkDurationMinutes;
            _remainingSeconds = _inputMinutes * 60;
            _totalSeconds = _inputMinutes * 60;
            _workSessionSeconds = 0;
            RefreshDisplay();
        }

        private void ApplyMinutes()
        {
            if (_inputMinutes <= 0) return;
            _timer.Stop();
            IsRunning = false;
            _remainingSeconds = _inputMinutes * 60;
            _totalSeconds = _inputMinutes * 60;
            _workSessionSeconds = 0;
            RefreshDisplay();
        }

        private void OnTick()
        {
            if (!_isRunning) return;

            _remainingSeconds--;
            if (!_isBreakMode)
                _workSessionSeconds++;

            if (_settings.TickSoundEnabled && _remainingSeconds <= 10 && _remainingSeconds > 0)
                _sound?.PlayTick();

            RefreshDisplay();

            if (_remainingSeconds <= 0)
            {
                IsRunning = false;
                _timer.Stop();
                OnTimerDone();
            }
        }

        private void OnTimerDone()
        {
            _sound?.PlayChime();

            if (!_isBreakMode)
            {
                // Work session completed
                PomodoroCompleted?.Invoke(_workSessionSeconds);

                _settings.CyclePosition++;

                if (_settings.AutoBreak)
                {
                    bool isLong = _settings.CyclePosition >= _settings.PomodorosBeforeLongBreak;
                    if (isLong)
                        _settings.CyclePosition = 0;

                    _isBreakMode = true;
                    _isLongBreak = isLong;
                    int breakMin = isLong ? _settings.LongBreakDurationMinutes : _settings.BreakDurationMinutes;
                    _remainingSeconds = breakMin * 60;
                    _totalSeconds = breakMin * 60;
                    _workSessionSeconds = 0;
                    _inputMinutes = breakMin;

                    RaisePropertyChanged(nameof(IsBreakMode));
                    RaisePropertyChanged(nameof(IsLongBreak));
                    RaisePropertyChanged(nameof(ModeLabel));
                    RaisePropertyChanged(nameof(CycleDots));

                    IsRunning = true;
                    _timer.Start();
                    RefreshDisplay();
                }
                else
                {
                    RefreshDisplay();
                }
            }
            else
            {
                // Break completed
                _isBreakMode = false;
                _isLongBreak = false;
                _inputMinutes = _settings.WorkDurationMinutes;
                _remainingSeconds = _inputMinutes * 60;
                _totalSeconds = _inputMinutes * 60;

                RaisePropertyChanged(nameof(IsBreakMode));
                RaisePropertyChanged(nameof(IsLongBreak));
                RaisePropertyChanged(nameof(ModeLabel));
                RefreshDisplay();
                BreakCompleted?.Invoke();
            }
        }

        public void RefreshCycleDots()
        {
            RaisePropertyChanged(nameof(CycleDots));
        }

        private void RefreshDisplay()
        {
            RaisePropertyChanged(nameof(TimeDisplay));
            RaisePropertyChanged(nameof(StatusText));
            RaisePropertyChanged(nameof(ModeLabel));
            RaisePropertyChanged(nameof(Progress));
            RaisePropertyChanged(nameof(RingColor));
            RaisePropertyChanged(nameof(CycleDots));
            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
        }
    }
}
