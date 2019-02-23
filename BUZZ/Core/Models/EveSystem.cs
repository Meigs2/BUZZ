using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EVEMapBuilder
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EveSystem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("solarSystemId")]
        public int SolarSystemId { get; set; }

        [JsonProperty("regionId")]
        public int RegionId { get; set; }  

        [JsonProperty("constellationId")]
        public int ConstellationId { get; set; }

        [JsonProperty("security")]
        public double Security { get; set; }

        [JsonProperty("border")]
        public bool Border { get; set; }

        [JsonProperty("fringe")]
        public bool Fringe { get; set; }

        [JsonProperty("corridor")]
        public bool Corridor { get; set; }

        [JsonProperty("hub")]
        public bool Hub { get; set; }

        [JsonProperty("international")]
        public bool International { get; set; }

        [JsonProperty("regional")]
        public bool Regional { get; set; }

        [JsonProperty("connections")]
        public List<int> Connections { get; set; }
    }
}
