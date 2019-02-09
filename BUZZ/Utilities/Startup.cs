using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core.CharacterManagement;

namespace BUZZ.Utilities
{
    public class Startup
    {
        public static async void PerformStartupActions()
        {
            SolarSystems.LoadSolarSystems();
            await CharacterManager.Initialize();
        }
    }
}
