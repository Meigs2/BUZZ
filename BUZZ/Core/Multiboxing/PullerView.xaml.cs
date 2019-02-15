using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            AnimateBackground(BackgroundGrid, Colors.LightGreen,Colors.Transparent, TimeSpan.FromSeconds(30),.5);
        }

        private void BringCharacterToForeground(object sender, MouseButtonEventArgs e)
        {
            CurrentViewModel.MakePullerActiveWindow();
        }

        public void AnimateBackground(Panel targetPanel, Color fromColor, Color toColor, TimeSpan duration, double startingOpacity)
        {
            ColorAnimation colorAnimation = new ColorAnimation(toColor, duration);
            targetPanel.Opacity = startingOpacity;
            targetPanel.Background = new SolidColorBrush(fromColor);
            targetPanel.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }
    }
}
