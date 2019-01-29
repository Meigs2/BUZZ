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
using System.Windows.Shapes;
using BUZZ.Core.CharacterManagement;
using EVEStandard.Models;
using EVEStandard.Models.API;

namespace BUZZ.Core.LPManager
{
    /// <summary>
    /// Interaction logic for LpViewer.xaml
    /// </summary>
    public partial class LpViewer : Window
    {
        public List<List<LoyaltyPoints>> CharacterLoyalty { get; set; } = new List<List<LoyaltyPoints>>();

        /// <summary>
        /// Class simply displays all current 
        /// </summary>
        public LpViewer()
        {
            InitializeComponent();
        }

        private List<List<LoyaltyPoints>> LoadLp()
        {
            var list = new List<List<LoyaltyPoints>>();
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                var model = buzzCharacter.GetLoyaltyPointsAsync();
                list.Add(model);
            }
            return list;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CharacterLoyalty = LoadLp();
        }
    }
}
