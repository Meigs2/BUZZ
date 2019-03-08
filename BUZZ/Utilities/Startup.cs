using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BUZZ.Core;
using BUZZ.Core.CharacterManagement;
using log4net;
using System.Reflection;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace BUZZ.Utilities
{
    public class Startup
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void PerformStartupActions()
        {
            Log.Info("Logger Initialized and new BUZZ session started.");
            SolarSystems.LoadSolarSystems();
            CharacterManager.Initialize();
        }
    }
}
