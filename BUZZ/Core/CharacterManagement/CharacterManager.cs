using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using BUZZ.Core.LogReading;
using BUZZ.Core.Models;
using BUZZ.Core.Verification;
using BUZZ.Data;
using Polenter.Serialization;
using Exception = System.Exception;

namespace BUZZ.Core.CharacterManagement
{
    /// <summary>
    /// This buzzCharacter manager is a singleton class that contains and manages the list of characters.
    /// </summary>
    public class CharacterManager
    {
        private static CharacterManager currentInstance;

        public static CharacterManager CurrentInstance {
            get {
                if (currentInstance == null)
                {
                    currentInstance = new CharacterManager();
                }
                return currentInstance;
            }
        }

        #region Properties and Lists

        public BindingList<BuzzCharacter> CharacterList { get; set; } = new BindingList<BuzzCharacter>();
        
        private EveLogReader LogReader { get; set; }

        public DispatcherTimer AuthRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(Properties.Settings.Default.CharacterAuthRefreshRateMinutes)
        };

        public DispatcherTimer CharacterInfoRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(3)
        };

        #endregion

        private CharacterManager()
        {
        }
        
        #region Public Methods

        /// <summary>
        /// Preforms startup actions for the Character side of things in BUZZ.
        /// </summary>
        public static async Task Initialize()
        {
            currentInstance = new CharacterManager();

            DeserializeCharacterData();
            await RefreshAccessTokensAsync();
            await RefreshCharacterInformation();
            SerializeCharacterData();
            SetUpRefreshTimers();
            InitializeLogReader();
        }

        private static void InitializeLogReader()
        {
            CurrentInstance.LogReader = new EveLogReader();
            CurrentInstance.LogReader.SystemChanged += CurrentInstance.LogReader_SystemChanged;
            CurrentInstance.LogReader.EnableFileWatching();
        }

        public void LogReader_SystemChanged(object sender, SystemChangedEventArgs e)
        {
            var character = CurrentInstance.CharacterList.SingleOrDefault(f => f.CharacterName == e.Listener);
            if (character == null) return;

            if (character.CurrentSolarSystem.SolarSystemName != e.NewSystemName)
            {
                var solarSystem = new SolarSystemModel();
                solarSystem.SolarSystemId = e.NewSystemId;
                solarSystem.SolarSystemName = e.NewSystemName;
                character.CurrentSolarSystem = solarSystem;
            }
        }

        #region Timer Methods

        private static void SetUpRefreshTimers()
        {
            CurrentInstance.AuthRefreshTimer.Tick += AuthRefreshTimer_Tick;
            CurrentInstance.CharacterInfoRefreshTimer.Tick += CharacterInfoRefreshTimer_Tick;
            StartRefreshTimers();
        }

        private static void StartRefreshTimers()
        {
            CurrentInstance.AuthRefreshTimer.IsEnabled = true;
            CurrentInstance.CharacterInfoRefreshTimer.IsEnabled = true;
        }

        private static void StopRefreshTimers()
        {
            CurrentInstance.AuthRefreshTimer.IsEnabled = false;
            CurrentInstance.CharacterInfoRefreshTimer.IsEnabled = false;
        }

        #endregion

        private static async void CharacterInfoRefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshCharacterInformation();
        }

        private static async void AuthRefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshAccessTokensAsync();
            SerializeCharacterData();
        }

        public static async Task RefreshCharacterInformation()
        {
            try
            {
                var taskList = new List<Task>();
                foreach (var buzzCharacter in CurrentInstance.CharacterList)
                {
                    taskList.Add(buzzCharacter.RefreshCharacterInformation());
                }
                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async void RefreshGivenCharacterInformation(BuzzCharacter character)
        {
            await character.RefreshCharacterInformation();
        }

        private static async Task RefreshAccessTokensAsync()
        {
            try
            {
                var taskList = new List<Task>();
                foreach (var buzzCharacter in CharacterManager.currentInstance.CharacterList)
                {
                    taskList.Add(buzzCharacter.RefreshAuthToken());
                }

                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private const string CharacterDataFilename = "Characters.bin";

        public static void DeserializeCharacterData()
        {
            try
            {
                SharpSerializer serializer = new SharpSerializer();
                CurrentInstance.CharacterList =
                    (BindingList<BuzzCharacter>)serializer.Deserialize(CharacterDataFilename);
            }
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                {
                    File.Create("Characters.bin");
                }
                Console.WriteLine(e);
            }
        }

        public static void SerializeCharacterData()
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(CurrentInstance.CharacterList, CharacterDataFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}
