using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using BUZZ.Utilities;

namespace BUZZ.Core.LogReading
{
    public class EveLogReader
    {
        private FileSystemWatcher FileWatcher { get; set; }
        private string LogPath { get; set; }
        private Dictionary<string,LogFile> ChatLogDictionary { get; set; } = new Dictionary<string,LogFile>();
        public DispatcherTimer FileRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = false
        };

        private readonly Regex ListenerRegex =
            new Regex(@"^[ ]*(Listener|Empfänger|Auditeur|Слушатель):[ ]*(?<Name>.*)$", RegexOptions.Compiled);
        private readonly Regex LocalSystemChange = new Regex(@"(\[ (?<TimeStamp>.*) \])(.+: (?<NewSystemName>.*))", RegexOptions.Compiled);

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EveLogReader()
        {
            LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\EVE\logs\";
            if (!Directory.Exists(LogPath))
                throw new DirectoryNotFoundException();

            FileWatcher = new FileSystemWatcher()
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Path = LogPath,
                IncludeSubdirectories = true,
                Filter = "*.txt",
            };
            FileWatcher.Created += FileWatcherOnCreated;
            FileWatcher.Changed += FileWatcher_Changed;

            // get local chat logs from 24hrs ago
            var directory = new DirectoryInfo(LogPath + @"\Chatlogs\");
            var localChatFiles = directory.GetFiles()
                .Where(file => file.LastWriteTime >= (DateTime.Now-TimeSpan.FromDays(1)) && file.Name.Contains("Local_"));
            // add files to be checked for changes
            foreach (var localChatFile in localChatFiles)
            {
                ChatLogDictionary.Add(localChatFile.FullName, new LogFile(){LogPath = localChatFile.FullName, CurrentFileLength = 0});
            }
            FileRefreshTimer.Tick += CheckLogFiles;
        }

        public void EnableFileWatching()
        {
            FileWatcher.EnableRaisingEvents = true;
            FileRefreshTimer.IsEnabled = true;
        }

        /// <summary>
        /// Watches relevant chat log files for changes. Due to how eve handles chat logs, a FileSystemWatcher
        /// wont pick up OnChanged events for them until the client is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckLogFiles(object sender, EventArgs e)
        {
            foreach (var localLogPath in ChatLogDictionary.Keys)
            {
                var file = File.Open(localLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // If the file length has changed, its been modified and we need to get the new info.
                if (ChatLogDictionary[localLogPath].CurrentFileLength < file.Length)
                {
                    var lines = new List<string>();
                    var streamReader = new StreamReader(file, Encoding.UTF8);

                    file.Seek(ChatLogDictionary[localLogPath].CurrentFileLength, SeekOrigin.Begin);
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        // Delete non-ASCII characters cause CCPls
                        line = Regex.Replace(line, @"[^\u0000-\u007F]+", string.Empty);
                        lines.Add(line);
                    }
                    ParseChatLogLines(lines, ChatLogDictionary[localLogPath]);
                    ChatLogDictionary[localLogPath].CurrentFileLength = file.Length;
                }
                file.Close();
            }
        }

        private void FileWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            // if path is to a local file, add it to watch list
            if (e.FullPath.Contains(@"Local_"))
            {
                ChatLogDictionary.Add(e.FullPath, new LogFile());
                Console.WriteLine(e.FullPath + " was created");
            }
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath + " was changed");
        }

        /// <summary>
        /// Contains the logic for parsing chat log lines, and raises the relevant events.
        /// </summary>
        private void ParseChatLogLines(List<string> chatLines, LogFile chatLogFile)
        {
            try
            {
                // Check if file has never been opened, if so preform file initialization, raise relevant event
                SystemChangedEventArgs eventArgs = new SystemChangedEventArgs();
                if (chatLogFile.CurrentFileLength <= 0)
                {
                    // Get listener
                    if (ListenerRegex.IsMatch(chatLines[8]))
                    {
                        chatLogFile.CurrentListener = ListenerRegex.Match(chatLines[8]).Groups["Name"].Value;
                        eventArgs.Listener = chatLogFile.CurrentListener;
                    }

                    // Get current system
                    foreach (var chatLine in chatLines)
                    {
                        if (!LocalSystemChange.IsMatch(chatLine)) continue;
                        var systemName = LocalSystemChange.Match(chatLine).Groups["NewSystemName"].Value;
                        // check if valid system
                        if (SolarSystems.SystemNameToIdDictionary.TryGetValue(systemName, out var systemId))
                        {
                            eventArgs.NewSystemName = systemName;
                            eventArgs.NewSystemId = systemId;
                        }
                    }
                    OnSystemChanged(eventArgs);
                }
                // if its been analyzed before, parse the new lines for a system change.
                else
                {
                    eventArgs.Listener = chatLogFile.CurrentListener;

                    // Get current system
                    foreach (var chatLine in chatLines)
                    {
                        if (!LocalSystemChange.IsMatch(chatLine)) continue;
                        var systemName = LocalSystemChange.Match(chatLine).Groups["NewSystemName"].Value;
                        // check if valid system
                        if (SolarSystems.SystemNameToIdDictionary.TryGetValue(systemName, out var systemId))
                        {
                            eventArgs.NewSystemName = systemName;
                            eventArgs.NewSystemId = systemId;
                        }
                    }
                    OnSystemChanged(eventArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e);
                throw;
            }

            // Else, determine if its a system change
        }

        #region Events

        public event EventHandler<SystemChangedEventArgs> SystemChanged;

        protected virtual void OnSystemChanged(SystemChangedEventArgs eventArgs)
        {
            EventHandler<SystemChangedEventArgs> handler = SystemChanged;
            handler?.Invoke(this, eventArgs);
        }

        #endregion
    }
}
