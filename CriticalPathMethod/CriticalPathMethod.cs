//
//	CPM - Critical Path Method C# Sample Application
//	Copyright ©2006 Leniel Braz de Oliveira Macaferi & Wellington Magalhães Leite.
//
//  UBM COMPUTER ENGINEERING - 7TH TERM [http://www.ubm.br/]
//  This program sample was developed and turned in as a term paper for Lab. of
//  Software Engineering.
//  The source code is provided "as is" without warranty.
//

using System;
using System.Collections;
using System.Text;
using CriticalPathMethod;
using System.Collections.Generic;

namespace ComputerEngineering
{
    class CPM
    {
        /// <summary>
        /// Number of activities
        /// </summary>
        private static int na;

        private static void Main(string[] args)
        {
            // Array to store the activities that'll be evaluated.
            Activity[] list = null;

            list = GetActivities(list);
            list = WalkListAhead(list);
            list = WalkListAback(list);

            CriticalPath(list);
        }

        /// <summary>
        /// Gets the activities that'll be evaluated by the critical path method.
        /// </summary>
        /// <param name="list">Array to store the activities that'll be evaluated.</param>
        /// <returns>list</returns>
        private static Activity[] GetActivities(Activity[] list)
        {
            var input = System.IO.File.ReadAllLines("input.txt");
            var ad = new Dictionary<string, Activity>();
            Console.Write("\n       Number of activities: " + input.Length);
            na = input.Length;
            list = new Activity[na];
            int inx = 0;
            foreach (var line in input) {
                var activity = new Activity();
                var elements = line.Split(' ');
                Console.WriteLine("\n                Activity {0}\n", inx + 1);

                activity.Id = elements[0];
                Console.WriteLine("\n                     ID: " + activity.Id);
                ad.Add(activity.Id, activity);
                activity.Description = elements[1];
                Console.WriteLine("            Description: " + activity.Description);

                activity.Duration = int.Parse(elements[2]);
                Console.WriteLine("               Duration: " + activity.Duration);

                int np = int.Parse(elements[3]);
                Console.WriteLine(" Number of predecessors: ", np);

                if (np != 0) {
                    activity.Predecessors = new Activity[np];

                    for (int j = 0; j < np; j++) {
                        var id = elements[4 + j];
                        Console.WriteLine("    #{0} predecessor's ID: " + id, j + 1);
                        
                        if (!ad.ContainsKey(id)) throw new InvalidOperationException();
                        var aux = ad[id];

                        activity.Predecessors[j] = aux;

                        list[Activity.GetIndex(list, aux, inx)] = aux.SetSuccessors(aux, activity);
                    }
                }
                list[inx++] = activity;
            }

            return list;
        }

        /// <summary>
        /// Performs the walk ahead inside the array of activities calculating for each
        /// activity its earliest start time and earliest end time.
        /// </summary>
        /// <param name="list">Array storing the activities already entered.</param>
        /// <returns>list</returns>
        private static Activity[] WalkListAhead(Activity[] list)
        {
            list[0].EarliestEndTime = list[0].EarliestStartTime + list[0].Duration;

            for (int i = 1; i < na; i++) {
                foreach (Activity activity in list[i].Predecessors) {
                    if (list[i].EarliestStartTime < activity.EarliestEndTime)
                        list[i].EarliestStartTime = activity.EarliestEndTime;
                }

                list[i].EarliestEndTime = list[i].EarliestStartTime + list[i].Duration;
            }

            return list;
        }

        /// <summary>
        /// Performs the walk aback inside the array of activities calculating for each
        /// activity its latest start time and latest end time.
        /// </summary>
        /// <param name="list">Array storing the activities already entered.</param>
        /// <returns>list</returns>
        private static Activity[] WalkListAback(Activity[] list)
        {
            list[na - 1].LatestEndTime = list[na - 1].EarliestEndTime;
            list[na - 1].LatestStartTime = list[na - 1].LatestEndTime - list[na - 1].Duration;

            for (int i = na - 2; i >= 0; i--) {
                foreach (Activity activity in list[i].Successors) {
                    if (list[i].LatestEndTime == 0)
                        list[i].LatestEndTime = activity.LatestStartTime;
                    else
                        if (list[i].LatestEndTime > activity.LatestStartTime)
                            list[i].LatestEndTime = activity.LatestStartTime;
                }

                list[i].LatestStartTime = list[i].LatestEndTime - list[i].Duration;
            }

            return list;
        }

        /// <summary>
        /// Calculates the critical path by verifyng if each activity's earliest end time
        /// minus the latest end time and earliest start time minus the latest start
        /// time are equal zero. If so, then prints out the activity id that match the
        /// criteria. Plus, prints out the project's total duration. 
        /// </summary>
        /// <param name="list">Array containg the activities already entered.</param>
        private static void CriticalPath(Activity[] list)
        {
            var sb = new StringBuilder();
            Console.Write("\n          Critical Path: ");

            foreach (Activity activity in list) {
                if ((activity.EarliestEndTime - activity.LatestEndTime == 0) && (activity.EarliestStartTime - activity.LatestStartTime == 0)) {
                    // This activity is on the critical path
                    Console.Write("{0} ", activity.Id);
                    sb.AppendFormat("{0} ", activity.Id);
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("\r\n" + list[list.Length - 1].EarliestEndTime);
            var output = System.IO.File.ReadAllText("output.txt");
            Console.Write("\n\n         Total duration: {0}\n\n", list[list.Length - 1].EarliestEndTime);
            System.Diagnostics.Debug.Assert(sb.ToString().CompareTo(output.Trim()) == 0);
        }
    }
}
