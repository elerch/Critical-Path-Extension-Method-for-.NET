using System;
using System.Collections.Generic;
using System.Linq;

namespace CriticalPathMethod
{
    public static class IEnumerableExtensions
    {
        private class PathInfo<T>
        {
            public IEnumerable<T> Predecessors { get; private set; }
            public IEnumerable<T> Successors { get; private set; }

            /// <summary>
            /// Earliest start
            /// </summary>
            public long EarliestStart { get; set; }

            /// <summary>
            ///  Latest start
            /// </summary>
            public long LatestStart { get; set; }

            /// <summary>
            /// Earliest end 
            /// </summary>
            public long EarliestEnd { get; set; }


            /// <summary>
            /// Latest end 
            /// </summary>
            public long LatestEnd { get; set; }

            public PathInfo(IEnumerable<T> predecessors, IEnumerable<T> sucessors) {
                Predecessors = predecessors;
                Successors = sucessors;
            }
        }

        private static IEnumerable<KeyValuePair<T,PathInfo<T>>> OrderByDependencies<T>(IEnumerable<KeyValuePair<T, PathInfo<T>>> list)
        {
            var processedPairs = new HashSet<T>();
            var totalCount = list.Count();
            var rc = new List<KeyValuePair<T,PathInfo<T>>>(totalCount);
            while (rc.Count < totalCount) {
                bool foundSomethingToProcess = false;
                foreach (var kvp in list) {
                    if (!processedPairs.Contains(kvp.Key)
                        && kvp.Value.Predecessors.All(processedPairs.Contains)) {
                        rc.Add(kvp);
                        processedPairs.Add(kvp.Key);
                        foundSomethingToProcess = true;
                        yield return kvp;
                    }
                }
                if (!foundSomethingToProcess)
                    throw new InvalidOperationException("Loop detected inside path");
            }
        }

        /// <summary>
        /// Performs the walk ahead inside the list of nodes calculating for each
        /// node its earliest start and earliest end.
        /// </summary>
        /// <param name="list">Dictionary of all nodes, with a PathInfo version as the key</param>
        /// <param name="lengthSelector">Function to determine the length, duration, etc for the node.  
        /// In the absence of INumeric, since we have to do math, we're going to force a long return value</param>
        private static void FillEarliestValues<T>(this IDictionary<T,PathInfo<T>> list, Func<T,long> lengthSelector)
        {
            if (!list.Any()) return;

            foreach (var item in list) {
                foreach (var predecessor in item.Value.Predecessors) {
                    if (item.Value.EarliestStart < list[predecessor].EarliestEnd)
                        item.Value.EarliestStart = list[predecessor].EarliestEnd;
                }
                item.Value.EarliestEnd = item.Value.EarliestStart + lengthSelector(item.Key);
            }
        }

        /// <summary>
        /// Performs the reverse walk inside the array of nodes calculating for each
        /// node its latest start and latest end.  Must be called after the forward walk
        /// </summary>
        /// <param name="list">Dictionary of all nodes, with a PathInfo version as the key</param>
        /// <param name="lengthSelector">Function to determine the length, duration, etc for the node.  
        /// In the absence of INumeric, since we have to do math, we're going to force a long return value</param>
        private static void FillLatestValues<T>(this IDictionary<T, PathInfo<T>> list, Func<T, long> lengthSelector)
        {
            var reversedList = list.Reverse();
            var isFirst = true;

            foreach (var node in reversedList) {
                if (isFirst) {
                    node.Value.LatestEnd = node.Value.EarliestEnd;
                    isFirst = false;
                }

                foreach (var successor in node.Value.Successors) {
                    if (node.Value.LatestEnd == 0)
                        node.Value.LatestEnd = list[successor].LatestStart;
                    else
                        if (node.Value.LatestEnd > list[successor].LatestStart)
                            node.Value.LatestEnd = list[successor].LatestStart;
                }

                node.Value.LatestStart = node.Value.LatestEnd - lengthSelector(node.Key);
            }
        }

        /// <summary>
        /// Calculates critical path through a series of nodes
        /// </summary>
        /// <param name="list"></param>
        /// <param name="predecessorSelector"></param>
        /// <param name="lengthSelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> CriticalPath<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predecessorSelector, Func<T, long> lengthSelector) {
            var successors = list.GetSucessors(predecessorSelector);
            var piList = list.ToPathInfoDictionary(predecessorSelector, n => successors[n]);
            var orderedList = OrderByDependencies(piList).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);            
            orderedList.FillEarliestValues(lengthSelector);
            orderedList.FillLatestValues(lengthSelector);
            return orderedList
                .Where(
                    kvp => (kvp.Value.EarliestEnd - kvp.Value.LatestEnd == 0)
                        && (kvp.Value.EarliestStart - kvp.Value.LatestStart == 0))
                .Select(n => n.Key);
        }

        private static IDictionary<T, IEnumerable<T>> GetSucessors<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predecessorSelector) {
            var rc = new Dictionary<T, IEnumerable<T>>();
            foreach (var item in list) {
                // Just in case this item isn't on anyone's predecessor list, 
                // we'll need to make sure there's a dictionary entry for it
                if (!rc.ContainsKey(item)) 
                    rc.Add(item, new List<T>());

                // Ok, go through the item's predecessors and add itself as a successor
                foreach (var predecessor in predecessorSelector(item)) {
                    if(!rc.ContainsKey(predecessor))
                        rc.Add(predecessor, new List<T>());
                    var predecessorSuccessorList =
                            (List<T>) rc[predecessor];
                    predecessorSuccessorList.Add(item);
                }
            }
            return rc;
        }

        private static IDictionary<T, PathInfo<T>> ToPathInfoDictionary<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predecessorSelector, Func<T, IEnumerable<T>> sucessorSelector) {
            return list.ToDictionary(
                item => item, 
                item => new PathInfo<T>(predecessorSelector(item), sucessorSelector(item)));
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> e)
        {
            var r = new Random();
            return e.OrderBy(x => r.Next());
        }
    }
}
