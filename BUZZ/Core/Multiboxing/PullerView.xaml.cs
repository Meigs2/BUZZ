using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BUZZ.Core.Models;
using BUZZ.Core.Thumbnails;

namespace BUZZ.Core.Multiboxing
{
    /// <summary>
    /// Interaction logic for PullerView.xaml
    /// </summary>
    public partial class PullerView : UserControl
    {
        public readonly PullerViewModel CurrentViewModel;
        private Window ParentWindow;

        public PullerView(BuzzCharacter buzzCharacter, Window parentWindow)
        {
            InitializeComponent();
            CurrentViewModel = new PullerViewModel(buzzCharacter);
            this.DataContext = CurrentViewModel;
            buzzCharacter.SystemInformationUpdated += BuzzCharacterSystemInformationUpdated;

            ParentWindow = parentWindow;

            /*
            // Set up things for thumbnail rendering. We must get the render area relative to the
            // parent window, so we need to encapsulate our code in the loaded event to prevent Window.GetWindow(this)
            // from returning null. I'm not quite sure why it behaves this way! /shrug
            this.Loaded += (sender, args) => {

                // get UserControl location relative to the parent window
                Point CurrentRelativePoint = this.TransformToAncestor(Window.GetWindow(this))
                    .Transform(new Point(0, 0));
            };
            */

            SizeChanged += (s, e) => { CurrentViewModel.ClientSizeChanged(GetRenderArea()); };
        }

        private void BuzzCharacterSystemInformationUpdated(object sender, Models.Events.SystemUpdatedEventArgs e)
        {
            if (e.OldSystemName == e.NewSystemName) return;

            AnimateBackground(BackgroundGrid, Colors.ForestGreen,Colors.Transparent, TimeSpan.FromSeconds(30),.75);
        }

        private void BringCharacterToForeground(object sender, MouseButtonEventArgs e)
        {
            CurrentViewModel.MakePullerActiveWindow();

            var renderArea = GetRenderArea();

            CurrentViewModel.InitializeThumbnailInfo(new WindowInteropHelper(Window.GetWindow(this)).Handle, renderArea);
        }

        public DwmClass.Rect GetRenderArea()
        {
            Point currentControlOffset = this.TranslatePoint(new Point(0, 0), ParentWindow);

            return new DwmClass.Rect()
            {
                Left = (int)currentControlOffset.X + (int)ViewGrid.Margin.Left,
                Top = (int)currentControlOffset.Y + (int)ViewGrid.Margin.Top + (1/3)*(int)InnerGrid.ActualHeight,
                Right = (int)currentControlOffset.X + (int)this.ActualWidth - (int)ViewGrid.Margin.Right,
                Bottom = (int)currentControlOffset.Y + (int)this.ActualHeight - (int)ViewGrid.Margin.Bottom
            };
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
