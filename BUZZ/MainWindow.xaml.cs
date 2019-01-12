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
        }

        private void MenuItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var manager = new CharacterManagementWindow();
            manager.ShowDialog();
        }
    }
}
