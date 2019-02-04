using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVEStandard.Models.API;
using Int64 = System.Int64;

namespace BUZZ.Utilities
{
    public class SolarSystems
    {
        private static Dictionary<int,string> SystemIdToNameDictionary { get; set; } = new Dictionary<int, string>();
        private static Dictionary<string, int> NameToSystemIdDictionary { get; set; } = new Dictionary<string, int>();

        public static string GetSolarSystemName(int systemId)
        {
            return SystemIdToNameDictionary[systemId];
        }

        public static void LoadSolarSystems()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith("mapSolarSystems.csv"));

            var stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine();
                var lineValues = currentLine.Split(',');
                try
                {
                    var systemName = lineValues[3];
                    int systemId;
                    Int32.TryParse(lineValues[2].ToString(),out systemId);
                    SystemIdToNameDictionary.Add(systemId, systemName);
                    NameToSystemIdDictionary.Add(systemName, systemId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
