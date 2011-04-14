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
      public Activity()
    {
      // TODO: Add Constructor Logic here
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
      public int Est { get; set; }

      /// <summary>
      ///  Latest start time
      /// </summary>
      public int Lst { get; set; }

      /// <summary>
      /// Earliest end time
      /// </summary>
      public int Eet { get; set; }


      /// <summary>
      /// Latest end time
      /// </summary>
      public int Let { get; set; }

      /// <summary>
      /// Activities that come before the activity.
      /// </summary>
      public Activity[] Predecessors { get; set; }

      /// <summary>
      /// Activities that come after the activity.
      /// </summary>
      public Activity[] Successors { get; set; }

      /// <summary>
    /// Performs a check to verify if an activity exists.
    /// </summary>
    /// <param name="list">Array storing the activities already entered.</param>
    /// <param name="id">ID being checked.</param>
    /// <param name="i">Current activities' array index.</param>
    /// <returns>Found activity or null.</returns>
    public Activity CheckActivity(Activity[] list, string id, int i)
    {
      for(int j = 0; j < i; j++)
      {
        if(list[j].Id == id)
          return list[j];
      }
      return null;
    }

    /// <summary>
    /// Returns the index of a given activity.
    /// </summary>
    /// <param name="aux">Activity serving as an auxiliary referencing an existing
    /// activity.</param>
    /// <param name="i">Current activities' array index.</param>
    /// <returns>index</returns>
    public int GetIndex(Activity[] list, Activity aux, int i)
    {
      for(int j = 1; j < i; j++)
      {
        if(list[j].Id == aux.Id)
          return j;
      }
      return 0;
    }

    /// <summary>
    /// Fills out the aux's array of successors by checking if it has already been
    /// filled up.
    /// If so, instantiates a new aux2 that'll store the aux's current
    /// successors plus the activity that's being entered. After that, aux receives the
    /// reference of aux2.
    /// Otherwise, store the activity being entered in the first index of aux's
    /// successors array.  
    /// </summary>
    /// <param name="aux">Activity serving as an auxiliary referencing an existing
    /// activity.</param>
    /// <param name="activity">Activity being entered.</param>
    /// <returns>aux</returns>
    public Activity SetSuccessors(Activity aux, Activity activity)
    {
      if(aux.Successors != null)
      {
        var aux2 = new Activity();
        aux2.Successors = new Activity[aux.Successors.Length + 1];
        aux.Successors.CopyTo(aux2.Successors, 0);
        aux2.Successors[aux.Successors.Length] = activity;
        aux.Successors = aux2.Successors;
      }
      else
      {
        aux.Successors = new Activity[1];
        aux.Successors[0] = activity;
      }
      return aux;
    }
  }
}

