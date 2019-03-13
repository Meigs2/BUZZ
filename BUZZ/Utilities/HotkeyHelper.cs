using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core.CharacterManagement;
using EVEStandard.API;

namespace BUZZ.Utilities
{
    public class HotkeyHelper
    {
        public static void ActivateAllCurrentHotkeys()
        {
            foreach (var buzzCharacter in CharacterManager.CurrentInstance.CharacterList)
            {
                buzzCharacter.RegisterActivateHotkey();
            }
        }
    }
}
