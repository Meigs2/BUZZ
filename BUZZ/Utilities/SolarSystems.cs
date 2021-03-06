﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Documents;
using EVEMapBuilder;
using EVEStandard.Models;

namespace BUZZ.Utilities
{
    public class SolarSystems
    {
        public static Dictionary<int,string> SystemIdToNameDictionary { get; set; } = new Dictionary<int, string>();
        public static Dictionary<string, int> SystemNameToIdDictionary { get; set; } = new Dictionary<string, int>();
        public static Dictionary<int, EveSystem> SystemIdToSolarSystem { get; set; } = new Dictionary<int, EveSystem>();
        
        public static List<string> GetAllSolarSystems()
        {
            return new List<string>(SystemNameToIdDictionary.Keys);
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
            return SystemNameToIdDictionary[systemName];
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
                SystemNameToIdDictionary.Add(eveSystem.Name,eveSystem.SolarSystemId);
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

        /// <summary>
        /// Optimizes a set of systemId's. The first system provided is considered the starting system.
        /// If provided a destination system, the route will optimize to end in that system.
        /// </summary>
        /// <param name="systems"></param>
        /// <param name="destinationSystem"></param>
        /// <returns></returns>
        public static async Task<List<int>> OptimizeRouteAsync(List<int> systems)
        {
            // Convert systems to a list of nodes to visit.
            var searchList = new List<Node>();
            foreach (var system in systems)
            {
                searchList.Add(new Node()
                {
                    System = SystemIdToSolarSystem[system],
                    Connections = new Dictionary<Node, int>()
                });
            }

            // Filter duplicates from list
            var copyList = new List<Node>();
            var systemList = new List<EveSystem>();
            foreach (var node in searchList)
            {
                if (!systemList.Contains(node.System))
                {
                    copyList.Add(node);
                    systemList.Add(node.System);
                }
            }
            searchList = copyList;

            // Get distances
            foreach (var searchNode in searchList)
            {
                List<Task<List<EveSystem>>> searchTasks = new List<Task<List<EveSystem>>>();
                foreach (var innerNode in searchList)
                {
                    if (innerNode.System != searchNode.System)
                    {
                        searchTasks.Add(Task.Run(()=>GetShortestPath(searchNode.System.SolarSystemId,innerNode.System.SolarSystemId)));
                    }
                }

                // get our list of connections from searchNode to all other nodes in searchList
                var result = await Task.WhenAll(searchTasks);
                foreach (var eveSystem in result)
                {
                    searchNode.Connections[searchList.Single(s => s.System == eveSystem[eveSystem.Count - 1])] = eveSystem.Count - 1;
                }
            }

            var route = new List<Node>(searchList.Count);
            var searchResult = new List<Node>();
        
            searchResult = SearchNode(searchList[0], route, new List<Node>(), searchList.Count);

            // Convert nodes to systemId's
            var optimizedSystems = new List<int>();
            foreach (var node in searchResult)
            {
                optimizedSystems.Add(node.System.SolarSystemId);
            }

            return optimizedSystems;
        }

        private static List<Node> SearchNode(Node node, List<Node> route, List<Node> optimalRoute, int capacity)
        {
            // for stupid c# reasons, we need to make copies of all the lists, as 
            // c# will pass them by reference, and that's not what we want.
            route = MakeRouteCopy(route);
            route.Add(node);

            // If we're full, check our route
            if (route.Count == capacity)
            {
                return MakeRouteCopy(route);
            }

            var routeList = new List<List<Node>>();
            foreach (var connection in node.Connections)
            {
                if (!route.Contains(connection.Key))
                {
                    route = MakeRouteCopy(route);
                    optimalRoute = MakeRouteCopy(optimalRoute);
                    routeList.Add(SearchNode(connection.Key, route, optimalRoute, capacity)); 
                }
            }


            foreach (var innerRoute in routeList)
            {
                if (GetSum(optimalRoute) > GetSum(innerRoute))
                {
                    optimalRoute = innerRoute;
                }
                else if (optimalRoute.Count == 0)
                {
                    optimalRoute = innerRoute;
                }
            }

            return MakeRouteCopy(optimalRoute);
        }

        private static int GetSum(List<Node> route)
        {
            var sum = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                sum += route[i].Connections[route[i + 1]];
            }
            return sum;
        }

        private static List<Node> MakeRouteCopy(List<Node> route)
        {
            var newList = new List<Node>();
            foreach (var node in route)
            {
                newList.Add(node);
            }

            return newList;
        }

        public class Node
        {
            public EveSystem System { get; set; }
            public Dictionary<Node,int> Connections { get; set; }
        }
    }

}
