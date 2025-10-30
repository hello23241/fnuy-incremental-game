using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WinFormsApp1
{
    /// <summary>
    /// NAudio-based audio manager with a single mixer to allow overlapping playback.
    /// Sounds are registered from embedded resources or file paths (loaded into memory),
    /// and played by mixing into a shared WaveOutEvent.
    /// </summary>
    public static class AudioManagerNAudio
    {
        // Map key -> in-memory audio bytes
        private static readonly ConcurrentDictionary<string, byte[]> _memorySounds = new();

        // Per-sfx combined gates enforcing both windowed count and min spacing
        private static readonly ConcurrentDictionary<string, SoundPlayGate> _gates = new();
        private const int MaxPlaysPerWindow = 2; // (1) allow at most 2 plays per window
        private static readonly TimeSpan RateWindow = TimeSpan.FromMilliseconds(200); // 0.2s window
        private static readonly TimeSpan MinSpacing = TimeSpan.FromMilliseconds(50);  // (2) require >= 0.05s between plays

        // Shared output & mixer
        private static readonly object _initLock = new();
        private static IWavePlayer _output; // WaveOutEvent
        private static MixingSampleProvider _mixer; // 32-bit float, 44.1kHz, stereo
        private static WaveFormat _mixerFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        private static void EnsureInitialized()
        {
            if (_output != null) return;
            lock (_initLock)
            {
                if (_output != null) return;
                _mixer = new MixingSampleProvider(_mixerFormat) { ReadFully = true };
                // Lower latency for more responsive SFX
                var waveOut = new WaveOutEvent
                {
                    DesiredLatency = 50, // ms
                    NumberOfBuffers = 2
                };
                waveOut.Init(_mixer);
                waveOut.Play();
                _output = waveOut;
            }
        }

        /// <summary>
        /// Register a sound file path (contents are loaded into memory to avoid file locks).
        /// </summary>
        public static void Register(string key, string filePath)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(filePath))
                return;
            try
            {
                if (File.Exists(filePath))
                {
                    var bytes = File.ReadAllBytes(filePath);
                    _memorySounds[key] = bytes;
                }
            }
            catch { }
        }

        /// <summary>
        /// Register in-memory audio bytes for a key.
        /// </summary>
        public static void RegisterFromBytes(string key, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null || data.Length == 0)
                return;
            _memorySounds[key] = data;
        }

        /// <summary>
        /// Register an embedded resource by resource name. If resourceName is null, attempts common patterns
        /// for the given key with extensions .wav, .mp3, .aif, .aiff.
        /// </summary>
        public static void RegisterFromEmbeddedResource(string key, string resourceName = null)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string found = null;
                var names = asm.GetManifestResourceNames();
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    found = Array.Find(names, r => r.Equals(resourceName, StringComparison.OrdinalIgnoreCase) || r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
                }

                if (found == null)
                {
                    // Try by key with common extensions and with/without Resources. prefix
                    string[] exts = new[] { ".wav", ".mp3", ".aif", ".aiff" };
                    foreach (var ext in exts)
                    {
                        found = Array.Find(names, r => r.EndsWith($"{key}{ext}", StringComparison.OrdinalIgnoreCase) || r.EndsWith($"Resources.{key}{ext}", StringComparison.OrdinalIgnoreCase));
                        if (found != null) break;
                    }
                }
                if (found == null) return;

                using var stream = asm.GetManifestResourceStream(found);
                if (stream == null) return;
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                RegisterFromBytes(key, ms.ToArray());
            }
            catch { }
        }

        /// <summary>
        /// Play a registered sound with volume 0..1. Uses a single shared mixer so multiple calls overlap.
        /// Rate-limited: max 2 plays per 200ms and min 50ms spacing between plays per key.
        /// Min-spacing is checked first; if it fails, the window quota is not consumed.
        /// Supports WAV/MP3/AIFF input formats.
        /// </summary>
        public static void Play(string key, float volume = 1.0f)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (!_memorySounds.TryGetValue(key, out var data))
                return;

            // Attempt to acquire permission to play for this key
            var gate = _gates.GetOrAdd(key, _ => new SoundPlayGate(MaxPlaysPerWindow, RateWindow, MinSpacing));
            if (!gate.TryAcquire())
                return; // discard this trigger per rules

            try
            {
                EnsureInitialized();

                // Create a reader over the in-memory data (support WAV/MP3/AIFF)
                if (!TryCreateReaderFromBytes(data, out var reader))
                    return;

                ISampleProvider provider = reader.ToSampleProvider();

                // Convert mono -> stereo if needed
                if (provider.WaveFormat.Channels == 1 && _mixerFormat.Channels == 2)
                {
                    provider = new MonoToStereoSampleProvider(provider);
                }

                // Resample if needed
                if (provider.WaveFormat.SampleRate != _mixerFormat.SampleRate)
                {
                    provider = new WdlResamplingSampleProvider(provider, _mixerFormat.SampleRate);
                }

                // Apply per-play volume
                var vol = new VolumeSampleProvider(provider) { Volume = Math.Clamp(volume, 0f, 1f) };

                // Wrap to dispose underlying readers when finished
                var disposingProvider = new AutoDisposeSampleProvider(vol, reader);

                _mixer.AddMixerInput(disposingProvider);
            }
            catch
            {
                // ignore playback errors
            }
        }

        private static bool TryCreateReaderFromBytes(byte[] data, out WaveStream reader)
        {
            reader = null;
            try
            {
                var ms = new MemoryStream(data, writable: false);
                // Try WAV first
                reader = new WaveFileReader(ms);
                return true;
            }
            catch { reader = null; }

            try
            {
                var ms = new MemoryStream(data, writable: false);
                reader = new Mp3FileReader(ms);
                return true;
            }
            catch { reader = null; }

            try
            {
                var ms = new MemoryStream(data, writable: false);
                reader = new AiffFileReader(ms);
                return true;
            }
            catch { reader = null; }

            return false;
        }

        private sealed class AutoDisposeSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider _source;
            private readonly IDisposable _toDispose;
            private bool _disposed;
            public AutoDisposeSampleProvider(ISampleProvider source, IDisposable toDispose)
            {
                _source = source;
                _toDispose = toDispose;
                WaveFormat = source.WaveFormat;
            }
            public WaveFormat WaveFormat { get; }
            public int Read(float[] buffer, int offset, int count)
            {
                int read = _source.Read(buffer, offset, count);
                if (read == 0 && !_disposed)
                {
                    _disposed = true;
                    try { _toDispose.Dispose(); } catch { }
                }
                return read;
            }
        }

        private sealed class SoundPlayGate
        {
            private readonly int _maxCount;
            private readonly long _windowTicks;
            private readonly long _minSpacingTicks;
            private readonly object _lock = new();
            private readonly Queue<long> _timestamps = new();
            private long _lastTick;
            public SoundPlayGate(int maxCount, TimeSpan window, TimeSpan minSpacing)
            {
                _maxCount = Math.Max(1, maxCount);
                _windowTicks = (long)(window.TotalSeconds * Stopwatch.Frequency);
                _minSpacingTicks = (long)(minSpacing.TotalSeconds * Stopwatch.Frequency);
                _lastTick = 0;
            }
            public bool TryAcquire()
            {
                var now = Stopwatch.GetTimestamp();
                lock (_lock)
                {
                    // (2) Enforce minimum spacing first. If this fails, do not consume window quota.
                    if (_lastTick != 0 && now - _lastTick < _minSpacingTicks)
                        return false;

                    // (1) Enforce at most _maxCount in the sliding window
                    while (_timestamps.Count > 0 && now - _timestamps.Peek() > _windowTicks)
                        _timestamps.Dequeue();

                    if (_timestamps.Count >= _maxCount)
                        return false;

                    // Accept: record both window and last tick
                    _timestamps.Enqueue(now);
                    _lastTick = now;
                    return true;
                }
            }
        }
    }
}
