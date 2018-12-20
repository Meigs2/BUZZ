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
            Uri = client.SSOv2.AuthorizeToEVEUri(Utilities.EsiScopes.Scopes);
        }

        private Authorization Uri { get; }


        private void LoginImage_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Uri.SignInURI);
        }

        private async void AcceptButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AuthCodeTextBox.Text == string.Empty) return;

                var authorization = new Authorization {AuthorizationCode = AuthCodeTextBox.Text};
                AccessTokenDetails = await _client.SSOv2.VerifyAuthorizationAsync(authorization);
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