using System;
using System.Runtime.InteropServices;

namespace PomodoroWPF.Services
{
    public enum AmbientSoundType
    {
        Rain,
        Wind,
        Cafe
    }

    public class AmbientSoundService : IDisposable
    {
        // WaveOut P/Invoke
        [DllImport("winmm.dll")]
        private static extern int waveOutOpen(out IntPtr phWaveOut, uint uDeviceID, ref WaveFormatEx pwfx,
            WaveOutProc? dwCallback, IntPtr dwInstance, uint fdwOpen);

        [DllImport("winmm.dll")]
        private static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr pwh, uint cbwh);

        [DllImport("winmm.dll")]
        private static extern int waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        private static extern int waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr pwh, uint cbwh);

        private delegate void WaveOutProc(IntPtr hwo, uint uMsg, IntPtr dwInstance, IntPtr hdr, IntPtr dwParam2);

        private const uint WAVE_MAPPER = unchecked((uint)-1);
        private const uint WOM_DONE = 0x3BD;
        private const int BufferSamples = 22050; // 1 second at 22050 Hz
        private const int SampleRate = 22050;

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveFormatEx
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveHdr
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
        }

        private IntPtr _waveOut;
        private readonly WaveHdr[] _headers = new WaveHdr[2];
        private readonly IntPtr[] _buffers = new IntPtr[2];
        private readonly Random _random = new();
        private bool _isPlaying;
        private bool _disposed;
        private string _currentType = "rain";
        private double _volume = 0.3;
        private double _fadeMultiplier = 1.0;
        private bool _fadingOut;
        private int _totalSamplesGenerated;

        // Brown noise state for cafe
        private double _brownLast;

        public bool IsPlaying => _isPlaying;

        public void Start(string type)
        {
            if (_isPlaying) Stop();

            _currentType = type;
            _fadingOut = false;
            _fadeMultiplier = 0.0;
            _totalSamplesGenerated = 0;
            _brownLast = 0;

            var fmt = new WaveFormatEx
            {
                wFormatTag = 1, // PCM
                nChannels = 1,
                nSamplesPerSec = SampleRate,
                wBitsPerSample = 16,
                nBlockAlign = 2,
                nAvgBytesPerSec = (uint)(SampleRate * 2),
                cbSize = 0,
            };

            int result = waveOutOpen(out _waveOut, WAVE_MAPPER, ref fmt, OnWaveOutDone, IntPtr.Zero, 0x30000 /* CALLBACK_FUNCTION */);
            if (result != 0) return;

            for (int i = 0; i < 2; i++)
            {
                _buffers[i] = Marshal.AllocHGlobal(BufferSamples * 2);
                FillBuffer(i);

                _headers[i] = new WaveHdr
                {
                    lpData = _buffers[i],
                    dwBufferLength = (uint)(BufferSamples * 2),
                    dwFlags = 0,
                };

                waveOutWrite(_waveOut, ref _headers[i], (uint)Marshal.SizeOf<WaveHdr>());
            }

            _isPlaying = true;
        }

        public void Stop()
        {
            if (!_isPlaying) return;
            _isPlaying = false;

            try
            {
                waveOutReset(_waveOut);
                for (int i = 0; i < 2; i++)
                {
                    waveOutUnprepareHeader(_waveOut, ref _headers[i], (uint)Marshal.SizeOf<WaveHdr>());
                }
                waveOutClose(_waveOut);
            }
            catch { }

            _waveOut = IntPtr.Zero;
        }

        private void OnWaveOutDone(IntPtr hwo, uint uMsg, IntPtr dwInstance, IntPtr hdr, IntPtr dwParam2)
        {
            if (uMsg != WOM_DONE || !_isPlaying) return;

            if (_fadingOut && _fadeMultiplier <= 0.01)
            {
                // Fade complete, stop
                _isPlaying = false;
                return;
            }

            // Find which buffer just finished and refill it
            for (int i = 0; i < 2; i++)
            {
                if (_headers[i].lpData == Marshal.ReadIntPtr(hdr))
                {
                    FillBuffer(i);
                    waveOutWrite(_waveOut, ref _headers[i], (uint)Marshal.SizeOf<WaveHdr>());
                    break;
                }
            }
        }

        private void FillBuffer(int index)
        {
            unsafe
            {
                short* ptr = (short*)_buffers[index].ToPointer();
                for (int i = 0; i < BufferSamples; i++)
                {
                    double sample = GenerateSample();

                    // Fade in/out
                    if (!_fadingOut && _fadeMultiplier < 1.0)
                        _fadeMultiplier = Math.Min(1.0, _fadeMultiplier + 0.001);
                    else if (_fadingOut)
                        _fadeMultiplier = Math.Max(0.0, _fadeMultiplier - 0.002);

                    sample *= _volume * _fadeMultiplier;
                    ptr[i] = (short)(Math.Max(-1, Math.Min(1, sample)) * 32767);
                    _totalSamplesGenerated++;
                }
            }
        }

        private double GenerateSample()
        {
            double t = (double)_totalSamplesGenerated / SampleRate;

            return _currentType switch
            {
                "rain" => GenerateRain(t),
                "wind" => GenerateWind(t),
                "cafe" => GenerateCafe(t),
                _ => GenerateRain(t),
            };
        }

        private double GenerateRain(double t)
        {
            // White noise with slow amplitude modulation
            double noise = _random.NextDouble() * 2 - 1;
            double envelope = 0.5 + 0.5 * Math.Sin(t * 0.3) * Math.Sin(t * 0.17);
            return noise * envelope * 0.15;
        }

        private double GenerateWind(double t)
        {
            // Filtered noise with slow sinusoidal modulation
            double noise = _random.NextDouble() * 2 - 1;
            // Simple low-pass by averaging with previous
            double filtered = noise * 0.1 + _brownLast * 0.9;
            _brownLast = filtered;
            double envelope = 0.3 + 0.7 * (0.5 + 0.5 * Math.Sin(t * 0.08));
            return filtered * envelope * 0.2;
        }

        private double GenerateCafe(double t)
        {
            // Brown noise (integrated white noise)
            double white = _random.NextDouble() * 2 - 1;
            _brownLast += white * 0.02;
            _brownLast *= 0.998; // Prevent drift

            // Occasional high-frequency clicks
            double click = 0;
            if (_random.NextDouble() < 0.001)
                click = (_random.NextDouble() * 2 - 1) * 0.3;

            return (_brownLast * 0.15 + click) * 0.5;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            for (int i = 0; i < 2; i++)
            {
                if (_buffers[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(_buffers[i]);
            }
        }
    }
}
