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
        }

        private void Character_SystemChanged(object sender, Models.Events.SystemChangedEventArgs e)
        {
            CurrentSolarSystem = e.NewSystemName;
        }
    }
}
