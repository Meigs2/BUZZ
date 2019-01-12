using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EVEStandard.Models.SSO;

namespace BUZZ.Data.Models
{
    [Serializable]
    public class BuzzCharacter
    {
        #region Properties
        
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

        #endregion 

        public BuzzCharacter()
        {

        }

        #region Methods

        public int GetLocation()
        {
            return -1;
        }

        #endregion
    }
}
