using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;
using BUZZ.Core.CharacterManagement;
using BUZZ.Data;
using EVEStandard.Models;
using EVEStandard.Models.API;

namespace BUZZ.Core.LPManager
{
    /// <summary>
    /// Interaction logic for LpViewer.xaml
    /// </summary>
    public partial class LpViewer : Window
    {
        private DataTable Table = new DataTable();

        public List<List<LoyaltyPoints>> CharacterLoyalty { get; set; } = new List<List<LoyaltyPoints>>();

        /// <summary>
        /// Class simply displays all current 
        /// </summary>
        public LpViewer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var corpIds = new List<int>();
            var corpNames = new List<UniverseIdsToNames>();
            LoadLoyaltyPoints();
            /*foreach (var CharacterLp in CharacterLpList)
            {
                foreach (var loyaltyPoints in CharacterLp)
                {
                    if (!corpIds.Contains(loyaltyPoints.CorporationId))
                    {
                        corpIds.Add(loyaltyPoints.CorporationId);
                    }
                }
            }

            var idResults = await EsiData.EsiClient.Universe.GetNamesAndCategoriesFromIdsV2Async(corpIds);
            corpNames = idResults.Model;
            var currentCorpList = new List<int>();
            foreach (var corporation in corpNames)
            {
                DataGrid.Columns.Add(new DataGridTextColumn(){ Header = corporation.Name });
                currentCorpList.Add(corporation.Id);
            }

            for (int i = 0; i < CharacterLpList.Count; i++)
            {
                var currentItem = new List<int>(currentCorpList.Count);
                var characterLp = CharacterLpList[i];
                for (int j = 0; j < currentCorpList.Count; j++)
                {
                    if (expr)
                    {
                        
                    }
                }

            }*/
        }

        private async void LoadLoyaltyPoints()
        {
            var CharacterLpList = new List<List<LoyaltyPoints>>();
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                var result = await buzzCharacter.GetLoyaltyPointsAsync();
                CharacterLpList.Add(result.Model);
            }
            // Get corporation names from ID's
            var corpIds = new List<int>();
            var corpNames = new List<UniverseIdsToNames>();
            foreach (var CharacterLp in CharacterLpList)
            {
                foreach (var loyaltyPoints in CharacterLp)
                {
                    if (!corpIds.Contains(loyaltyPoints.CorporationId))
                    {
                        corpIds.Add(loyaltyPoints.CorporationId);
                    }
                }
            }
            var idResults = await EsiData.EsiClient.Universe.GetNamesAndCategoriesFromIdsV2Async(corpIds);
            corpNames = idResults.Model;


            string[,] dataView = new string[CharacterLpList.Count, corpIds.Count+1];

            for (int i = 0; i < CharacterManager.CurrentInstance.CharacterList.Count; i++)
            {
                var currentLpCharacter = CharacterLpList[i];
                dataView[i, 0] = CharacterManager.CurrentInstance.CharacterList[i].CharacterName;
                for (int j = 0; j < corpIds.Count; j++)
                {
                    dataView[i, j + 1] = 0.ToString();
                    foreach (var loyaltyPoint in currentLpCharacter)
                    {
                        if (corpIds[j] == loyaltyPoint.CorporationId)
                        {
                            dataView[i, j + 1 ] = loyaltyPoint.Points.ToString();
                            break;
                        }
                    }
                }
            }

            Table = new DataTable();
            Table.Columns.Add(new DataColumn("Names"));
            foreach (var corporation in corpNames)
            {
                Table.Columns.Add(new DataColumn(corporation.Name));
            }

            for (int i = 0; i < dataView.GetLength(0); i++)
            {
                var row = Table.NewRow();
                for (int j = 0; j < dataView.GetLength(1); j++)
                {
                    row[j] = dataView[i, j];
                }

                Table.Rows.Add(row);
            }

            DataGrid.ItemsSource = Table.DefaultView;


            // Add DataGrid Columns
            /*DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Name" });
            foreach (var corporation in corpNames)
            {
                var column = new DataGridTextColumn() {Header = corporation.Name};
                DataGrid.Columns.Add(column);
            }
            */

        }
    }
}
