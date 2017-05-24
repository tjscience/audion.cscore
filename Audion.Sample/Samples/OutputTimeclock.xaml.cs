using System.Windows.Controls;

namespace Audion.Sample.Samples
{
    /// <summary>
    /// Interaction logic for OutputTimeclock.xaml
    /// </summary>
    public partial class OutputTimeclock : UserControl
    {
        public OutputTimeclock(OutputSource output)
        {
            InitializeComponent();
            Timeclock.SourceCollection.Add(output);
            PropertyGrid.SelectedObject = Timeclock;
            PropertyGrid.SelectedType = "Timeclock (Output)";

        }
    }
}
