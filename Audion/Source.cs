using CSCore;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Audion
{
    public class Source : IDisposable
    {
        private string _filename;
        private IWaveSource _waveSource;
        private ISoundOut _soundOut;
        public BasicSpectrumProvider SpectrumProvider;

        protected const double MinDbValue = -90;
        protected const double MaxDbValue = 0;
        protected const double DbScale = (MaxDbValue - MinDbValue);
        private int[] _spectrumIndexMax = new int[100];
        private int _maxFftIndex = 4096 / 2 - 1;
        private int _maximumFrequency = 20000;
        private int _maximumFrequencyIndex;
        private int _minimumFrequency = 20; //Default spectrum from 20Hz to 20kHz
        private int _minimumFrequencyIndex;
        private int SpectrumResolution = 50;


        //private ISampleSource _sampleSource;
        private Timer _sourceTimer;
        private TimeSpan cachedPosition = TimeSpan.Zero;
        private PlaybackState cachedPlaybackState = PlaybackState.Stopped;

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

        public static string SupportedFiles
        {
            get { return CSCore.Codecs.CodecFactory.SupportedFilesFilterEn; }
        }

        public TimeSpan Length
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetLength(); }
        }
        public TimeSpan Position
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetPosition(); }
        }
        public PlaybackState PlaybackState
        {
            get { return _soundOut == null ? PlaybackState.Stopped : _soundOut.PlaybackState; }
        }

        #region Constructors

        public Source()
        {
            TimerSetup();
        }

        public Source(string filename)
        {
            Load(filename);
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
                    RaiseSourcePropertyChangedEvent(Property.Position, cachedPosition);
                }

                if (_soundOut != null)
                {
                    if (PlaybackState != cachedPlaybackState)
                    {
                        cachedPlaybackState = PlaybackState;
                        RaiseSourcePropertyChangedEvent(Property.PlaybackState, cachedPlaybackState);
                    }
                }

                if (SpectrumProvider != null && cachedPlaybackState == PlaybackState.Playing)
                {
                    fftData = new float[(int)fftSize];
                    SpectrumProvider.GetFftData(fftData);
                    RaiseSourcePropertyChangedEvent(Property.FftData, FftData);
                    //var spectrum = CalculateSpectrumPoints(100, fftData);
                }
            }
        }

        private void LoadSoundOut()
        {
            if (_waveSource != null)
            {
                _soundOut = new CSCore.SoundOut.WasapiOut(true, CSCore.CoreAudioAPI.AudioClientShareMode.Exclusive,
                    100, System.Threading.ThreadPriority.Highest)
                {
                    Device = Device.GetDefaultDevice().ActualDevice
                };

                _soundOut.Initialize(_waveSource.ToSampleSource().ToWaveSource(16));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously fetched data from the WaveSource.
        /// </summary>
        public void GetData(int resolution = 2048)
        {
            Task.Factory.StartNew(() =>
            {
                if (_waveSource == null)
                    throw new ArgumentNullException("waveSource");

                var waveSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(_filename);
                var sampleSource = waveSource.ToSampleSource();

                var channels = sampleSource.WaveFormat.Channels;
                double waveformResolution = Math.Pow(2, resolution);
                var blockSize = sampleSource.Length / resolution;
                var buffer = new float[blockSize];
                var l = sampleSource.Length / blockSize;

                if (l % 2 != 0)
                {
                    l++;
                }

                int sleep = ((int)l / 1000);

                waveformData = new float[l];
                var waveformDataCount = 0;

                var flag = true;
                while (flag)
                {
                    var samplesToRead = buffer.Length;
                    var read = sampleSource.Read(buffer, 0, samplesToRead);
                    var pos = buffer.Where(x => x > 0).ToArray();
                    var neg = buffer.Where(x => x < 0).ToArray();
                    var peakPos = pos.Length > 0 ? pos.Max() : 0;
                    //var peakNeg = neg.Length > 0 ? Math.Abs(neg.Min()) : 0;
                    //var peak = peakPos > peakNeg ? peakPos : peakNeg;
                    waveformData[waveformDataCount] = peakPos;

                    if (waveformDataCount % 2 != 0)
                    {
                        //RaisePropertyChangedEvent()
                        RaiseSourcePropertyChangedEvent(Property.WaveformData, waveformData);
                        Thread.Sleep(sleep);
                    }

                    waveformDataCount++;                    

                    if (read == 0)
                        flag = false;

                    if (waveformDataCount >= waveformData.Length)
                        flag = false;
                }

                waveSource.Position = 0;
                ((IAudioSource)waveSource).Dispose();
                waveSource.Dispose();
                //RaiseDataProcessedEvent();
                RaiseSourceEvent(Event.WaveformDataCompleted);
            });
        }

        public void Load(string filename)
        {
            Dispose();
            _filename = filename;
            _waveSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(_filename)
                .ToSampleSource()
                .ToMono()
                .ToWaveSource();
            //RaiseSourceLoadedEvent();
            
            SpectrumProvider = new BasicSpectrumProvider(_waveSource.WaveFormat.Channels,
                _waveSource.WaveFormat.SampleRate, 
                CSCore.DSP.FftSize.Fft4096);

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(_waveSource.ToSampleSource());
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => SpectrumProvider.Add(a.Left, a.Right);

            _waveSource = notificationSource.ToWaveSource(16);

            RaiseSourceEvent(Event.Loaded);

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

        public void SetPosition(TimeSpan position)
        {
            _waveSource.SetPosition(position);
        }

        #endregion

        #region Events

        public event EventHandler<SourcePropertyChangedEventArgs> SourcePropertyChangedEvent;
        public event EventHandler<SourceEventArgs> SourceEvent;

        void RaiseSourceEvent(Event e)
        {
            SourceEvent?.Invoke(this, new SourceEventArgs
            {
                Event = e
            });
        }

        void RaiseSourcePropertyChangedEvent(Property property, object value)
        {
            SourcePropertyChangedEvent?.Invoke(this, new SourcePropertyChangedEventArgs
            {
                Property = property,
                Value = value
            });
        }

        #endregion

        #region Enums

        public enum Property
        {
            FftData,
            PlaybackState,
            Position,
            WaveformData,
        }

        public enum Event
        {
            Loaded,
            WaveformDataCompleted,
        }

        #endregion
    }

    public class SourcePropertyChangedEventArgs : EventArgs
    {
        public Source.Property Property { get; set; }
        public object Value { get; set; }
    }

    public class SourceEventArgs : EventArgs
    {
        public Source.Event Event { get; set; }
    }
}
