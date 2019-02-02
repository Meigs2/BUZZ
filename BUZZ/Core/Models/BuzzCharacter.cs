using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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

        public BuzzCharacter()
        {

        }

        #region Methods

        public CharacterLocation GetLocation()
        {
            var task = Task.Run(() => EsiData.EsiClient.Location.GetCharacterLocationV1Async(new AuthDTO()
            {
                AccessToken = AccessTokenDetails,
                CharacterId = CharacterDetails.CharacterId,
                Scopes = EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_LOCATION_1
            }));

            return task.Result.Model;
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

        public List<LoyaltyPoints> GetLoyaltyPoints()
        {
            var task = Task.Run(() => EsiData.EsiClient.Loyalty.GetLoyaltyPointsV1Async(new AuthDTO()
            {
                AccessToken = AccessTokenDetails,
                CharacterId = CharacterDetails.CharacterId,
                Scopes = EVEStandard.Enumerations.Scopes.ESI_CHARACTERS_READ_LOYALTY_1
            }));

            return task.Result.Model;
        }

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
