using System;
using System.Windows.Threading;

namespace PomodoroWPF.Services
{
    public class TimerService : IDisposable
    {
        private readonly DispatcherTimer _timer;

        public event Action? Tick;

        public bool IsRunning { get; private set; }

        public TimerService(TimeSpan interval)
        {
            _timer = new DispatcherTimer { Interval = interval };
            _timer.Tick += (_, _) => Tick?.Invoke();
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            _timer.Stop();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
