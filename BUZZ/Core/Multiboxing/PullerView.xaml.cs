using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BUZZ.Core.Models;

namespace BUZZ.Core.Multiboxing
{
    /// <summary>
    /// Interaction logic for PullerView.xaml
    /// </summary>
    public partial class PullerView : UserControl
    {
        public readonly PullerViewModel CurrentViewModel;

        public PullerView(BuzzCharacter buzzCharacter)
        {
            InitializeComponent();
            CurrentViewModel = new PullerViewModel(buzzCharacter);
            this.DataContext = CurrentViewModel;
            buzzCharacter.SystemInformationUpdated += BuzzCharacterSystemInformationUpdated;
        }

        private void BuzzCharacterSystemInformationUpdated(object sender, Models.Events.SystemUpdatedEventArgs e)
        {
            if (e.OldSystemName == e.NewSystemName) return;

            ColorAnimation colorAnimation = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromSeconds(30)));
            ViewGrid.Background = new SolidColorBrush(Colors.LightGreen);
            ViewGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }
    }
}
