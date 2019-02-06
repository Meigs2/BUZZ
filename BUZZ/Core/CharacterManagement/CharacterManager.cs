using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
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

        public DispatcherTimer AuthRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(Properties.Settings.Default.CharacterAuthRefreshRateMinutes),
            IsEnabled = false
        };

        public DispatcherTimer CharacterInfoRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(Properties.Settings.Default.CharacterInfoRefreshRateSeconds),
            IsEnabled = false
        };

        #endregion

        private CharacterManager()
        {
        }

        #region Public Methods

        /// <summary>
        /// Preforms startup actions for the Character side of things in BUZZ.
        /// </summary>
        public async static void Initialize()
        {
            currentInstance = new CharacterManager();

            DeserializeCharacterData();
            await RefreshAccessTokensAsync();
            await RefreshCharacterInformation();
            SerializeCharacterData();
            SetUpRefreshTimers();
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


        private async static void CharacterInfoRefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshCharacterInformation();
        }

        private async static void AuthRefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshAccessTokensAsync();
            SerializeCharacterData();
        }

        private async static Task RefreshCharacterInformation()
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
                SharpSerializer searializer = new SharpSerializer();
                CurrentInstance.CharacterList =
                    (BindingList<BuzzCharacter>)searializer.Deserialize(CharacterDataFilename);
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
                var searializer = new SharpSerializer();
                searializer.Serialize(CurrentInstance.CharacterList, CharacterDataFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
