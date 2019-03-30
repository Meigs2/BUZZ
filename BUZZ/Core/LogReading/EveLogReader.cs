using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BUZZ.Core.LogReading
{
    public class EveLogReader
    {
        private FileSystemWatcher FileWatcher { get; set; }
        private string LogPath { get; set; }
        private Dictionary<string,LogFile> LocalLogDictionary { get; set; } = new Dictionary<string,LogFile>();
        public DispatcherTimer FileRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = false
        };

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
            FileWatcher.EnableRaisingEvents = true;

            // get local chat logs from 24hrs ago
            var directory = new DirectoryInfo(LogPath + @"\Chatlogs\");
            var localChatFiles = directory.GetFiles()
                .Where(file => file.LastWriteTime >= (DateTime.Now-TimeSpan.FromDays(0.1)) && file.Name.Contains("Local_"));
            foreach (var localChatFile in localChatFiles)
            {
                LocalLogDictionary.Add(localChatFile.FullName, new LogFile(){LogPath = localChatFile.FullName, CurrentFileLength = 0});
            }

            FileRefreshTimer.Tick += FileRefreshTimer_Tick;
            FileRefreshTimer.IsEnabled = true;
        }

        private void FileRefreshTimer_Tick(object sender, EventArgs e)
        {
            foreach (var localLogPath in LocalLogDictionary.Keys)
            {
                var file = File.Open(localLogPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (LocalLogDictionary[localLogPath].CurrentFileLength < file.Length)
                {
                    LocalLogDictionary[localLogPath].CurrentFileLength = file.Length;
                    Console.WriteLine(file.Name + " has been modified at " + DateTime.Now);
                }
                file.Close();
            }
        }

        private void FileWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath + " was created");
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath + " was changed");
        }
    }
}
