using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BUZZ.Utilities;

namespace BUZZ.Core.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public IEnumerable<string> Systems;

        public SettingsWindow()
        {
            InitializeComponent();
            Closing += SettingsWindow_Closing;
            Systems = SolarSystems.GetAllSolarSystems();
            SystemsComboBox.ItemsSource = Systems;
            SystemsComboBox.SelectedItem = SolarSystems.GetSolarSystemName(Properties.Settings.Default.DestinationSystem);
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.DestinationSystem =
                SolarSystems.GetSolarSystemId(SystemsComboBox.SelectedItem.ToString());
            Properties.Settings.Default.Save();
        }
    }
}
