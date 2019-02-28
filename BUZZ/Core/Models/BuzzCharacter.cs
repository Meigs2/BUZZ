using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using BUZZ.Core.Models.Events;
using BUZZ.Data;
using EVEStandard.Models;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;

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

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public BuzzCharacter()
        {

        }

        #region Methods

        /// <summary>
        /// Updates 
        /// </summary>
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
                    SystemName = Utilities.SolarSystems.GetSolarSystemName(locationResult.Model.SolarSystemId)
                };
                CurrentSolarSystem = solarSystemModel;

                var onlineResult = await GetOnlineStatusAsync();
                CharacterOnlineInfo = onlineResult.Model;
                IsOnline = CharacterOnlineInfo.Online;

                OnCharacterInformationUpdated();
            }
            catch (Exception e)
            {
                log.Fatal("Unable refresh character information for " + CharacterName);
                log.Error(e);
                Console.WriteLine(e);
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
                log.Error("Unable to get online status for " + CharacterName);
                log.Error(e);
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
                log.Error("Unable to get location of " + CharacterName);
                log.Error(e);
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
                log.Error("Unable to get loyalty points for " + CharacterName);
                log.Error(e);
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
                log.Error("Unable to return a route for " + CharacterName);
                log.Error(e);
            }

            return null;
        }

        public async void SetWaypoints(List<int> systems, bool clearOtherWaypoints)
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
                }
            }
            catch (Exception e)
            {
                log.Error("Unable to set waypoints for " + CharacterName);
                log.Error(e);
            }
        }

        #endregion


        public async Task RefreshAuthToken()
        {
            try
            {
                AccessTokenDetails = await EsiData.EsiClient.SSOv2.GetRefreshTokenAsync(AccessTokenDetails.RefreshToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                log.Error("Unable to refresh Authorization for " + CharacterName);
                log.Error(e);
            }
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
