using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BUZZ.Core.Models;
using BUZZ.Core.Thumbnails;
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
            var currentEveClient = GetCurrentEveClient();

            if (currentEveClient == null) return;

            WindowHelper.BringProcessToFront(currentEveClient);

            if (Properties.Settings.Default.UseThumbnailPreviews != true) return;

            CurrentThumbnailTargetWindow = GetCurrentClientWindowPointer();
        }

        // I'm not sure why, but the pointer returned by GetCurrentEveClient() is
        // different than the one returned by using the user32.dll method, so we must use
        // this to get the correct pointer to be used by CurrentThumbnailTargetWindow
        private IntPtr GetCurrentClientWindowPointer()
        {
            var availableWindows = new ObservableCollection<KeyValuePair<string, IntPtr>>();
            DwmClass.EnumWindows((hwnd, e) =>
            {
                if (_targetWindowHandle != hwnd && (DwmClass.GetWindowLongA(hwnd, DwmClass.GWL_STYLE) & DwmClass.Targetwindow) == DwmClass.Targetwindow)
                {
                    var sb = new StringBuilder(100);
                    DwmClass.GetWindowText(hwnd, sb, sb.Capacity);

                    var text = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(text))
                        availableWindows.Add(new KeyValuePair<string, IntPtr>(text, hwnd));
                }

                return true;
            }, 0);

            // find the IntPtr of our current client
            foreach (var availableWindow in availableWindows)
            {
                if (availableWindow.Key.Contains(Character.CharacterName))
                {
                    return availableWindow.Value;
                }
            }
            return IntPtr.Zero;
        }

        private Process GetCurrentEveClient()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;

                if (process.MainWindowTitle.Contains(Character.CharacterDetails.CharacterName))
                {
                    return process;
                }
            }

            return null;

        }

        #region DWM Thumbnails region

        private IntPtr _targetWindowHandle, _thumbnailHandle;
        private DwmClass.Rect _targetRenderAreaRectangle;

        private IntPtr _currentThumbnailTargetWindow = IntPtr.Zero;
        public IntPtr CurrentThumbnailTargetWindow {
            get
            {
                return _currentThumbnailTargetWindow;
            }
            set
            {
                if (value == IntPtr.Zero)
                {
                     return;
                }

                _currentThumbnailTargetWindow = value;

                if (_thumbnailHandle != IntPtr.Zero)
                {
                    DwmClass.DwmUnregisterThumbnail(_thumbnailHandle);
                }

                //register the thumbnail with DWM
                if (DwmClass.DwmRegisterThumbnail(_targetWindowHandle, CurrentThumbnailTargetWindow, out _thumbnailHandle) == 0)
                    UpdateThumbnail();

                OnPropertyChanged();
            }
        }

        public void UnregesterCurrentThumbnail()
        {
            if (_thumbnailHandle != IntPtr.Zero)
            {
                DwmClass.DwmUnregisterThumbnail(_thumbnailHandle);
            }
        }

        public void InitializeThumbnailInfo(IntPtr Target, DwmClass.Rect renderLocation)
        {
            _targetWindowHandle = Target;
            _targetRenderAreaRectangle = renderLocation;
        }

        private void UpdateThumbnail()
        {
            if (_thumbnailHandle == IntPtr.Zero)
            {
                return;
            }

            DwmClass.DwmQueryThumbnailSourceSize(_thumbnailHandle, out DwmClass.Psize size);

            var thumbnailProperties = new DwmClass.DwmThumbnailProperties
            {
                fVisible = true,
                dwFlags = DwmClass.DwmTnpVisible | DwmClass.DwmTnpRectdestination | DwmClass.DwmTnpOpacity,
                opacity = 255,
                rcDestination = _targetRenderAreaRectangle
            };

            if (size.x < _targetRenderAreaRectangle.Width)
                thumbnailProperties.rcDestination.Right = thumbnailProperties.rcDestination.Left + size.x;

            if (size.y < _targetRenderAreaRectangle.Height)
                thumbnailProperties.rcDestination.Bottom = thumbnailProperties.rcDestination.Top + size.y;

            DwmClass.DwmUpdateThumbnailProperties(_thumbnailHandle, ref thumbnailProperties);
        }

        #endregion

        public void ClientSizeChanged(DwmClass.Rect RenderArea)
        {
            _targetRenderAreaRectangle = RenderArea;
            UpdateThumbnail();
        }
    }
}
