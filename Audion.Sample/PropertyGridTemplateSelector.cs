using Audion.Visualization;
using System.Windows;
using System.Windows.Controls;

namespace Audion.Sample
{
    public class PropertyGridTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultDataTemplate { get; set; }
        public DataTemplate WaveformDataTemplate { get; set; }
        public DataTemplate DynamicWaveformDataTemplate { get; set; }
        public DataTemplate OutputTimelineDataTemplate { get; set; }
        public DataTemplate OutputSpectrumAnalyzerDataTemplate { get; set; }
        public DataTemplate OutputTimeclockDataTemplate { get; set; }
        public DataTemplate TimeclockDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Waveform)
                return WaveformDataTemplate;
            if (item is DynamicWaveform)
                return DynamicWaveformDataTemplate;
            if (item is Timeline)
                return OutputTimelineDataTemplate;
            if (item is SpectrumAnalyzer)
                return OutputSpectrumAnalyzerDataTemplate;
            if (item is Timeclock)
                return TimeclockDataTemplate;

            return DefaultDataTemplate;
        }
    }
}
