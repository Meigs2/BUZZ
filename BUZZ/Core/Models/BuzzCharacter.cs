using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BUZZ.Core.Models.Events;
using BUZZ.Data;
using BUZZ.Utilities;
using EVEStandard.API;
using EVEStandard.Models;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using mrousavy;

namespace BUZZ.Core.Models
{
    public class BuzzCharacter
    {
        #region Properties

        public bool IsEnabled { get; set; } = true;
        public AccessTokenDetails AccessTokenDetails { get; set; } = new AccessTokenDetails();
        public CharacterDetails CharacterDetails { get; set; } = new CharacterDetails();
        public CharType CharacterType { get; set; } = CharType.Normal;
        public int RowNumber { get; set; } = 1;
        public int ColumnNumber { get; set; } = 1;
        public int AccountNumber { get; set; } = 1;
        public CharacterOnline CharacterOnlineInfo { get; set; } = new CharacterOnline();
        public string WindowOverride { get; set; } = string.Empty;
        public List<Key> FocusKeysList { get; set; } = new List<Key>();
        public List<ModifierKeys> FocusModifierKeysList { get; set; } = new List<ModifierKeys>();
        public HotKey ActiveHotKey;

        public string KeybindString
        {
            get
            {
                string s = string.Empty;
                foreach (var modifierKeyse in FocusModifierKeysList)
                {
                    s += modifierKeyse;
                }

                foreach (var key in FocusKeysList)
                {
                    s += key;
                }
                return s;
            }
        }

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Process currentProcess;

        private bool isOnline = false;
        public bool IsOnline
        {
            get => isOnline;
            set
            {
                isOnline = value;
                OnOnlineStatusUpdated(new OnlineStatusUpdatedEventArgs()
                {
                    IsOnline = value
                });
            }
        }

        private SolarSystemModel currentSolarSystem = new SolarSystemModel();
        public SolarSystemModel CurrentSolarSystem {
            get
            {
                if (currentSolarSystem == null)
                {
                    currentSolarSystem = new SolarSystemModel();
                }
                return currentSolarSystem;
            }
            set
            {
                OnSystemInformationUpdated(new SystemUpdatedEventArgs()
                {
                    NewSystemName = value.SystemName, NewSystemId = value.SolarSystemId,
                    OldSystemId = currentSolarSystem.SolarSystemId, OldSystemName = currentSolarSystem.SystemName
                });
                currentSolarSystem = value;
            }
        }

        public string CharacterName
        {
            get
            {
                if (CharacterDetails == null)
                {
                    return string.Empty;
                }

                return CharacterDetails.CharacterName;
            }
        }

