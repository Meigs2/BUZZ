using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUZZ.Core.LogReading
{
    public class LogFile
    {
        public string LogPath { get; set; }
        public long CurrentFileLength { get; set; }
    }
}
