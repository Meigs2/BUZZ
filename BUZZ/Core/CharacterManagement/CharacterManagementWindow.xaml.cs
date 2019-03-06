using BUZZ.Core.Models;
using BUZZ.Core.Verification;
using BUZZ.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BUZZ.Core.Hotkeys;

namespace BUZZ.Core.CharacterManagement
{
    /// <summary>
    /// Interaction logic for CharacterManagementWindow.xaml
    /// </summary>
    public partial class CharacterManagementWindow : Window
    {
        private int HotkeyColumnIndex = 8;

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

        private void DataGrid_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = (DataGrid) e.Source;
            var currentCell = source.CurrentCell;
            var column = currentCell.Column;

            var selectedCharacter = (BuzzCharacter) currentCell.Item;

            if (column.DisplayIndex == HotkeyColumnIndex)
            {
                var a = new CharacterHotkeyWindow();
                a.Show();
            }
        }
    }
}