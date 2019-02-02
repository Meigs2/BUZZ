using System;

namespace BUZZ.Core.Models
{
    [Serializable]
    class AuthTokenDetails
    {
        // Token Details
        internal string AccessToken { get; set; } = string.Empty;

        internal string TokenType { get; set; } = string.Empty;

        internal int ExpiresIn { get; set; } = 0;

        internal string RefreshToken { get; set; } = string.Empty;
    }
}
