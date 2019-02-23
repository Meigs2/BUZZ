using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using EVEMapBuilder;

namespace BUZZ.Utilities
{
    public class SolarSystems
    {
        private static Dictionary<int,string> SystemIdToNameDictionary { get; set; } = new Dictionary<int, string>();
        private static Dictionary<string, int> NameToSystemIdDictionary { get; set; } = new Dictionary<string, int>();

        private static Dictionary<long, EveSystem> SystemIdToSolarSystem { get; set; } = new Dictionary<long, EveSystem>();
        
        public static List<string> GetAllSolarSystems()
        {
            return new List<string>(NameToSystemIdDictionary.Keys);
        }

        public static string GetSolarSystemName(int systemId)
        {
            return SystemIdToNameDictionary[systemId];
        }

        public static int GetSolarSystemId(string systemName)
        {
            return NameToSystemIdDictionary[systemName];
        }

        public static void LoadSolarSystems()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var map = new List<EveSystem>();

            string mapResourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith("EveMap.json"));

            var mapStream = assembly.GetManifestResourceStream(mapResourceName);
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(mapStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
               map = serializer.Deserialize<List<EveSystem>>(jsonTextReader);
            }

            foreach (var eveSystem in map)
            {
                SystemIdToSolarSystem.Add(eveSystem.SolarSystemId,eveSystem);
            }


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
                    Int32.TryParse(lineValues[2].ToString(), out systemId);
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

        public static List<EveSystem> GetShortestPath(int startingSystemId, int endingSystemId)
        {
            var start = (long) startingSystemId;
            var end = (long) endingSystemId;

            var path = new Dictionary<EveSystem,EveSystem>();
            var visitedHash = new HashSet<long>();

            Queue<EveSystem> queue = new Queue<EveSystem>();
            queue.Enqueue(SystemIdToSolarSystem[start]);
            visitedHash.Add(start);
            while (queue.Count > 0)
            {
                EveSystem currentSystem = queue.Dequeue();
                foreach (var systemId in currentSystem.Connections)
                {
                    if (visitedHash.Contains(systemId))
                        continue;
                    visitedHash.Add(systemId);

                    var currentChild = SystemIdToSolarSystem[systemId];
                    path[currentChild] = currentSystem;

                    if (currentSystem.SolarSystemId == end)
                    {
                        break;
                    }

                    queue.Enqueue(currentChild);
                }
            }
            var shortestPath = new List<EveSystem>();
            var current = SystemIdToSolarSystem[end];
            while (current!=SystemIdToSolarSystem[start])
            {
                shortestPath.Add(current);
                current = path[current];
            }
            shortestPath.Add(SystemIdToSolarSystem[start]);
            shortestPath.Reverse();
            return shortestPath;
        }
    }
}
