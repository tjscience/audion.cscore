using CSCore;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Threading;
using System.Linq;

namespace Audion
{
    public class InputSource : ISource, IDisposable
    {
        private int[] bits = new int[] { 8, 16, 24, 32 };
        private Timer _sourceTimer;
        private WasapiCapture _capture;
        private SoundInSource _soundInSource;
        private IWaveSource _waveSource;
        private SingleBlockNotificationStream _notificationSource;
        private WaveWriter _waveWriter;

        private Device inputDevice;
        public Device InputDevice
        {
            get { return inputDevice; }
            set { inputDevice = value; }
        }

        private BasicSpectrumProvider spectrumProvider;
        public BasicSpectrumProvider SpectrumProvider { get { return spectrumProvider; } }

        private bool isRecording = false;
        public bool IsRecording
        {
            get { return isRecording; }
        }

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

        private InputType inputType = InputType.Capture;
        public InputType InputType
        {
            get { return inputType; }
            set { inputType = value; }
        }

        private int sampleRate = 44100;
        public int SampleRate
        {
            get { return sampleRate; }
            set
            {
                if (value < 100)
                    sampleRate = 100;
                else if (value > 200000)
                    sampleRate = 200000;
                else
                    sampleRate = value;
            }
        }

        private int bitResolution = 16;
        public int BitResolution
        {
            get { return bitResolution; }
            set
            {
                if (bits.Contains(value))
                    bitResolution = value;
                else
                    bitResolution = 24;
            }
        }

        public long SampleLength { get; }
        public int BytesPerSecond { get; }
        public TimeSpan Position { get; set; }
        public TimeSpan Length
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetLength(); }
        }

        #region Constructors

        public InputSource(Device device)
        {
            inputDevice = device;
            TimerSetup();
        }

        #endregion

        #region Private Methods

        private void TimerSetup()
        {
            _sourceTimer = new Timer(TimerTick, null, 0, 10);
        }

        private void TimerTick(object state)
        {
            if (SpectrumProvider != null && IsRecording)
            {
                fftData = new float[(int)fftSize];
                SpectrumProvider.GetFftData(fftData);
                RaiseSourcePropertyChangedEvent(SourceProperty.FftData, FftData);
            }
        }

        private void _soundInSource_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            byte[] buffer = new byte[_waveSource.WaveFormat.BytesPerSecond / 2];
            int read;

            //keep reading as long as we still get some data
            while ((read = _waveSource.Read(buffer, 0, buffer.Length)) > 0)
            {
                //write the read data to a file
                _waveWriter.Write(buffer, 0, read);
            }
        }

        #endregion

        #region Public Methods

        public void GetData(int resolution = 2048)
        {
            waveformData = new float[2048];
        }

        public float[] GetDataRange(int start, int length, int resolution = 2048)
        {
            return new float[2048];
        }

        public void Record(string filename)
        {
            if (InputDevice == null)
                return;

            if (IsRecording)
                return;

            isRecording = true;

            if (inputDevice.Type == DeviceType.Capture)
                _capture = new WasapiCapture();
            else
                _capture = new WasapiLoopbackCapture();

            _capture.Device = inputDevice.ActualDevice;
            _capture.Initialize();

            _soundInSource = new SoundInSource(_capture) { FillWithZeros = false };
            _soundInSource.DataAvailable += _soundInSource_DataAvailable;

            _waveSource = _soundInSource
                .ChangeSampleRate(SampleRate)
                .ToSampleSource()
                .ToWaveSource(BitResolution)
                .ToMono();

            spectrumProvider = new BasicSpectrumProvider(_waveSource.WaveFormat.Channels,
                _waveSource.WaveFormat.SampleRate,
                CSCore.DSP.FftSize.Fft4096);

            _waveWriter = new WaveWriter(filename, _waveSource.WaveFormat);

            //the SingleBlockNotificationStream is used to intercept the played samples
            _notificationSource = new SingleBlockNotificationStream(_waveSource.ToSampleSource());
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            _notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);
            _waveSource = _notificationSource.ToWaveSource(16);

            RaiseSourceEvent(SourceEventType.Loaded);
            _capture.Start();
            RaiseSourcePropertyChangedEvent(SourceProperty.RecordingState, _capture.RecordingState);
        }

        public void Stop()
        {
            if (_capture != null)
            {
                _capture.Stop();
                isRecording = false;
                RaiseSourcePropertyChangedEvent(SourceProperty.RecordingState, _capture.RecordingState);
                Dispose();
            }
        }

        public void Dispose()
        {
            if (_waveWriter != null)
            {
                _waveWriter.Dispose();
                _waveWriter = null;
            }

            if (_notificationSource != null)
            {
                _notificationSource.Dispose();
                _notificationSource = null;
            }

            if (_waveSource != null)
            {
                _waveSource.Dispose();
                _waveSource = null;
            }

            if (_soundInSource != null)
            {
                _soundInSource.Dispose();
                _soundInSource = null;
            }

            if (_capture != null)
            {
                _capture.Dispose();
                _capture = null;
            }

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

    public enum InputType
    {
        Capture,
        LoopbackCapture
    }
}
