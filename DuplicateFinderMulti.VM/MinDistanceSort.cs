using System;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateFinderMulti.VM
{
  static class MinDistanceSort
  {
    /// <summary>
    /// Sorts the smaller of the two lists such that each element acquires the index of its closest match in the bigger list.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void Sort(List<string> inputa, List<string> inputb, Func<string, string, int> DistFunc)
    {
      if (inputa == null || inputb == null || inputa.Count == 0 || inputb.Count == 0)
        return;

      List<string> A, B;

      if (inputb.Count > inputa.Count)
      {
        A = inputb.ToList();
        B = inputa.ToList();
      }
      else
      {
        A = inputb.ToList();
        B = inputa.ToList();
      }

      List<string> OutputB = new List<string>(B.Count);

      for (int i = 0; i < A.Count; i++)
      {
        var MatchIndex = B.MinDistIndex(A[i], DistFunc);
        OutputB.Add(B[MatchIndex]);
        B.RemoveAt(MatchIndex);
      }

      for (int i = 0; i < B.Count; i++)
      {
        OutputB.Add(B[i]);
      }

      if (inputb.Count > inputa.Count)
      {
        for (int i = 0; i < B.Count; i++)
          inputb[i] = OutputB[i];
      }
      else
      {
        for (int i = 0; i < OutputB.Count; i++)
          inputa[i] = OutputB[i];
      }
    }
 
    private static int MinDistIndex(this IEnumerable<string> sequence, string text, Func<string, string, int> distFunc)
    {
      int minIndex = -1;
      double minDist = 0;

      int index = 0;
      foreach (var value in sequence)
      {
        double Dist = distFunc(value, text);
        if (Dist < minDist || minIndex == -1)
        {
          minIndex = index;
          minDist = Dist;
        }

        index++;
      }

      return minIndex;
    }
  }
}