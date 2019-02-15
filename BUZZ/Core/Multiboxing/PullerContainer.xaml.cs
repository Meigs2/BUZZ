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

namespace BUZZ.Core.Multiboxing
{
    /// <summary>
    /// Interaction logic for PullerContainer.xaml
    /// </summary>
    public partial class PullerContainer : UserControl
    {
        public PullerContainer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Resets and load all the UserControls to the grid.
        /// </summary>
        /// <param name="userControls"></param>
        public void LoadUserControlsToGrid(List<PullerView> userControls)
        {
            // Clear grid
            MultiboxingGrid.Children.Clear();
            MultiboxingGrid.ColumnDefinitions.Clear();
            MultiboxingGrid.RowDefinitions.Clear();

            // Check if theres a negative or zero row/column value in our pullers. If so, return and do not load, and inform the user.
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                if (buzzCharacter.RowNumber <= 0 || buzzCharacter.ColumnNumber <= 0)
                {
                    MessageBox.Show("A Character has a negative or zero column/row assignment. \n" +
                                    "Please return to the CharacterManager and set the negative or zero \n" +
                                    "assignment to a positive number.");
                    return;
                }
            }

            // Find number of rows and columns we need for the grid
            var maxCol = 1;
            var maxRow = 1;
            foreach (var pullerView in userControls)
            {
                if (pullerView.CurrentViewModel.Character.ColumnNumber > maxCol)
                {
                    maxCol = pullerView.CurrentViewModel.Character.ColumnNumber;
                }
                if (pullerView.CurrentViewModel.Character.RowNumber > maxRow)
                {
                    maxRow = pullerView.CurrentViewModel.Character.RowNumber;
                }
            }

            // Add columns and rows
            for (int i = 0; i < maxCol; i++)
            {
                MultiboxingGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < maxRow; i++)
            {
                MultiboxingGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Assign usercontrols to grids
            foreach (var pullerView in userControls)
            {
                MultiboxingGrid.Children.Add(pullerView);
                Grid.SetRow(pullerView, pullerView.CurrentViewModel.Character.RowNumber - 1);
                Grid.SetColumn(pullerView, pullerView.CurrentViewModel.Character.ColumnNumber - 1);
            }
        }
    }
}
