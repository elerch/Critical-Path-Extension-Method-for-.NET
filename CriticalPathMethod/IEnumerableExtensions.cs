using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CriticalPathMethod
{
    public static class IEnumerableExtensions
    {
        private static IEnumerable<Activity> OrderByDependencies(IEnumerable<Activity> list)
        {
            var processedActivities = new HashSet<Activity>();
            var totalCount = list.Count();
            var rc = new List<Activity>(totalCount);
            while (rc.Count < totalCount) {
                foreach (var activity in list) {
                    if (!processedActivities.Contains(activity)
                        && activity.Predecessors.All(processedActivities.Contains)) {
                        rc.Add(activity);
                        processedActivities.Add(activity);
                        yield return activity;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the walk ahead inside the array of activities calculating for each
        /// activity its earliest start time and earliest end time.
        /// </summary>
        /// <param name="list">Array storing the activities already entered.</param>
        /// <returns>list</returns>
        private static void WalkListAhead(IEnumerable<Activity> list)
        {
            var firstItem = list.FirstOrDefault();
            if (firstItem == null) return;

            foreach (var activity in list) {
                foreach (var predecessor in activity.Predecessors) {
                    if (activity.EarliestStartTime < predecessor.EarliestEndTime)
                        activity.EarliestStartTime = predecessor.EarliestEndTime;
                }
                activity.EarliestEndTime = activity.EarliestStartTime + activity.Duration;
            }
        }

        /// <summary>
        /// Performs the reverse walk inside the array of activities calculating for each
        /// activity its latest start time and latest end time.  Must be called after the
        /// forward walk
        /// </summary>
        /// <param name="list">Array storing the activities already entered.</param>
        /// <returns>list</returns>
        private static void WalkListAback(IEnumerable<Activity> list)
        {
            var reversedList = list.Reverse();
            var isFirst = true;

            foreach (var activity in reversedList) {
                if (isFirst) {
                    activity.LatestEndTime = activity.EarliestEndTime;
                    isFirst = false;
                }

                foreach (Activity successor in activity.Successors) {
                    if (activity.LatestEndTime == 0)
                        activity.LatestEndTime = successor.LatestStartTime;
                    else
                        if (activity.LatestEndTime > successor.LatestStartTime)
                            activity.LatestEndTime = successor.LatestStartTime;
                }

                activity.LatestStartTime = activity.LatestEndTime - activity.Duration;
            }
        }

        /// <summary>
        /// Calculates the critical path by verifyng if each activity's earliest end time
        /// minus the latest end time and earliest start time minus the latest start
        /// time are equal zero. If so, then prints out the activity id that match the
        /// criteria. Plus, prints out the project's total duration. 
        /// </summary>
        /// <param name="list">Array containg the activities already entered.</param>
        private static IEnumerable<Activity> CriticalPathInternal(IEnumerable<Activity> list)
        {
            var sb = new StringBuilder();
            var rc = new List<Activity>();
            foreach (Activity activity in list) {
                if ((activity.EarliestEndTime - activity.LatestEndTime == 0) && (activity.EarliestStartTime - activity.LatestStartTime == 0)) {
                    // This activity is on the critical path
                    Console.Write("{0} ", activity.Id);
                    sb.AppendFormat("{0} ", activity.Id);
                    rc.Add(activity);
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("\r\n" + list.Last().EarliestEndTime);
            var output = System.IO.File.ReadAllText("output.txt");
            Console.Write("\n\n         Total duration: {0}\n\n", list.Last().EarliestEndTime);
            System.Diagnostics.Debug.Assert(sb.ToString().CompareTo(output.Trim()) == 0);
            return rc;
        }

        public static IEnumerable<Activity> CriticalPath(this IEnumerable<Activity> list) {
            var orderedList = OrderByDependencies(list);
            WalkListAhead(orderedList);
            WalkListAback(orderedList);
            return CriticalPathInternal(orderedList);            
        }


    }
}
