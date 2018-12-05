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
using BUZZ.Properties;
using EVEStandard;
using EVEStandard.Enumerations;

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

        private void AboutMenuitem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var esiClientv2 = new EVEStandardAPI(
                "BUZZ",
                DataSource.Tranquility,
                TimeSpan.FromSeconds(30),
                "https://meigs2.github.io/ESICallback/",
                "a8c4bd8f30444c65b2c68d0eb886c545"
            );

            var verificationWindow = new UI.VerificationWindow(esiClientv2);
            verificationWindow.ShowDialog();

        }
    }
}
