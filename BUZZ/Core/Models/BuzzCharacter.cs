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
        public int RowNumber { get; set; } = 0;
        public int ColumnNumber { get; set; } = 0;
        public CharacterOnline CharacterOnlineInfo { get; set; } = new CharacterOnline();

        private bool isOnline = false;
        public bool IsOnline
        {
            get
            {
                return isOnline;
            }
            set
            {
                if (isOnline!=value)
                {
                    isOnline = value;
                    OnOnlineStatusChanged(new OnlineStatusChangedEventArgs()
                    {
                        IsOnline = value
                    });
                }
            }
        }

        private SolarSystemModel currentSolarSystem;
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
                if (currentSolarSystem != value)
                {
                    currentSolarSystem = value;
                    OnSystemChanged(new SystemChangedEventArgs()
                    {
                        NewSystemName = value.SystemName, NewSystemId = value.SolarSystemId
                    });
                }
            }
        }

        public string CharacterName
        {
            get
            {
                if (CharacterDetails != null)
                {
                    return CharacterDetails.CharacterName;
                }
                else
                {
                    return string.Empty;
                }
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

        public event EventHandler<OnlineStatusChangedEventArgs> OnlineStatusChanged;
        protected virtual void OnOnlineStatusChanged(OnlineStatusChangedEventArgs e)
        {
            OnlineStatusChanged?.Invoke(this, e);
        }

        public event EventHandler<SystemChangedEventArgs> SystemChanged;
        protected virtual void OnSystemChanged(SystemChangedEventArgs e)
        {
            SystemChanged?.Invoke(this, e);
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

        #region ESI Methods

        public async Task<ESIModelDTO<CharacterOnline>> GetOnlineStatusAsync()
        {
            return await EsiData.EsiClient.Location.GetCharacterOnlineV2Async(new AuthDTO()
            {
                AccessToken = AccessTokenDetails,
                CharacterId = CharacterDetails.CharacterId,
                Scopes = EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_ONLINE_1
            });
        }

        public async Task<ESIModelDTO<CharacterLocation>> GetLocationAsync()
        {
            return await EsiData.EsiClient.Location.GetCharacterLocationV1Async(new AuthDTO()
            {
                AccessToken = AccessTokenDetails,
                CharacterId = CharacterDetails.CharacterId,
                Scopes = EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_LOCATION_1
            });
        }

        public async Task<ESIModelDTO<List<LoyaltyPoints>>> GetLoyaltyPointsAsync()
        {
            return await EsiData.EsiClient.Loyalty.GetLoyaltyPointsV1Async(new AuthDTO()
            {
                AccessToken = AccessTokenDetails,
                CharacterId = CharacterDetails.CharacterId,
                Scopes = EVEStandard.Enumerations.Scopes.ESI_CHARACTERS_READ_LOYALTY_1
            });
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
                throw;
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
