using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core.Verification;
using BUZZ.Data;
using BUZZ.Data.Models;

namespace BUZZ.Core.CharacterManagement
{
    /// <summary>
    /// This buzzCharacter manager is a singleton class that contains and manages the list of characters.
    /// </summary>
    public sealed class CharacterManager
    {
        private static CharacterManager currentInstance { get; } = new CharacterManager();
        public static CharacterManager CurrentInstance {
            get {
                lock (CurrentInstance)
                {
                    return currentInstance;
                }
            }
        }

        #region Properties and Lists

        public List<BuzzCharacter> CharacterList { get; private set; } = new List<BuzzCharacter>();

        #endregion

        private CharacterManager(){}

        
        
        #region Public Methods

        internal void AddNewCharacter(BuzzCharacter buzzCharacter)
        {
            CharacterList.Add(buzzCharacter);
        }

        #endregion
    }
}
