using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using BUZZ.Data;
using BUZZ.Data.Models;
using EVEStandard;
using EVEStandard.Models.SSO;

namespace BUZZ.Core.Verification
{
    /// <summary>
    ///     Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow
    {
        private readonly EVEStandardAPI _client;
        public BuzzCharacter Character { get; set; } = new BuzzCharacter();
        private Authorization Authorization { get; }



        public VerificationWindow(EVEStandardAPI client)
        {
            InitializeComponent();

            _client = client;
            Authorization = _client.SSOv2.AuthorizeToEVEUri(EsiScopes.Scopes);
        }


        private void LoginImage_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Authorization.SignInURI);
        }

        private async void AcceptButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AuthCodeTextBox.Text == string.Empty) return;

                Authorization.AuthorizationCode = AuthCodeTextBox.Text;
                Authorization.ExpectedState = string.Empty; // Expected state is set to empty, as we don't require the user to provide it from the returned URL

                Character.AccessTokenDetails = await _client.SSOv2.VerifyAuthorizationAsync(Authorization);
                Character.CharacterDetails = _client.SSOv2.GetCharacterDetailsAsync(Character.AccessTokenDetails.AccessToken);

                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show("Authorization verification failed, error message:\n" + error.Message +
                                "\n Source: \n" + error.Source + "\n\n Please click the image again and re-enter the code in the web prompt.");
            }
        }
    }
}