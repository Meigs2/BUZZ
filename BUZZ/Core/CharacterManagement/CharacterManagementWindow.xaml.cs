using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using BUZZ.Core.Models;
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
            DataGrid.ItemsSource = CharacterManager.CurrentInstance.CharacterList;
        }

        private void AddCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var verificationWindow = new VerificationWindow(EsiData.EsiClient);
            verificationWindow.ShowDialog();

            RefreshDataGrid();
        }

        private void RemoveCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItems != null && DataGrid.SelectedItems.Count > 0)
            {
                List<BuzzCharacter> toRemove = DataGrid.SelectedItems.Cast<BuzzCharacter>().ToList();
                var diffList =
                    CharacterManager.CurrentInstance.CharacterList.Except(toRemove).ToList();
                CharacterManager.CurrentInstance.CharacterList = new BindingList<BuzzCharacter>(diffList);
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = CharacterManager.CurrentInstance.CharacterList;
        }


        private async void CharacterManagementWindow_OnClosing(object sender, CancelEventArgs e)
        {
            await CharacterManager.RefreshCharacterInformation();
            CharacterManager.SerializeCharacterData();
        }
    }
}
