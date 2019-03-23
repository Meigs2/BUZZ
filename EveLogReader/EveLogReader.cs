using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpReader
{
    public class EveLogReader
    {
        private string LogPath { get; set; } = string.Empty;
        private HashSet<string> SubscribedCharacters { get; set; } = new HashSet<string>();

        /// <summary>
        /// Initializes a new EveLogReader. Assumes default location of log files
        /// if not given a path. 
        /// </summary>
        /// <param name="logPath"></param>
        public EveLogReader(string logPath = "", List<string> charactersToWatch = null)
        {
            if (logPath == "")
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\EVE\logs\";
                LogPath = Path.GetFullPath(path);
            }
            else
            {
                LogPath = logPath;
            }
            // Initialize the LogReader/watcher
            // get log path
            // verify path
            // Init watcher
        }

        public void Subscribe(string characterName)
        {
            SubscribedCharacters.Add(characterName);
        }

        public void Unsubscribe(string characterName)
        {
            SubscribedCharacters.Remove(characterName);
        }
    }
}
