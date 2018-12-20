using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVEStandard;

namespace BUZZ.Utilities
{
    internal class EsiData
    {
        public static EVEStandardAPI EsiClient = new EVEStandardAPI(
            "BUZZ",
            EVEStandard.Enumerations.DataSource.Tranquility,
            TimeSpan.FromSeconds(30),
            "https://meigs2.github.io/ESICallback/",
            "a8c4bd8f30444c65b2c68d0eb886c545"
        );
    }
}
