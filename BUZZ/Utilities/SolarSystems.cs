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

        public static EveSystem GetEveSystem(int systemId)
        {
            return SystemIdToSolarSystem[systemId];
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
                SystemIdToNameDictionary.Add(eveSystem.SolarSystemId,eveSystem.Name);
                NameToSystemIdDictionary.Add(eveSystem.Name,eveSystem.SolarSystemId);
            }
        }

        public static List<EveSystem> GetShortestPath(int startingSystemId, int endingSystemId)
        {
            var path = new Dictionary<EveSystem,EveSystem>();
            var visitedHash = new HashSet<int>();

            Queue<EveSystem> queue = new Queue<EveSystem>();
            queue.Enqueue(SystemIdToSolarSystem[startingSystemId]);
            visitedHash.Add(startingSystemId);
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

                    if (currentSystem.SolarSystemId == endingSystemId)
                    {
                        break;
                    }

                    queue.Enqueue(currentChild);
                }
            }
            var shortestPath = new List<EveSystem>();
            var current = SystemIdToSolarSystem[endingSystemId];
            while (current!=SystemIdToSolarSystem[startingSystemId])
            {
                shortestPath.Add(current);
                current = path[current];
            }
            shortestPath.Add(SystemIdToSolarSystem[startingSystemId]);
            shortestPath.Reverse();
            return shortestPath;
        }

        public static void OptimizeRoute(List<int> systems, int startingSystem = 0, int endSystem = 0)
        {

        }
    }
}
