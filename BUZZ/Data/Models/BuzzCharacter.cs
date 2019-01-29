using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EVEStandard.Models;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;

namespace BUZZ.Data.Models
{
    public class BuzzCharacter
    {
        #region Properties
        
        public bool IsEnabled { get; set; } = true;
        public AccessTokenDetails AccessTokenDetails { get; set; } = new AccessTokenDetails();
        public CharacterDetails CharacterDetails { get; set; } = new CharacterDetails();

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

        public List<LoyaltyPoints> GetLoyaltyPointsAsync()
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
}
