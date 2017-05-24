using Audion.Visualization;
using System.ComponentModel;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;

namespace Audion.Sample.Controls
{
    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    public partial class PropertyGrid : UserControl, INotifyPropertyChanged
    {
        private object selectedObject;
        public object SelectedObject
        {
            get { return selectedObject; }
            set
            {
                if (selectedObject != value)
                {
                    selectedObject = value;
                    NotifyPropertyChanged("SelectedObject");
                }
            }
        }

        private string selectedType;
        public string SelectedType
        {
            get { return selectedType; }
            set
            {
                if (selectedType != value)
                {
                    selectedType = value;
                    NotifyPropertyChanged("SelectedType");
                }
            }
        }

        public PropertyGrid()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void ClockTypeChanged(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cbi = (ComboBoxItem)sender;

            var timeclock = cbi.Tag as Timeclock;
            var clockType = (string)cbi.Content;

            timeclock.ClockType = (ClockType)System.Enum.Parse(typeof(ClockType), clockType);
            (cbi.Parent as ComboBox).Text = clockType;
        }

        private void ClockTypeComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ComboBox)sender).Text = "TimeElapsed";
        }

        private void LabelFontComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ComboBox)sender).Text = "Segoe UI";
        }

        private void TimeFontComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ComboBox)sender).Text = "Ebrima";
        }

        private void FontChanged(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cbi = (ComboBoxItem)sender;

            var font = cbi.DataContext as FontFamily;
            var parent = ItemsControl.ItemsControlFromItemContainer(cbi) as ComboBox;

            if (parent.Name == "LabelFont")
            {
                var timeclock = parent.DataContext as Timeclock;
                timeclock.LabelFont = font;
                parent.Text = font.Source;
            }
            else if (parent.Name == "TimeFont")
            {
                var timeclock = parent.DataContext as Timeclock;
                timeclock.TimeFont = font;
                parent.Text = font.Source;
            }

        }
    }
}