        public string AccessTokenExpiry
        {
            get
            {
                if (AccessTokenDetails != null)
                {
                    return AccessTokenDetails.ExpiresUtc.ToLocalTime().ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<OnlineStatusUpdatedEventArgs> OnlineStatusChanged;
        protected virtual void OnOnlineStatusUpdated(OnlineStatusUpdatedEventArgs e)
        {
            OnlineStatusChanged?.Invoke(this, e);
        }

        public event EventHandler<SystemUpdatedEventArgs> SystemInformationUpdated;
        protected virtual void OnSystemInformationUpdated(SystemUpdatedEventArgs e)
        {
            SystemInformationUpdated?.Invoke(this, e);
        }

        public event EventHandler CharacterInformationUpdated;
        protected virtual void OnCharacterInformationUpdated()
        {
            CharacterInformationUpdated?.Invoke(this,new EventArgs());
        }

        #endregion

        #region Methods

        public async Task RefreshCharacterInformation()
        {
            try
            {
                var locationResult = await GetLocationAsync();
                var solarSystemModel = new SolarSystemModel()
                {
                    SolarSystemId = locationResult.Model.SolarSystemId,
                    StationId = locationResult.Model.StationId.GetValueOrDefault(),
                    StructureId = locationResult.Model.StationId.GetValueOrDefault(),
                    SystemName = SolarSystems.GetSolarSystemName(locationResult.Model.SolarSystemId)
                };
                var onlineResult = await GetOnlineStatusAsync();

                CharacterOnlineInfo = onlineResult.Model;
                IsOnline = CharacterOnlineInfo.Online;
                // If this is not called, there is an odd thread access error that gets thrown here,
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentSolarSystem = solarSystemModel;
                });
                OnCharacterInformationUpdated();

                // Get current process
                if (IsOnline && currentProcess == null)
                {
                    currentProcess = GetCurrentEveProcess();
                }
                else if (!IsOnline)
                {
                    currentProcess = null;
                }
            }
            catch (Exception e)
            {
                Log.Error("Unable refresh character information for " + CharacterName);
                Log.Error(e);
            }
        }

        #region ESI Methods

        public async Task<ESIModelDTO<CharacterOnline>> GetOnlineStatusAsync()
        {
            try
            {
                return await EsiData.EsiClient.Location.GetCharacterOnlineV2Async(new AuthDTO()
                {
                    AccessToken = AccessTokenDetails,
                    CharacterId = CharacterDetails.CharacterId,
                    Scopes = EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_ONLINE_1
                });
            }
            catch (Exception e)
            {
                Log.Error("Unable to get online status for " + CharacterName);
                Log.Error(e);
                return new ESIModelDTO<CharacterOnline>();
            }
        }

        public async Task<ESIModelDTO<CharacterLocation>> GetLocationAsync()
        {
            try
            {
                return await EsiData.EsiClient.Location.GetCharacterLocationV1Async(new AuthDTO()
                {
                    AccessToken = AccessTokenDetails,
                    CharacterId = CharacterDetails.CharacterId,
                    Scopes = EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_LOCATION_1
                });
            }
            catch (Exception e)
            {
                Log.Error("Unable to get location of " + CharacterName);
                Log.Error(e);
                return new ESIModelDTO<CharacterLocation>();
            }
        }

        public async Task<ESIModelDTO<List<LoyaltyPoints>>> GetLoyaltyPointsAsync()
        {
            try
            {
                return await EsiData.EsiClient.Loyalty.GetLoyaltyPointsV1Async(new AuthDTO()
                {
                    AccessToken = AccessTokenDetails,
                    CharacterId = CharacterDetails.CharacterId,
                    Scopes = EVEStandard.Enumerations.Scopes.ESI_CHARACTERS_READ_LOYALTY_1
                });
            }
            catch (Exception e)
            {
                Log.Error("Unable to get loyalty points for " + CharacterName);
                Log.Error(e);
                return new ESIModelDTO<List<LoyaltyPoints>>();
            }
        }

        public async Task<ESIModelDTO<List<int>>> GetRouteAsync(int origin, int destination)
        {
            try
            {
                return await EsiData.EsiClient.Routes.GetRouteV1Async(origin, destination,null);
            }
            catch (Exception e)
            {
                Log.Error("Unable to return a route for " + CharacterName);
                Log.Error(e);
            }

            return null;
        }

        public async Task SetWaypoints(List<int> systems, bool clearOtherWaypoints)
        {
            try
            {
                var auth = new AuthDTO()
                {
                    AccessToken = AccessTokenDetails,
                    CharacterId = CharacterDetails.CharacterId,
                    Scopes = EVEStandard.Enumerations.Scopes.ESI_UI_WRITE_WAYPOINT_1
                };
                foreach (var system in systems)
                {
                    // We don't want to clear all the other systems every time we add a new one, so we 
                    // check if its true, and set it to false right after.
                    Log.Info(CharacterName + " is setting a waypoint to" + SolarSystems.GetSolarSystemName(system));
                    if (clearOtherWaypoints)
                    {
                        await EsiData.EsiClient.UserInterface.SetAutopilotWaypointV2Async(auth, false, clearOtherWaypoints,
                            system);
                        clearOtherWaypoints = false;
                    }
                    else
                    {
                        await EsiData.EsiClient.UserInterface.SetAutopilotWaypointV2Async(auth, false, clearOtherWaypoints,
                            system);
                    }
                    Log.Info(CharacterName + " successfully set waypoint to " + SolarSystems.GetSolarSystemName(system));
                }
            }
            catch (Exception e)
            {
                Log.Error("Unable to set waypoint(s) for " + CharacterName);
                Log.Error(e);
            }
        }

        #endregion


        public async Task RefreshAuthToken()
        {
            try
            {
                AccessTokenDetails = await EsiData.EsiClient.SSOv2.GetRefreshTokenAsync(AccessTokenDetails.RefreshToken);
                Log.Info(CharacterName + "'s AuthToken has been refreshed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("Unable to refresh Authorization for " + CharacterName);
                Log.Error(e);
            }
        }

        public void RegisterActivateHotkey()
        {
            if (ActiveHotKey != null)
            {
                ActiveHotKey.Dispose();
            }
            var window = Application.Current.MainWindow;
            if (FocusKeysList.Count == 0)
            {
                // unregister hotkey if we have no keys to press.
                ActiveHotKey = new HotKey(ModifierKeys.None, Key.None, window);
            }
            else
            {
                var keys = new Key();
                // Build key
                foreach (var key in FocusKeysList)
                {
                    keys = keys | key;
                }

                var modifierKeys = new ModifierKeys();
                foreach (var mKey in FocusModifierKeysList)
                {
                    modifierKeys = modifierKeys | mKey;
                }

                ActiveHotKey = new HotKey(modifierKeys, keys, window, delegate
                {
                    BringToForeground();
                });
            }
        }

        public void BringToForeground()
        {
            if (currentProcess == null) return;


            WindowHelper.BringProcessToFront(currentProcess);
        }

        private Process GetCurrentEveProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;

                if (process.MainWindowTitle.Contains(CharacterName) ||
                    (CharacterName != string.Empty && process.MainWindowTitle.Contains(CharacterName)))
                {
                    return process;
                }
            }

            return null;
        }

        #endregion
    }

    public enum CharType
    {
        Puller,
        Runner,
        Both,
        Normal,
    }
}
