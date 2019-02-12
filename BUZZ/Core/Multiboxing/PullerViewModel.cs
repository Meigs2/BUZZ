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
using Color = System.Drawing.Color;

namespace BUZZ.Core.Multiboxing
{
    public class PullerViewModel : ViewModelBase
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

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

            var handle = currentProcess.MainWindowHandle;
            ShowWindow(handle,ShowWindowCommands.ShowMaximized);
            SetForegroundWindow(handle);
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

        #region dll enums


        private enum ShowWindowCommands : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized
            /// or maximized, the system restores it to its original size and
            /// position. An application should specify this flag when displaying
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position.
            /// This value is similar to "ShowNormal", except the window is not
            /// actived.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size
            /// and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This
            /// value is similar to "Show", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is
            /// minimized or maximized, the system restores it to its original size
            /// and position. An application should specify this flag when restoring
            /// a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread
            /// that owns the window is hung. This flag should only be used when
            /// minimizing windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }

        #endregion
    }
}
