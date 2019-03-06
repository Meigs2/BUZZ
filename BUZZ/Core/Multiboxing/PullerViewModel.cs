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
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BuzzCharacter Character { get; set; }

        public List<int> WaypointSystems { get; set; } = new List<int>();

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
            Character.CharacterInformationUpdated += Character_CharacterInformationUpdated;

            if (Properties.Settings.Default.AlwaysShowMultiboxingControls == true)
                IsOnline = true;

        }

        private void Character_CharacterInformationUpdated(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UseThumbnailPreviews && IsOnline == true)
            {
                CurrentThumbnailWindowHandle = this.GetCurrentEveWindowPointer();
            }

            if (Properties.Settings.Default.UseThumbnailPreviews && IsOnline == false)
            {
                UnregesterCurrentThumbnail();
            }
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
            var currentEveClient = GetCurrentEveProcess();

            if (currentEveClient == null) return;

            WindowHelper.BringProcessToFront(currentEveClient);
        }

        // I'm not sure why, but the pointer returned by GetCurrentEveProcess() is
        // different than the one returned by using the user32.dll method, so we must use
        // this to get the correct pointer to be used by CurrentThumbnailWindowHandle
        private IntPtr GetCurrentEveWindowPointer()
        {
            var availableWindows = new ObservableCollection<KeyValuePair<string, IntPtr>>();
            DwmClass.EnumWindows((hwnd, e) =>
            {
                if (_targetRenderWindowHandle != hwnd && (DwmClass.GetWindowLongA(hwnd, DwmClass.GWL_STYLE) & DwmClass.TargetWindow) == DwmClass.TargetWindow)
                {
                    var sb = new StringBuilder(200);
                    DwmClass.GetWindowText(hwnd, sb, sb.Capacity);

                    var text = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(text))
                        availableWindows.Add(new KeyValuePair<string, IntPtr>(text, hwnd));
                }

                return true;
            }, 0);

            // find the IntPtr of our current client, or return a null pointer.
            foreach (var availableWindow in availableWindows)
            {
                if (availableWindow.Key.Contains(Character.CharacterName) || 
                    (Character.WindowOverride!=string.Empty && availableWindow.Key.Contains(Character.WindowOverride)))
                {
                    return availableWindow.Value;
                }
            }
            return IntPtr.Zero;
        }

        private Process GetCurrentEveProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;

                if (process.MainWindowTitle.Contains(Character.CharacterName) || 
                    (Character.WindowOverride!=string.Empty && process.MainWindowTitle.Contains(Character.WindowOverride)))
                {
                    return process;
                }
            }

            return null;
        }

        #region DWM Thumbnails region

        private IntPtr _targetRenderWindowHandle, _thumbnailStreamHandle;
        private DwmClass.Rect _targetRenderArea;

        private IntPtr _currentThumbnailWindowHandle = IntPtr.Zero;
        public IntPtr CurrentThumbnailWindowHandle {

            get => _currentThumbnailWindowHandle;
            set
            {
                if (value == IntPtr.Zero) return;

                _currentThumbnailWindowHandle = value;

                // Un-register old _thumbnailStreamHandle
                if (_thumbnailStreamHandle != IntPtr.Zero)
                {
                    DwmClass.DwmUnregisterThumbnail(_thumbnailStreamHandle);
                }

                // Register the new thumbnail with DWM
                if (DwmClass.DwmRegisterThumbnail(_targetRenderWindowHandle, CurrentThumbnailWindowHandle,
                        out _thumbnailStreamHandle) == 0)
                    UpdateThumbnail();

                OnPropertyChanged();
            }
        }

        public void ClearThumbnailRegisters()
        {
            DwmClass.DwmUnregisterThumbnail(_thumbnailStreamHandle);
            _targetRenderWindowHandle = IntPtr.Zero;
            _thumbnailStreamHandle = IntPtr.Zero;
            _currentThumbnailWindowHandle = IntPtr.Zero;
            _targetRenderArea = new DwmClass.Rect();
        }

        public void UnregesterCurrentThumbnail()
        {
            if (_thumbnailStreamHandle != IntPtr.Zero)
            {
                DwmClass.DwmUnregisterThumbnail(_thumbnailStreamHandle);
            }
        }

        public void InitializeThumbnailInfo(IntPtr Target, DwmClass.Rect renderLocation)
        {
            _targetRenderWindowHandle = Target;
            _targetRenderArea = renderLocation;
        }

        private void UpdateThumbnail()
        {
            if (_thumbnailStreamHandle == IntPtr.Zero) return;

            DwmClass.DwmQueryThumbnailSourceSize(_thumbnailStreamHandle, out DwmClass.Psize size);

            byte opacity = (Properties.Settings.Default.ThumbnailOpacity >= 1.0? (byte)255: 
                (Properties.Settings.Default.ThumbnailOpacity <= 0.0? (byte)0 : (byte)Math.Floor(Properties.Settings.Default.ThumbnailOpacity*256.0)));

            var thumbnailProperties = new DwmClass.DwmThumbnailProperties
            {
                fVisible = true,
                dwFlags = DwmClass.DwmTnpVisible | DwmClass.DwmTnpRectdestination | DwmClass.DwmTnpOpacity,
                opacity = opacity,
                rcDestination = _targetRenderArea
            };

            if (size.x < _targetRenderArea.Width)
                thumbnailProperties.rcDestination.Right = thumbnailProperties.rcDestination.Left + size.x;

            if (size.y < _targetRenderArea.Height)
                thumbnailProperties.rcDestination.Bottom = thumbnailProperties.rcDestination.Top + size.y;

            DwmClass.DwmUpdateThumbnailProperties(_thumbnailStreamHandle, ref thumbnailProperties);
        }

        public void ClientSizeChanged(DwmClass.Rect RenderArea)
        {
            _targetRenderArea = RenderArea;
            UpdateThumbnail();
        }

        #endregion

        /// <summary>
        /// This function takes data from the Agents tab and plots an optimized route.
        /// </summary>
        public async Task OptimizeRouteFromClipboard()
        {
            List<int> systemsList = new List<int>();

            // Extract our systems from the clipboard.  
            try
            {
                var clipboardText = Clipboard.GetText();
                var lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                Log.Info("Systems Parsed from Clipboard:");
                foreach (var line in lines)
                {
                    var result = line.Split(new[] { "\t" }, StringSplitOptions.None);
                    if (result[0].Contains("Agent Home Base"))
                        continue;

                    Log.Info(result[3]);

                    systemsList.Add(SolarSystems.GetSolarSystemId(result[3]));
                }

                if (systemsList.Count == 0)
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                Console.WriteLine(exception);
                MessageBox.Show("Error parsing clipboard contents, make sure you copy from your people and places.");
                return;
            }

            // Set search node to be our current system
            systemsList.Insert(0, Character.CurrentSolarSystem.SolarSystemId);

            // Optimize our route
            var optimized = await SolarSystems.OptimizeRouteAsync(systemsList,
                Properties.Settings.Default.UseDestinationSystem ? Properties.Settings.Default.DestinationSystem : 0);

            Log.Info("Optimized Systems (including starting system)");
            foreach (var i in optimized)
            {
                Log.Info(i);
            }

            // (remove first element, as we're already here)
            optimized.RemoveAt(0);

            foreach (var waypoint in optimized)
            {
                WaypointSystems.Add(waypoint);
            }
            Log.Info("Setting waypoints for " + Character.CharacterName);
            await Character.SetWaypoints(optimized,true);
        }
    }
}
