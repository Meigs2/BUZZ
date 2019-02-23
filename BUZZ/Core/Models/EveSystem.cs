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
        public long SolarSystemId { get; set; }

        [JsonProperty("regionId")]
        public long RegionId { get; set; }  

        [JsonProperty("constellationId")]
        public long ConstellationId { get; set; }

        [JsonProperty("security")]
        public double Security { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

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
        public List<long> Connections { get; set; }
    }
}
