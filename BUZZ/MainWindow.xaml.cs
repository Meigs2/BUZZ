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

        private async void MenuItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var verificationWindow = new UI.VerificationWindow(Utilities.EsiData.EsiClient);
            verificationWindow.ShowDialog();

            var dto = new AuthDTO()
            {
                AccessToken = verificationWindow.AccessTokenDetails,
                CharacterId = verificationWindow.CharacterDetails.CharacterId,
                Scopes = Scopes.ESI_LOCATION_READ_LOCATION_1
            };

            var a = await Utilities.EsiData.EsiClient.Location.GetCharacterLocationV1Async(dto);
        }
    }
}
