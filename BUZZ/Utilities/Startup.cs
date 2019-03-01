using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core;
using BUZZ.Core.CharacterManagement;
using System.Reflection;

namespace BUZZ.Utilities
{
    public class Startup
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async void PerformStartupActions()
        {

            SolarSystems.LoadSolarSystems();
            await CharacterManager.Initialize();
        }
    }
}
