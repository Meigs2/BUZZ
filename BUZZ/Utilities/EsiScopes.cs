using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUZZ.Utilities
{
    public class EsiScopes
    {
        public static List<string> Scopes = new List<string>()
        {
            EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_LOCATION_1,
            EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_SHIP_TYPE_1,
            EVEStandard.Enumerations.Scopes.ESI_LOCATION_READ_ONLINE_1,
            EVEStandard.Enumerations.Scopes.ESI_UI_WRITE_WAYPOINT_1
        };
    }
}
