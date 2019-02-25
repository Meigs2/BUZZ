using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using BUZZ.Core;
using BUZZ.Core.CharacterManagement;
using BUZZ.Core.LPManager;
using BUZZ.Core.Models;
using BUZZ.Core.Multiboxing;
using BUZZ.Core.Settings;
using BUZZ.Properties;
using BUZZ.Utilities;
using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models.API;

namespace BUZZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Utilities.Startup.PerformStartupActions();
            this.Closing += MainWindow_Closing;
            Topmost = Settings.Default.AlwaysOnTop;

            SetupWindowPosition();
        }

        private void SetupWindowPosition()
        {
            if ((Settings.Default.MainWindowTop
                 + Settings.Default.MainWindowLeft
                 + Settings.Default.MainWindowHeight
                 + Settings.Default.MainWindowWidth) <= 0.0)
            {
                Settings.Default.MainWindowTop = this.Top;
                Settings.Default.MainWindowLeft = this.Left;
                Settings.Default.MainWindowHeight = this.Height;
                Settings.Default.MainWindowWidth = this.Width;
            }

            this.BringIntoView();

            this.Top = Settings.Default.MainWindowTop;
            this.Left = Settings.Default.MainWindowLeft;
            this.Height = Settings.Default.MainWindowHeight;
            this.Width = Settings.Default.MainWindowWidth;

            Settings.Default.Save();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.MainWindowTop = this.Top;
            Settings.Default.MainWindowLeft = this.Left;
            Settings.Default.MainWindowHeight = this.Height;
            Settings.Default.MainWindowWidth = this.Width;

            Settings.Default.Save();
        }

        private void MenuItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var manager = new CharacterManagementWindow();
            manager.ShowDialog();
        }

        private void CharacterLPViewer_Click(object sender, RoutedEventArgs e)
        {
            var lpViewer = new LpViewer();
            lpViewer.Show();
        }

        private async void LoadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mainGridChild in MainGrid.Children.OfType<PullerContainer>())
            {
                foreach (var child in mainGridChild.MultiboxingGrid.Children.OfType<PullerView>())
                {
                    child.CurrentViewModel.ClearThumbnailRegisters();
                }
            }

            MainGrid.Children.Clear();
            var pullerList = new List<PullerView>();

            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                pullerList.Add(new PullerView(buzzCharacter,this));
            }

            var pullerGrid = new PullerContainer();
            MainGrid.Children.Add(pullerGrid);
            pullerGrid.LoadUserControlsToGrid(pullerList);

            await SolarSystems.OptimizeRouteAsync(new List<int>()
            {
                30001279,
                30001274,
                30001272
            }, 30001277);
        }

        private void SettingsMenuItem_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void OpenDestinationMenu(object sender, MouseButtonEventArgs e)
        {
            var destinationWindow = new DestinationManager();
            destinationWindow.ShowDialog();
        }

        private void AlwaysOnTopCheckbox_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Topmost = Settings.Default.AlwaysOnTop;
        }
    }
}
