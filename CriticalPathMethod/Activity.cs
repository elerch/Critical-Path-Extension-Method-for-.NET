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
using System.Collections.Generic;
using System.Text;

namespace CriticalPathMethod
{
    /// <summary>
    /// Describes an activity according to the Critical Path Method.
    /// </summary>
    public class Activity
    {
        public Activity() {
            Predecessors = new List<Activity>();
            Successors = new List<Activity>();
        }
        /// <summary>
        /// Identification concerning the activity.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Brief description concerning the activity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Total amount of time taken by the activity.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Earliest start time
        /// </summary>
        public int EarliestStartTime { get; set; }

        /// <summary>
        ///  Latest start time
        /// </summary>
        public int LatestStartTime { get; set; }

        /// <summary>
        /// Earliest end time
        /// </summary>
        public int EarliestEndTime { get; set; }


        /// <summary>
        /// Latest end time
        /// </summary>
        public int LatestEndTime { get; set; }

        /// <summary>
        /// Activities that come before the activity.
        /// </summary>
        public ICollection<Activity> Predecessors { get; private set; }

        /// <summary>
        /// Activities that come after the activity.
        /// </summary>
        public ICollection<Activity> Successors { get; private set; }
        public override string ToString() {
            return Id;
        }
    }
}

