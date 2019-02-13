using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BUZZ.Core.Models;
using BUZZ.Utilities;
using Color = System.Drawing.Color;

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
                if (IsVisible)
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

        public string CurrentSolarSystem
        {
            get => currentSolarSystem;
            set
            {
                currentSolarSystem = value;
                OnPropertyChanged();
            }
        }

        public PullerViewModel(BuzzCharacter buzzCharacter)
        {
            Character = buzzCharacter;
            Character.SystemInformationUpdated += CharacterSystemInformationUpdated;
            Character.OnlineStatusChanged += Character_OnlineStatusChanged;

            if (Properties.Settings.Default.AlwaysShowMultiboxingControls == true)
                IsOnline = true;

        }

        private void Character_OnlineStatusChanged(object sender, Models.Events.OnlineStatusUpdatedEventArgs e)
        {
            if (Properties.Settings.Default.AlwaysShowMultiboxingControls == true)
            {
                IsVisible = true;
                IsOnline = e.IsOnline;
                return;
            }

            IsOnline = e.IsOnline;
        }

        private void CharacterSystemInformationUpdated(object sender, Models.Events.SystemUpdatedEventArgs e)
        {
            CurrentSolarSystem = e.NewSystemName;
        }

        public void MakePullerActiveWindow()
        {
            var currentProcess = GetCurrentProcess();

            if (currentProcess == null) return;

            WindowHelper.BringProcessToFront(currentProcess);
        }

        private Process GetCurrentProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;

                if (Character.CharacterWindowOverride != string.Empty && process.MainWindowTitle.Contains(Character.CharacterWindowOverride))
                {
                    return process;
                }
                if (process.MainWindowTitle.Contains(Character.CharacterDetails.CharacterName))
                {
                    return process;
                }
            }

            return null;

        }


    }
}
