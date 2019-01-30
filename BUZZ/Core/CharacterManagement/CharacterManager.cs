using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using BUZZ.Core.Verification;
using BUZZ.Data;
using BUZZ.Data.Models;
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

        public DispatcherTimer RefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(10),
            IsEnabled = false
        };

        #endregion

        private CharacterManager()
        {
        }

        #region Public Methods

        public static void Initialize()
        {
            currentInstance = new CharacterManager();
            DeserializeCharacterData();
            RefreshAccessTokensAsync();
            StartAutoRefresh();
        }

        private static void StartAutoRefresh()
        {
            CurrentInstance.RefreshTimer.Tick += RefreshTimer_Tick;
            CurrentInstance.RefreshTimer.IsEnabled = true;
        }

        private static void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshAccessTokensAsync();
        }

        private static async void RefreshAccessTokensAsync()
        {
            try
            {
                var tasks = new List<Task>();
                foreach (var buzzCharacter in CharacterManager.currentInstance.CharacterList)
                {
                    tasks.Add(buzzCharacter.RefreshAuthToken());
                }
                await Task.WhenAll(tasks);
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
                Console.WriteLine(e);
                throw;
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
                throw;
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
