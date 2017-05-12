using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Audion.Sample.Samples
{
    /// <summary>
    /// Interaction logic for OutputWaveform.xaml
    /// </summary>
    public partial class OutputTimeline : UserControl, ISample
    {

        public OutputTimeline(OutputSource output)
        {
            InitializeComponent();
            Timeline.Source = output;
            PropertyGrid.SelectedObject = Timeline;
            PropertyGrid.SelectedType = "Timeline (Output)";
        }

    }
}
