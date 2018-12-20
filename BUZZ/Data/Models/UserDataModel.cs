using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVEStandard.API;

namespace BUZZ.Data.Models
{
    /// <summary>
    /// This Model contains everything that the application will need to store for re-use upon the next sessions of the application.
    /// </summary>
    [Serializable]
    internal class UserDataModel
    {
        // Token Details
        internal string AccessToken { get; set; } = string.Empty;

        internal string TokenType { get; set; } = string.Empty;

        internal int ExpiresIn { get; set; } = 0;

        internal string RefreshToken { get; set; } = string.Empty;


    }
}
