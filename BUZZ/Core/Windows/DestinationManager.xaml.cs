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
using BUZZ.Core.CharacterManagement;
using BUZZ.Utilities;

namespace BUZZ.Core
{
    /// <summary>
    /// Interaction logic for DestinationManager.xaml
    /// </summary>
    public partial class DestinationManager : Window
    {
        public int SystemID { get; set; } = 0;

        public IEnumerable<string> Systems;

        public DestinationManager()
        {
            InitializeComponent();
            Systems = SolarSystems.GetAllSolarSystems();
            SystemsComboBox.ItemsSource = Systems;
            SystemsComboBox.Focus();
        }


        private void SystemsComboBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AcceptButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SystemID = SolarSystems.GetSolarSystemId(SystemsComboBox.SelectedItem.ToString());
                foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
                {
                    if (buzzCharacter.IsOnline || Properties.Settings.Default.DestinationManagerIncludeOffline)
                    {
                        await buzzCharacter.SetWaypoints(new List<int>(){SystemID}, Properties.Settings.Default.ClearOtherWaypointsDestinationManager);
                    }
                }
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Could not find a system with that name.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            BUZZ.Properties.Settings.Default.Save();
        }
    }
}
