using NAudio.Wave;
using System.IO;
using System.Speech.Synthesis;

namespace AxiaFuturesDesktopApp.Services
{
    public class TtsService
    {
        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private WaveOutEvent _waveOut;
        private VolumeWaveProvider16 _volumeProvider;
        private MemoryStream _memoryStream;
        private float _lastVolume = 0.5f;
        private bool _isPaused;

        public TtsService()
        {
            _synthesizer.SpeakCompleted += OnSpeakCompleted;
        }

        public void Speak(string text, int volume)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Stop();

            _memoryStream = new MemoryStream();
            _synthesizer.SetOutputToWaveStream(_memoryStream);
            _synthesizer.SpeakAsync(text);
            _lastVolume = volume / 100f;
            _isPaused = false;
        }

        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            try
            {
                _synthesizer.SetOutputToNull();
                _memoryStream.Seek(0, SeekOrigin.Begin);

                var reader = new WaveFileReader(_memoryStream);
                _volumeProvider = new VolumeWaveProvider16(reader)
                {
                    Volume = _lastVolume
                };

                _waveOut = new WaveOutEvent();
                _waveOut.Init(_volumeProvider);
                if (!_isPaused)
                {
                    _waveOut.Play();
                }
            }
            catch (EndOfStreamException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao ler o áudio: {ex.Message}");
            }
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            _memoryStream?.Dispose();
            _memoryStream = null;

            _synthesizer.SpeakAsyncCancelAll();
            _isPaused = false;
        }

        public void Pause()
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
                _isPaused = true;
            }
            else if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
                _isPaused = false;
            }
        }

        public bool IsPlaying()
        {
            return _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing;
        }

        public void SetVolume(int volume)
        {
            if (_volumeProvider != null)
                _volumeProvider.Volume = volume / 100f;

            _lastVolume = volume / 100f;
        }
    }
}