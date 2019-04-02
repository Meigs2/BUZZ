using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUZZ.Core.LogReading
{
    public class SystemChangedEventArgs : EventArgs
    {
        public string NewSystemName { get; set; }
        public int NewSystemId { get; set; }
        public string Listener { get; set; }
    }
}
