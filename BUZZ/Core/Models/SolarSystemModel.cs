using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUZZ.Core.Models
{
    public class SolarSystemModel
    {
        public int SolarSystemId { get; set; }
        public int? StationId { get; set; }
        public long? StructureId { get; set; }
        public string SolarSystemName { get; set; } = string.Empty;
    }
}
