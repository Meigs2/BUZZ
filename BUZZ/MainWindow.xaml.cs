using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BUZZ.Core;
using BUZZ.Core.CharacterManagement;
using BUZZ.Core.LPManager;
using BUZZ.Core.Multiboxing;
using BUZZ.Core.Settings;
using BUZZ.Properties;
using BUZZ.Utilities;
using mrousavy;

namespace BUZZ
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Startup.PerformStartupActions();
            Closing += MainWindow_Closing;
            Topmost = Settings.Default.AlwaysOnTop;

            SetupWindowPosition();
        }

        private void SetupWindowPosition()
        {
            if (Settings.Default.MainWindowTop
                + Settings.Default.MainWindowLeft
                + Settings.Default.MainWindowHeight
                + Settings.Default.MainWindowWidth <= 0.0)
            {
                Settings.Default.MainWindowTop = Top;
                Settings.Default.MainWindowLeft = Left;
                Settings.Default.MainWindowHeight = Height;
                Settings.Default.MainWindowWidth = Width;
            }

            BringIntoView();

            Top = Settings.Default.MainWindowTop;
            Left = Settings.Default.MainWindowLeft;
            Height = Settings.Default.MainWindowHeight;
            Width = Settings.Default.MainWindowWidth;

            Settings.Default.Save();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.MainWindowTop = Top;
            Settings.Default.MainWindowLeft = Left;
            Settings.Default.MainWindowHeight = Height;
            Settings.Default.MainWindowWidth = Width;

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

        private void LoadMenuItem_Click(object sender, RoutedEventArgs e)
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
                pullerList.Add(new PullerView(buzzCharacter, this));
            }

            var pullerGrid = new PullerContainer();
            MainGrid.Children.Add(pullerGrid);
            pullerGrid.LoadUserControlsToGrid(pullerList);
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