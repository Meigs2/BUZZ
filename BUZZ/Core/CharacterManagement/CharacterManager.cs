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
using BUZZ.Core.Models;
using BUZZ.Core.Verification;
using BUZZ.Data;
using log4net.Config;
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
        public static void Initialize()
        {
            currentInstance = new CharacterManager();

            DeserializeCharacterData();
            RefreshAccessTokensAsync();
            RefreshCharacterInformation();
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


        private static void CharacterInfoRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
               RefreshCharacterInformation();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void AuthRefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshAccessTokensAsync();
            SerializeCharacterData();
        }

        public static void RefreshCharacterInformation()
        {
            try
            {
                foreach (var buzzCharacter in CurrentInstance.CharacterList)
                {

                    ThreadPool.QueueUserWorkItem(async a =>
                    {
                        await buzzCharacter.RefreshCharacterInformation();
                    });
                }
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

        private static void RefreshAccessTokensAsync()
        {
            try
            {
                foreach (var buzzCharacter in CurrentInstance.CharacterList)
                {
                    ThreadPool.QueueUserWorkItem(async a =>
                    {
                        await buzzCharacter.RefreshAuthToken();
                    });
                }
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

        #region Private Methods

        #endregion
    }
}
