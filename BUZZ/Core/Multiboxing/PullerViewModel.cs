using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core.Models;

namespace BUZZ.Core.Multiboxing
{
    public class PullerViewModel : ViewModelBase
    {
        public BuzzCharacter Character { get; set; }

        // Acts as a override in case we dont want to show the UserControl, but still want to know if the character is
        // online.
        public bool IsVisible { get; set; }

        // The visibility of the grid is bound to IsOnline, however IsVisible can override this.
        // We could also have a IsHidden, however I dont believe this will be of much use, and can be added later.
        private bool isOnline = false;
        public bool IsOnline
        {
            get
            {
                if (IsVisible == true)
                {
                    return true;
                }
                return isOnline;
            }
            set
            {
                isOnline = value;
                OnPropertyChanged();
            }
        }

        private string currentSolarSystem = string.Empty;
        public string CurrentSolarSystem {
            get { return currentSolarSystem; }
            set
            {
                currentSolarSystem = value;
                OnPropertyChanged();
            }
        }

        public PullerViewModel(BuzzCharacter buzzCharacter)
        {
            Character = buzzCharacter;
            Character.SystemChanged += Character_SystemChanged;
            Character.OnlineStatusChanged += Character_OnlineStatusChanged;

            if (Properties.Settings.Default.AlwaysShowMultiboxingControls == true)
                IsOnline = true;

        }

        private void Character_OnlineStatusChanged(object sender, Models.Events.OnlineStatusChangedEventArgs e)
        {
            if (Properties.Settings.Default.AlwaysShowMultiboxingControls == true)
            {
                IsVisible = true;
                IsOnline = e.IsOnline;
                return;
            }
            IsOnline = e.IsOnline;
        }

        private void Character_SystemChanged(object sender, Models.Events.SystemChangedEventArgs e)
        {
            CurrentSolarSystem = e.NewSystemName;
        }
    }
}
