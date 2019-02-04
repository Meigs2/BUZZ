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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BUZZ.Core.CharacterManagement;
using BUZZ.Core.LPManager;
using BUZZ.Core.Multiboxing;
using BUZZ.Properties;
using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models.API;

namespace BUZZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Utilities.Startup.PerformStartupActions();
        }

        private void MenuItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var manager = new CharacterManagementWindow();
            manager.ShowDialog();
        }

        private void CharacterLPViewer_Click(object sender, RoutedEventArgs e)
        {
            var lpViewer = new LpViewer();
            lpViewer.Show();
        }

        private async void AboutMenuitem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                TestWrapPannel.Children.Add(new PullerView(buzzCharacter));
                await buzzCharacter.UpdateCharacterInformation();
            }
        }
    }
}
