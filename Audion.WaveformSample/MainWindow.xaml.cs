using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;

namespace Audion.WaveformSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Audion.Source _source;

        public MainWindow()
        {
            InitializeComponent();

            this.Closed += MainWindow_Closed;

            _source = new Source();

            // set the source for the controls
            timeline.Source = _source;
            waveform.Source = _source;
            spectrum.Source = _source;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _source.Dispose();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Audion.Source.SupportedFiles
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _source.Load(openFileDialog.FileName);
            }
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            _source.Play();
        }

        private void Pause(object sender, RoutedEventArgs e)
        {
            _source.Pause();
        }

        private void Resume(object sender, RoutedEventArgs e)
        {
            _source.Resume();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            _source.Stop();
        }

        private void FastForward(object sender, RoutedEventArgs e)
        {
            _source.SetPosition(_source.Position.Add(TimeSpan.FromSeconds(10)));
        }
    }
}
