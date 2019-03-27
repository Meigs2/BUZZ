using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BUZZ.Core.LogReading
{
    public class EveLogReader
    {
        private string LogPath { get; set; }
        private HashSet<string> Characters { get; set; } = new HashSet<string>();
        private HashSet<string> Paths { get; set; } = new HashSet<string>();
        private FileSystemWatcher Watcher { get; set; }

        public DispatcherTimer FileRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = false
        };

        /// <summary>
        /// Initializes a new EveLogReader. Assumes default location of log files
        /// if not given a path. 
        /// </summary>
        /// <param name="logPath"></param>
        public EveLogReader(string logPath = "")
        {
            if (logPath == "")
            {
                LogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\EVE\logs\";
                if (!Directory.Exists(LogPath))
                    throw new DirectoryNotFoundException();
            }
            else
            {
                LogPath = logPath;
                if (!Directory.Exists(LogPath))
                    throw new DirectoryNotFoundException();
            }

            // Get list of characters and most recent Local Chat paths to said characters.
            var directory = new DirectoryInfo(LogPath + @"\ChatLogs\");
            DateTime from_date = DateTime.Now.AddHours(-10);
            DateTime to_date = DateTime.Now;
            var files = directory.GetFiles()
                .Where(file => file.LastWriteTime >= from_date && file.LastWriteTime <= to_date);
            foreach (var filePath in files)
            {
                try
                {
                    using (var fileStream = File.Open(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (line != null && line.Contains("Listener:        "))
                            {
                                var listenerName = line.Split(new string[] { "          Listener:        " }, StringSplitOptions.RemoveEmptyEntries)[0];
                                Characters.Add(listenerName);
                                Paths.Add(filePath.FullName);
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            // Init Refresher (to make sure we EVE actually dumps chat history to the log files, CCPLS)
            // Actually not CCPLS theres probably a very valid reason CCP wont just dump chat logs 24/7.
            // ...but then again they dump combat logs right away so who knows...
            FileRefreshTimer.Tick += FileRefreshTimerOnTick;
            FileRefreshTimer.Start();
            FileRefreshTimerOnTick(null,null);

            // Init watcher
            Watcher = new FileSystemWatcher()
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Path = LogPath,
                IncludeSubdirectories = true,
                Filter = "*.txt",
            };
            Watcher.Created += ChatLogsWatcher_FileCreated;
            Watcher.Changed += ChatLogsWathcer_FileChanged;
            Watcher.EnableRaisingEvents = true;
        }

        private void FileRefreshTimerOnTick(object sender, EventArgs e)
        {
            try
            {
                foreach (var path in Paths)
                {
                    var refreshFileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    refreshFileStream.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void ChatLogsWathcer_FileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath + " Changed");
        }

        private void ChatLogsWatcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath + " Created");
        }
    }
}
