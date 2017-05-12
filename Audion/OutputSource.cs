using CSCore;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Audion
{
    public class OutputSource : ISource, IDisposable
    {
        private string _filename;
        private Uri _uri;
        private IWaveSource _waveSource;
        private ISampleSource _sampleSource;
        private ISoundOut _soundOut;

        private Timer _sourceTimer;
        private TimeSpan cachedPosition = TimeSpan.Zero;
        private PlaybackState cachedPlaybackState = PlaybackState.Stopped;

        private Device outputDevice;
        public Device OutputDevice
        {
            get { return outputDevice; }
            set { outputDevice = value; }
        }

        private BasicSpectrumProvider spectrumProvider;
        public BasicSpectrumProvider SpectrumProvider { get { return spectrumProvider; } }

        private float[] waveformData;
        public float[] WaveformData
        {
            get { return waveformData; }
        }

        private float[] fftData;
        public float[] FftData
        {
            get { return fftData; }
        }

        private FftSize fftSize = FftSize.Fft4096;
        public FftSize FftSize
        {
            get { return fftSize; }
            set
            {
                fftSize = value;
            }
        }

        private float volume = 1.0f;
        public float Volume
        {
            get { return _soundOut == null ? 0.0f : _soundOut.Volume; }
            set
            {
                volume = value;

                if (_soundOut != null)
                {
                    _soundOut.Volume = volume;
                }
            }
        }

        public static string SupportedFiles
        {
            get { return CSCore.Codecs.CodecFactory.SupportedFilesFilterEn; }
        }

        public TimeSpan Length
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetLength(); }
        }

        public long SampleLength
        {
            get { return _sampleSource == null ? 0L : _sampleSource.Length; }
        }

        public int BytesPerSecond
        {
            get { return _sampleSource == null ? 0 : (_sampleSource.WaveFormat.Channels * _sampleSource.WaveFormat.SampleRate); }
        }

        public TimeSpan Position
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetPosition(); }
            set
            {
                if (_waveSource != null)
                {
                    _waveSource.SetPosition(value);
                }
            }
        }

        public PlaybackState PlaybackState
        {
            get { return _soundOut == null ? PlaybackState.Stopped : _soundOut.PlaybackState; }
        }

        public bool IsStopped
        {
            get { return PlaybackState == PlaybackState.Stopped; }
        }

        public bool IsPlaying
        {
            get { return PlaybackState == PlaybackState.Playing; }
        }

        public bool Paused
        {
            get { return PlaybackState == PlaybackState.Paused; }
        }

        private bool hasMedia;
        public bool HasMedia
        {
            get { return hasMedia; }
        }

        #region Constructors

        public OutputSource()
        {
            outputDevice = Device.GetDefaultDevice();
            TimerSetup();
        }

        public OutputSource(Device device)
        {
            outputDevice = device;
            TimerSetup();
        }

        public OutputSource(string filename)
        {
            outputDevice = Device.GetDefaultDevice();
            Load(filename);
            TimerSetup();
        }

        public OutputSource(Device device, string filename)
        {
            outputDevice = device;
            Load(filename);
            TimerSetup();
        }

        public OutputSource(Uri uri)
        {
            outputDevice = Device.GetDefaultDevice();
            Load(uri);
            TimerSetup();
        }

        public OutputSource(Device device, Uri uri)
        {
            outputDevice = device;
            Load(uri);
            TimerSetup();
        }

        #endregion

        public void Dispose()
        {
            if (_waveSource != null)
            {
                _waveSource.Dispose();
                _waveSource = null;
            }

            if (_soundOut != null)
            {
                _soundOut.Dispose();
                _soundOut = null;
            }

            if (_sampleSource != null)
            {
                _sampleSource.Dispose();
                _sampleSource = null;
            }
        }

        #region Private Methods

        private void TimerSetup()
        {
            _sourceTimer = new Timer(TimerTick, null, 0, 10);
        }

        private void TimerTick(object state)
        {
            // This is a loop for the life of the source that reports things like 
            // position back to a subscriber.
            if (_waveSource != null)
            {
                if (Position != cachedPosition)
                {
                    // position has changed
                    cachedPosition = Position;
                    RaiseSourcePropertyChangedEvent(SourceProperty.Position, cachedPosition);
                }

                if (_soundOut != null)
                {
                    if (PlaybackState != cachedPlaybackState)
                    {
                        cachedPlaybackState = PlaybackState;
                        RaiseSourcePropertyChangedEvent(SourceProperty.PlaybackState, cachedPlaybackState);
                    }
                }

                if (SpectrumProvider != null && cachedPlaybackState == PlaybackState.Playing)
                {
                    fftData = new float[(int)fftSize];
                    SpectrumProvider.GetFftData(fftData);
                    RaiseSourcePropertyChangedEvent(SourceProperty.FftData, FftData);
                }
            }
        }

        private void LoadSoundOut()
        {
            if (_waveSource != null)
            {
                _soundOut = new CSCore.SoundOut.WasapiOut(true, CSCore.CoreAudioAPI.AudioClientShareMode.Shared,
                    100, System.Threading.ThreadPriority.Highest)
                {
                    Device = OutputDevice.ActualDevice
                };

                _soundOut.Initialize(_waveSource.ToSampleSource().ToWaveSource(16));
                // set defaults
                _soundOut.Volume = Volume;
            }
        }

        private float GetPeak(float[] values)
        {
            float peak = 0;
            var length = values.Length;

            for (var i = 0; i < length; i++)
            {
                var value = Math.Abs(values[i]);

                if (peak < value)
                {
                    peak = value;
                }
            }

            return peak;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously fetched data from an isolated SampleSource.
        /// </summary>
        public void GetData(int resolution = 2048)
        {
            Task.Factory.StartNew(() =>
            {
                var _source = CSCore.Codecs.CodecFactory.Instance.GetCodec(_filename).ToSampleSource();
                var blockSize = _sampleSource.Length / resolution;
                var buffer = new float[blockSize];
                var l = _sampleSource.Length / blockSize;

                if (l % 2 != 0)
                {
                    l++;
                }

                waveformData = new float[l];
                var waveformDataCount = 0;
                var flag = true;

                while (flag)
                {
                    var samplesToRead = buffer.Length;
                    var read = _source.Read(buffer, 0, samplesToRead);
                    waveformData[waveformDataCount] = GetPeak(buffer);

                    if (waveformDataCount % 2 != 0)
                    {
                        RaiseSourcePropertyChangedEvent(SourceProperty.WaveformData, waveformData);
                    }

                    waveformDataCount++;                    

                    if (read == 0)
                        flag = false;

                    if (waveformDataCount >= waveformData.Length)
                        flag = false;
                }

                _source.Dispose();
                _source = null;

                RaiseSourceEvent(Audion.SourceEventType.WaveformDataCompleted);
            });
        }

        /// <summary>
        /// Fetch data dynamically from the SampleSource.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public float[] GetDataRange(int start, int length, int resolution = 2048)
        {
            if (start + length > _sampleSource.Length)
                return null;

            if (resolution <= 0)
                return null;

            if (length < resolution)
                resolution = length;

            var blockSize = length / resolution;
            var samples = new float[resolution];

            _sampleSource.Position = start;
            var buffer = new float[blockSize];

            for (var i = 0; i < resolution; i++)
            {
                int read = _sampleSource.Read(buffer, 0, blockSize);

                if (read < buffer.Length)
                    Array.Clear(samples, read, buffer.Length - read);

                samples[i] = GetPeak(buffer);
            }

            return samples;
        }

        public void Load(string filename)
        {
            Dispose();
            _filename = filename;
            _waveSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(_filename)
                .ToSampleSource()
                .ToMono()
                .ToWaveSource();

            spectrumProvider = new BasicSpectrumProvider(_waveSource.WaveFormat.Channels,
                _waveSource.WaveFormat.SampleRate,
                CSCore.DSP.FftSize.Fft4096);

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(_waveSource.ToSampleSource());
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => SpectrumProvider.Add(a.Left, a.Right);
            _waveSource = notificationSource.ToWaveSource(16);
            // Load the sample source
            var ws = CSCore.Codecs.CodecFactory.Instance.GetCodec(_filename);
            _sampleSource = ws.ToSampleSource();
            RaiseSourceEvent(SourceEventType.Loaded);
            hasMedia = true;
            LoadSoundOut();
        }

        public void Load(Uri uri)
        {
            Dispose();
            _uri = uri;
            _waveSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(_uri)
                .ToSampleSource()
                .ToMono()
                .ToWaveSource();

            spectrumProvider = new BasicSpectrumProvider(_waveSource.WaveFormat.Channels,
                _waveSource.WaveFormat.SampleRate,
                CSCore.DSP.FftSize.Fft4096);

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(_waveSource.ToSampleSource());
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => SpectrumProvider.Add(a.Left, a.Right);
            _waveSource = notificationSource.ToWaveSource(16);
            // Load the sample source
            var ws = CSCore.Codecs.CodecFactory.Instance.GetCodec(_uri);
            _sampleSource = ws.ToSampleSource();
            RaiseSourceEvent(SourceEventType.Loaded);
            hasMedia = true;
            LoadSoundOut();
        }

        public bool Play()
        {
            if (_soundOut != null)
            {
                if (_soundOut.PlaybackState != CSCore.SoundOut.PlaybackState.Playing)
                {
                    _soundOut.Play();
                    return true;
                }
            }

            return false;
        }

        public bool Pause()
        {
            if (_soundOut != null)
            {
                if (_soundOut.PlaybackState != CSCore.SoundOut.PlaybackState.Paused)
                {
                    _soundOut.Pause();
                    return true;
                }
            }

            return false;
        }

        public bool Resume()
        {
            if (_soundOut != null)
            {
                if (_soundOut.PlaybackState == CSCore.SoundOut.PlaybackState.Paused)
                {
                    _soundOut.Resume();
                    return true;
                }
            }

            return false;
        }

        public bool Stop()
        {
            if (_soundOut != null)
            {
                _soundOut.Stop();
                _waveSource.Position = 0;
                return true;
            }

            return false;
        }

        #endregion

        #region Events

        public event EventHandler<SourcePropertyChangedEventArgs> SourcePropertyChangedEvent;
        public event EventHandler<SourceEventArgs> SourceEvent;

        public void RaiseSourceEvent(SourceEventType e)
        {
            SourceEvent?.Invoke(this, new SourceEventArgs
            {
                Event = e
            });
        }

        public void RaiseSourcePropertyChangedEvent(SourceProperty property, object value)
        {
            SourcePropertyChangedEvent?.Invoke(this, new SourcePropertyChangedEventArgs
            {
                Property = property,
                Value = value
            });
        }

        #endregion
    }
}
