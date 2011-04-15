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
            /// Earliest start time
            /// </summary>
            public long EarliestStartTime { get; set; }

            /// <summary>
            ///  Latest start time
            /// </summary>
            public long LatestStartTime { get; set; }

            /// <summary>
            /// Earliest end time
            /// </summary>
            public long EarliestEndTime { get; set; }


            /// <summary>
            /// Latest end time
            /// </summary>
            public long LatestEndTime { get; set; }

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
                foreach (var kvp in list) {
                    if (!processedPairs.Contains(kvp.Key)
                        && kvp.Value.Predecessors.All(processedPairs.Contains)) {
                        rc.Add(kvp);
                        processedPairs.Add(kvp.Key);
                        yield return kvp;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the walk ahead inside the array of activities calculating for each
        /// activity its earliest start time and earliest end time.
        /// </summary>
        /// <param name="list">Dictionary of all nodes, with a PathInfo version as the key</param>
        /// <param name="lengthSelector">Function to determine the length, duration, etc for the node.  
        /// In the absence of INumeric, since we have to do math, we're going to force a long return value</param>
        private static void WalkListAhead<T>(IDictionary<T,PathInfo<T>> list, Func<T,long> lengthSelector)
        {
            if (!list.Any()) return;

            foreach (var item in list) {
                foreach (var predecessor in item.Value.Predecessors) {
                    if (item.Value.EarliestStartTime < list[predecessor].EarliestEndTime)
                        item.Value.EarliestStartTime = list[predecessor].EarliestEndTime;
                }
                item.Value.EarliestEndTime = item.Value.EarliestStartTime + lengthSelector(item.Key);
            }
        }

        /// <summary>
        /// Performs the reverse walk inside the array of activities calculating for each
        /// activity its latest start time and latest end time.  Must be called after the
        /// forward walk
        /// </summary>
        /// <param name="list">Dictionary of all nodes, with a PathInfo version as the key</param>
        /// <param name="lengthSelector">Function to determine the length, duration, etc for the node.  
        /// In the absence of INumeric, since we have to do math, we're going to force a long return value</param>
        private static void WalkListAback<T>(IDictionary<T, PathInfo<T>> list, Func<T, long> lengthSelector)
        {
            var reversedList = list.Reverse();
            var isFirst = true;

            foreach (var node in reversedList) {
                if (isFirst) {
                    node.Value.LatestEndTime = node.Value.EarliestEndTime;
                    isFirst = false;
                }

                foreach (var successor in node.Value.Successors) {
                    if (node.Value.LatestEndTime == 0)
                        node.Value.LatestEndTime = list[successor].LatestStartTime;
                    else
                        if (node.Value.LatestEndTime > list[successor].LatestStartTime)
                            node.Value.LatestEndTime = list[successor].LatestStartTime;
                }

                node.Value.LatestStartTime = node.Value.LatestEndTime - lengthSelector(node.Key);
            }
        }

        /// <summary>
        /// Calculates critical path through a series of nodes
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> CriticalPath<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predecessorSelector, Func<T, IEnumerable<T>> sucessorSelector, Func<T, long> lengthSelector) {
            var piList = list.ToPathInfoDicationary(predecessorSelector, sucessorSelector);
            var orderedList = OrderByDependencies(piList).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            WalkListAhead(orderedList, lengthSelector);
            WalkListAback(orderedList, lengthSelector);
            return orderedList
                .Where(
                    kvp => (kvp.Value.EarliestEndTime - kvp.Value.LatestEndTime == 0)
                        && (kvp.Value.EarliestStartTime - kvp.Value.LatestStartTime == 0))
                .Select(n => n.Key);
        }

        private static IDictionary<T, PathInfo<T>> ToPathInfoDicationary<T>(this IEnumerable<T> list, Func<T, IEnumerable<T>> predecessorSelector, Func<T, IEnumerable<T>> sucessorSelector) {
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
