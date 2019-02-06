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

        private bool isOnline = false;
        public bool IsOnline
        {
            get { return isOnline;}
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
                IsOnline = true;
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
