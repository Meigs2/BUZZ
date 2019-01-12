using System;
using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models.SSO;

namespace BUZZ.Data
{
    internal class EsiData
    {
        public static EVEStandardAPI EsiClient = new EVEStandardAPI(
            "BUZZ",
            DataSource.Tranquility,
            TimeSpan.FromSeconds(30),
            "https://meigs2.github.io/ESICallback/",
            "a8c4bd8f30444c65b2c68d0eb886c545",
            null,
            SSOVersion.v2,
            SSOMode.Native
        );
    }
}
