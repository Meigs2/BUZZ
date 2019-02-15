using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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
        public string CharacterWindowOverride { get; set; } = string.Empty;

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
            }
            catch (Exception e)
            {
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
                Console.WriteLine(e);
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
                Console.WriteLine(e);
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
                Console.WriteLine(e);
                return new ESIModelDTO<List<LoyaltyPoints>>();
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
