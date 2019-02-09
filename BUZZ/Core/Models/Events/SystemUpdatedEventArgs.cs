using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUZZ.Core.Models.Events
{
    public class SystemUpdatedEventArgs
    {
        public int OldSystemId { get; set; }
        public string OldSystemName { get; set; }

        public int NewSystemId { get; set; }
        public string NewSystemName { get; set; }
    }
}
