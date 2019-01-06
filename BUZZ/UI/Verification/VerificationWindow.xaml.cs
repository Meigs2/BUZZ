using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using EVEStandard;
using EVEStandard.Models.SSO;

namespace BUZZ.UI
{
    /// <summary>
    ///     Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow
    {
        private readonly EVEStandardAPI _client;
        public CharacterDetails CharacterDetails;
        public AccessTokenDetails AccessTokenDetails;


        public VerificationWindow(EVEStandardAPI client)
        {
            InitializeComponent();
            _client = client;
            authorization = client.SSOv2.AuthorizeToEVEUri(Utilities.EsiScopes.Scopes);
        }

        private Authorization authorization { get; }


        private void LoginImage_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(authorization.SignInURI);
        }

        private async void AcceptButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AuthCodeTextBox.Text == string.Empty) return;

                authorization.AuthorizationCode = AuthCodeTextBox.Text;
                authorization.ExpectedState = string.Empty; // Expected state is set to empty, as we dont require the user to provide it from the returned URL
                AccessTokenDetails = await _client.SSOv2.VerifyAuthorizationAsync(authorization);
                CharacterDetails = _client.SSOv2.GetCharacterDetailsAsync(AccessTokenDetails.AccessToken);
                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show("Authorization verification failed, error message:\n" + error.Message +
                                "\n Source: \n" + error.Source);
            }
        }
    }
}