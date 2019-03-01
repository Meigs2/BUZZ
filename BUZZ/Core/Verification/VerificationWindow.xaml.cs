using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BUZZ.Core.CharacterManagement;
using BUZZ.Core.Models;
using BUZZ.Data;
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
        private Authorization Authorization { get; set; }

        public static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public VerificationWindow(EVEStandardAPI client)
        {
            Log.Info("New Verification Window Opened");
            InitializeComponent();

            _client = client;
        }


        private void LoginImage_Click(object sender, MouseButtonEventArgs e)
        {
            Authorization = _client.SSOv2.AuthorizeToEVEUri(EsiScopes.Scopes);
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

                // If this character is already added to our list, ask if we want to replace it.
                if (CharacterManager.CurrentInstance.CharacterList.SingleOrDefault(c => c.CharacterName == Character.CharacterName) != null)
                {
                    var duplicate =
                        CharacterManager.CurrentInstance.CharacterList.Single(c =>
                            c.CharacterName == Character.CharacterName);
                    var dialogResult = MessageBox.Show("Character is already added, would you like to replace it?","",MessageBoxButton.YesNo);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        CharacterManager.CurrentInstance.CharacterList[
                                CharacterManager.CurrentInstance.CharacterList.IndexOf(duplicate)] =
                            Character;
                        Close();
                        return;
                    }
                    else
                    {
                        Close();
                        return;
                    }
                }

                CharacterManager.CurrentInstance.CharacterList.Add(Character);
                Close();
            }
            catch (Exception error)
            {
                Log.Error(error);
                MessageBox.Show("Authorization verification failed, error message:\n" + error.Message +
                                "\n Source: \n" + error.Source + "\n\n Please click the image again and re-enter the code in the web prompt.");
            }
        }
    }
}