using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace PomodoroWPF
{
    /// <summary>
    /// 音效管理器 — 生成并播放滴答声和完成音
    /// </summary>
    public class SoundManager : IDisposable
    {
        private readonly string _tickFile;
        private readonly string _chimeFile;
        private readonly SoundPlayer _tickPlayer;
        private readonly SoundPlayer _chimePlayer;

        public SoundManager()
        {
            string exeDir = Path.GetDirectoryName(Environment.ProcessPath)
                ?? AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(exeDir, "sounds");
            Directory.CreateDirectory(dir);

            _tickFile = Path.Combine(dir, "tick.wav");
            _chimeFile = Path.Combine(dir, "chime.wav");

            if (!File.Exists(_tickFile))
                GenerateTickWav(_tickFile);
            if (!File.Exists(_chimeFile))
                GenerateChimeWav(_chimeFile);

            _tickPlayer = new SoundPlayer(_tickFile);
            _tickPlayer.Load();

            _chimePlayer = new SoundPlayer(_chimeFile);
            _chimePlayer.Load();
        }

        /// <summary>
        /// 播放滴答声（非阻塞）
        /// </summary>
        public void PlayTick()
        {
            Task.Run(() =>
            {
                try
                {
                    using var player = new SoundPlayer(_tickFile);
                    player.PlaySync();
                }
                catch { }
            });
        }

        /// <summary>
        /// 播放完成提示音
        /// </summary>
        public void PlayChime()
        {
            try { _chimePlayer.Play(); } catch { }
            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
        }

        /// <summary>
        /// 生成短促的滴答声 WAV（~80ms 的衰减正弦波）
        /// </summary>
        private static void GenerateTickWav(string path)
        {
            int sampleRate = 22050;
            double duration = 0.08;
            double frequency = 1000;
            int numSamples = (int)(sampleRate * duration);

            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs);

            // WAV header
            int dataSize = numSamples * 2;
            bw.Write("RIFF"u8); bw.Write(36 + dataSize);
            bw.Write("WAVE"u8);
            bw.Write("fmt "u8); bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sampleRate); bw.Write(sampleRate * 2);
            bw.Write((short)2); bw.Write((short)16);
            bw.Write("data"u8); bw.Write(dataSize);

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double envelope = Math.Exp(-t * 40); // 快速衰减
                double sample = Math.Sin(2 * Math.PI * frequency * t) * envelope * 0.3;
                bw.Write((short)(sample * 32767));
            }
        }

        /// <summary>
        /// 生成悦耳的完成提示音 WAV（~500ms 双音）
        /// </summary>
        private static void GenerateChimeWav(string path)
        {
            int sampleRate = 22050;
            double duration = 0.6;
            int numSamples = (int)(sampleRate * duration);

            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs);

            int dataSize = numSamples * 2;
            bw.Write("RIFF"u8); bw.Write(36 + dataSize);
            bw.Write("WAVE"u8);
            bw.Write("fmt "u8); bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sampleRate); bw.Write(sampleRate * 2);
            bw.Write((short)2); bw.Write((short)16);
            bw.Write("data"u8); bw.Write(dataSize);

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double envelope = Math.Exp(-t * 3);

                // 两个频率叠加产生悦耳的和弦
                double s1 = Math.Sin(2 * Math.PI * 523 * t); // C5
                double s2 = Math.Sin(2 * Math.PI * 659 * t); // E5
                double sample = (s1 + s2) * 0.5 * envelope * 0.4;

                bw.Write((short)(sample * 32767));
            }
        }

        public void Dispose()
        {
            _tickPlayer.Dispose();
            _chimePlayer.Dispose();
        }
    }
}
