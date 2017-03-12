using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Audion.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Audion.OutputSource _output;
        private Audion.InputSource _input;

        private ObservableCollection<Device> inputDevices = new ObservableCollection<Device>();
        public ObservableCollection<Device> InputDevices
        {
            get { return inputDevices; }
            set { inputDevices = value; }
        }

        private ObservableCollection<Device> outputDevices = new ObservableCollection<Device>();
        public ObservableCollection<Device> OutputDevices
        {
            get { return outputDevices; }
            set { outputDevices = value; }
        }

        private Device selectedInputDevice;
        public Device SelectedInputDevice
        {
            get { return selectedInputDevice; }
            set
            {
                if (selectedInputDevice != value)
                {
                    selectedInputDevice = value;
                    NotifyPropertyChanged("SelectedInputDevice");

                    if (_input != null)
                    {
                        _input.InputDevice = selectedInputDevice;
                    }
                }
            }
        }

        private Device selectedOutputDevice;
        public Device SelectedOutputDevice
        {
            get { return selectedOutputDevice; }
            set
            {
                if (selectedOutputDevice != value)
                {
                    selectedOutputDevice = value;
                    NotifyPropertyChanged("SelectedOutputDevice");
                }
            }
        }

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();

            this.Closed += MainWindow_Closed;

            GetInputDevices();
            GetOutputDevices();

            _output = new OutputSource();
            _input = new InputSource(SelectedInputDevice);
            spectrum.Source = _input;

        }

        private void GetInputDevices()
        {
            InputDevices.Clear();

            foreach (var device in Device.GetDevices())
            {
                InputDevices.Add(device);
            }
        }
        private void GetOutputDevices()
        {
            OutputDevices.Clear();

            foreach (var device in Device.GetOutputDevices())
            {
                OutputDevices.Add(device);
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _output.Dispose();

            // stop the recording if running
            if (_input.IsRecording)
            {
                _input.Stop();
            }

            _input.Dispose();
            
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Audion.OutputSource.SupportedFiles
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _output.Load(openFileDialog.FileName);
            }
        }

        private void Record(object sender, RoutedEventArgs e)
        {
            _input.Record("test.wav");
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _output.Play();
        }

        private void Pause(object sender, RoutedEventArgs e)
        {
            _output.Pause();
        }

        private void Resume(object sender, RoutedEventArgs e)
        {
            _output.Resume();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _output.Stop();
            _input.Stop();
        }

        private void FastForward(object sender, RoutedEventArgs e)
        {
            _output.Position = _output.Position.Add(TimeSpan.FromSeconds(10));
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
