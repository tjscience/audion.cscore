using CSCore;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        private TimeSpan cachedPosition = TimeSpan.Zero;

        private Device inputDevice;
        public Device InputDevice
        {
            get { return inputDevice; }
            set { inputDevice = value; }
        }

        private BasicSpectrumProvider spectrumProvider;
        public BasicSpectrumProvider SpectrumProvider { get { return spectrumProvider; } }

        private float[] waveformData;
        public float[] WaveformData
        {
            get { return waveformData; }
        }

        private RecordingState recordingState;
        public RecordingState RecordingState
        {
            get { return recordingState; }
            private set
            {
                if (recordingState != value)
                {
                    recordingState = value;
                    RaiseSourcePropertyChangedEvent(SourceProperty.RecordingState, recordingState);
                }
            }
        }

        private List<float> recordedData = new List<float>();

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

        private long sampleLength;
        public long SampleLength
        {
            get { return sampleLength; }
            set { sampleLength = value; }
        }

        public int BytesPerSecond
        {
            get { return _waveSource == null ? 0 : (_waveSource.WaveFormat.Channels * _waveSource.WaveFormat.SampleRate); }
        }

        private TimeSpan position;
        public TimeSpan Position
        {
            get { return position; }
            set { position = value; }
        }

        public TimeSpan Length
        {
            get { return _waveSource == null ? TimeSpan.Zero : _waveSource.GetLength(); }
        }

        public bool IsRecording
        {
            get { return recordingState == RecordingState.Recording; }
        }

        public bool IsPaused
        {
            get { return recordingState == RecordingState.Paused; }
        }

        public bool IsStopped
        {
            get { return recordingState == RecordingState.Stopped; }
        }

        #region Constructors

        public InputSource()
        {
            inputDevice = Device.GetDefaultRecordingDevice();
            TimerSetup();
        }

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
            if (_waveSource != null)
            {
                if (recordingState == RecordingState.Recording)
                {
                    if (Position - TimeSpan.FromMilliseconds(50) > cachedPosition)
                    //if (Position != cachedPosition)
                    {
                        // position has changed
                        cachedPosition = Position;
                        RaiseSourcePropertyChangedEvent(SourceProperty.Position, cachedPosition);
                    }
                }

                if (SpectrumProvider != null && recordingState == RecordingState.Recording)
                {
                    fftData = new float[(int)fftSize];
                    SpectrumProvider.GetFftData(fftData);
                    RaiseSourcePropertyChangedEvent(SourceProperty.FftData, FftData);
                }
            }
        }

        private void _soundInSource_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            if (recordingState != RecordingState.Recording)
                return;

            byte[] buffer = new byte[_waveSource.WaveFormat.BytesPerSecond / 2];
            int read;

            //keep reading as long as we still get some data
            while ((read = _waveSource.Read(buffer, 0, buffer.Length)) > 0)
            {
                double seconds = (double)read / (double)_waveSource.WaveFormat.BytesPerSecond;
                position += TimeSpan.FromSeconds(seconds);
                //write the read data to a file
                _waveWriter.Write(buffer, 0, read);
                sampleLength += read;
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

        public void GetData(int resolution = 2048)
        {
            waveformData = new float[2048];
        }

        public float[] GetDataRange(int start, int length, int resolution = 2048)
        {
            var data = recordedData.ToArray();
            recordedData.Clear();

            if (data.Length == 0)
                return null;

            if (resolution <= 0)
                return null;

            if (data.Length < resolution)
                resolution = length;

            var blockSize = data.Length / resolution;
            var samples = new float[resolution];

            //_sampleSource.Position = start;
            var buffer = new float[blockSize];

            for (var i = 0; i < resolution; i++)
            {
                //int read = _sampleSource.Read(buffer, 0, blockSize);

                Array.Copy(data, i * blockSize, buffer, 0, blockSize);

                /*if (read < buffer.Length)
                    Array.Clear(samples, read, buffer.Length - read);*/

                var value = GetPeak(buffer);

                if (value < 1E-10 || value > 1E+10)
                    value = 0;

                samples[i] = value;
            }

            return samples;

        }

        public void Record(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return;

            cachedPosition = TimeSpan.Zero;
            position = TimeSpan.Zero;
            sampleLength = 0;
            recordedData = new List<float>();

            if (InputDevice == null)
                return;

            if (recordingState == RecordingState.Recording)
                return;

            recordingState = RecordingState.Recording;

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
            _notificationSource.SingleBlockRead += _notificationSource_SingleBlockRead;
            _waveSource = _notificationSource.ToWaveSource(16);

            RaiseSourceEvent(SourceEventType.Loaded);
            _capture.Start();
            RaiseSourcePropertyChangedEvent(SourceProperty.RecordingState, _capture.RecordingState);
        }

        private void _notificationSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            recordedData.Add(e.Left);
            recordedData.Add(e.Right);
            spectrumProvider.Add(e.Left, e.Right);
        }

        public void Pause()
        {
            if (recordingState == RecordingState.Recording)
            {
                recordingState = RecordingState.Paused;
            }
        }

        public void Resume()
        {
            if (recordingState == RecordingState.Paused)
            {
                recordingState = RecordingState.Recording;
            }
        }

        public void Stop()
        {
            if (_notificationSource != null)
            {
                _notificationSource.SingleBlockRead -= _notificationSource_SingleBlockRead;
            }

            if (_capture != null)
            {
                _capture.Stop();
                recordingState = RecordingState.Stopped;
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
