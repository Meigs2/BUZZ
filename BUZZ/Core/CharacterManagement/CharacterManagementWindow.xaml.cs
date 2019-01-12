using System.Windows;
using BUZZ.Core.Verification;
using BUZZ.Data;
using EVEStandard.Enumerations;
using EVEStandard.Models.API;

namespace BUZZ.Core.CharacterManagement
{
    /// <summary>
    /// Interaction logic for CharacterManagementWindow.xaml
    /// </summary>
    public partial class CharacterManagementWindow : Window
    {
        public CharacterManagementWindow()
        {
            InitializeComponent();
        }

        private async void AddCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var verificationWindow = new VerificationWindow(EsiData.EsiClient);
            verificationWindow.ShowDialog();

            CharacterManager.CurrentInstance.AddNewCharacter(verificationWindow.BuzzCharacter);
        }
    }
}
