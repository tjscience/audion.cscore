using Audion.Sample.Samples;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Audion.Sample
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Window, INotifyPropertyChanged
    {
        private OutputSource output;
        private InputSource input;
        private string inputMediaPath;
        // Samples
        private OutputWaveform outputWaveform;
        private OutputDynamicWaveform outputDynamicWaveform;
        private InputDynamicWaveform inputDynamicWaveform;
        private OutputTimeline outputTimeline;
        private OutputSpectrumAnalyzer outputSpectrumAnalyzer;
        private OutputTimeclock outputTimeclock;

        private ObservableCollection<Device> inputDevices = new ObservableCollection<Device>(
            Device.GetDevices());
        public ObservableCollection<Device> InputDevices
        {
            get { return inputDevices; }
            set
            {
                if (inputDevices != value)
                {
                    inputDevices = value;
                    NotifyPropertyChanged("InputDevices");
                }
            }
        }

        private ObservableCollection<Device> outputDevices = new ObservableCollection<Device>(
            Device.GetOutputDevices());
        public ObservableCollection<Device> OutputDevices
        {
            get { return outputDevices; }
            set
            {
                if (outputDevices != value)
                {
                    outputDevices = value;
                    NotifyPropertyChanged("OutputDevices");
                }
            }
        }

        private Device inputDevice = Device.GetDefaultRecordingDevice();
        public Device InputDevice
        {
            get { return inputDevice; }
            set
            {
                if (inputDevice != value)
                {
                    inputDevice = value;
                    NotifyPropertyChanged("InputDevice");

                    if (input != null)
                    {
                        input.InputDevice = InputDevice;
                    }
                }
            }
        }

        private Device outputDevice = Device.GetDefaultDevice();
        public Device OutputDevice
        {
            get { return outputDevice; }
            set
            {
                if (outputDevice != value)
                {
                    outputDevice = value;
                    NotifyPropertyChanged("OutputDevice");

                    if (output != null)
                    {
                        output.OutputDevice = OutputDevice;
                    }
                }
            }
        }

        public StartPage()
        {
            InitializeComponent();

            this.DataContext = this;

            output = new OutputSource(OutputDevice);
            input = new InputSource(InputDevice);

            SetupComboBoxes();
        }

        private void SetupComboBoxes()
        {
            foreach (var device in InputDevices)
            {
                var cbi = new ComboBoxItem();
                cbi.Content = string.Format("{0} [{1}]", device.Name, device.Type);
                cbi.Tag = device;
                cb_inputDevices.Items.Add(cbi);

                if (device.DeviceId == InputDevice.DeviceId)
                    cb_inputDevices.SelectedItem = cbi;

                cbi.PreviewMouseDown += cbi_PreviewMouseDown_InputDevice;
            }

            foreach (var device in OutputDevices)
            {
                var cbi = new ComboBoxItem();
                cbi.Content = string.Format("{0} [{1}]", device.Name, device.Type);
                cbi.Tag = device;
                cb_outputDevices.Items.Add(cbi);

                if (device.DeviceId == OutputDevice.DeviceId)
                    cb_outputDevices.SelectedItem = cbi;

                cbi.PreviewMouseDown += cbi_PreviewMouseDown_OutputDevice;
            }
        }

        private void cbi_PreviewMouseDown_InputDevice(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ComboBoxItem)
            {
                var cbi = sender as ComboBoxItem;
                InputDevice = cbi.Tag as Device;
                cb_inputDevices.SelectedItem = cbi;
            }
        }

        private void cbi_PreviewMouseDown_OutputDevice(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ComboBoxItem)
            {
                var cbi = sender as ComboBoxItem;
                InputDevice = cbi.Tag as Device;
                cb_inputDevices.SelectedItem = cbi;
            }
        }

        private void OpenSample(object sender, RoutedEventArgs e)
        {
            var sample = (string)((Button)sender).Tag;

            if (sample == "output-waveform")
            {
                if (outputWaveform == null)
                    outputWaveform = new OutputWaveform(output);

                Display.Content = outputWaveform;
            }
            else if (sample == "input-dynamic-waveform")
            {
                if (inputDynamicWaveform == null)
                    inputDynamicWaveform = new InputDynamicWaveform(input);

                Display.Content = inputDynamicWaveform;
            }
            else if (sample == "output-dynamic-waveform")
            {
                if (outputDynamicWaveform == null)
                    outputDynamicWaveform = new OutputDynamicWaveform(output);

                Display.Content = outputDynamicWaveform;
            }
            else if (sample == "output-timeline")
            {
                if (outputTimeline == null)
                    outputTimeline = new OutputTimeline(output);

                Display.Content = outputTimeline;
            }
            else if (sample == "output-spectrum-analyzer")
            {
                if (outputSpectrumAnalyzer == null)
                    outputSpectrumAnalyzer = new OutputSpectrumAnalyzer(output);

                Display.Content = outputSpectrumAnalyzer;
            }
            else if (sample == "output-timeclock")
            {
                if (outputTimeclock == null)
                    outputTimeclock = new OutputTimeclock(output);

                Display.Content = outputTimeclock;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            output.Dispose();
            input.Dispose();
        }

        private void LoadMedia(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Audion.OutputSource.SupportedFiles
            };
            if (openFileDialog.ShowDialog() == true)
            {
                output.Load(openFileDialog.FileName);
                OutputMediaPath.Text = openFileDialog.FileName;
            }
        }

        private void Focus(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((Grid)sender).Focus();
        }

        private void SetInputFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = Audion.OutputSource.SupportedFiles
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                InputMediaPath.Text = saveFileDialog.FileName;
                inputMediaPath = saveFileDialog.FileName;
            };
        }

        private void Play(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            output.Play();
        }

        private void PausePlayback(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            output.Pause();
        }

        private void StopPlayback(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            output.Stop();
        }

        private void Record(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            input.Record(inputMediaPath);
        }

        private void PauseRecording(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            input.Pause();
        }

        private void StopRecording(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            input.Stop();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
