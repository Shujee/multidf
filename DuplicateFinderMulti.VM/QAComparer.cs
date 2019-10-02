using System;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateFinderMulti.VM
{
  public class DefaultQAComparer : IQAComparer
  {
    /// <summary>
    /// Weight of the Choices section when computing distance between two QAs.
    /// </summary>
    public double ChoiceSectionWeightage => .1;

    public double Distance(QA q1, QA q2, bool ignoreCase)
    {
      Func<string, string, int> DistFunc;

      if (ignoreCase)
        DistFunc = CalcLevenshteinDistanceIgnoreCase;
      else
        DistFunc = CalcLevenshteinDistance;

      var QDist = (DistFunc(q1.Question, q2.Question) / (float)Math.Max(q1.Question.Length, q2.Question.Length));

      var ChoicesDist = CalcSetDistance(q1.Choices, q2.Choices, DistFunc);

      return QDist * (1 - ChoiceSectionWeightage) + ChoicesDist * ChoiceSectionWeightage;
    }

    private double CalcSetDistance(List<string> choices1, List<string> choices2, Func<string, string, int> distFunction)
    {
      if (choices1.Count == 0 && choices2.Count == 0)
        return 0;
      else if (choices1.Count == 0 || choices2.Count == 0)
        return 1;
      else
      {
        //for each choice in first list, we'll try to find its closest cousin in the second list
        if (choices1.Count >= choices2.Count)
          return choices1.Select(c1 => choices2.Min(c2 => distFunction(c1, c2) / (float)Math.Max(c1.Length, c2.Length))).Average();
        else
          return choices2.Select(c2 => choices1.Min(c1 => distFunction(c1, c2) / (float)Math.Max(c1.Length, c2.Length))).Average();
      }
    }

    /// <summary>   bnvLevenshtein Distance is a measure of similarity between two string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int CalcLevenshteinDistance(string a, string b)
    {
      if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
        return 0;

      if (String.IsNullOrEmpty(a))
        return b.Length;

      if (String.IsNullOrEmpty(b))
        return a.Length;

      int lengthA = a.Length;
      int lengthB = b.Length;
      var distances = new int[lengthA + 1, lengthB + 1];
      for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
      for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

      for (int i = 1; i <= lengthA; i++)
      {
        for (int j = 1; j <= lengthB; j++)
        {
          int cost = b[j - 1] == a[i - 1] ? 0 : 1;
          distances[i, j] = Math.Min
              (
              Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
              distances[i - 1, j - 1] + cost
              );
        }
      }

      return distances[lengthA, lengthB];
    }

    /// <summary>
    /// Computes Levenshtein Distance between specified strings. Levenshtein Distance is a measure of similarity between two string. This version is not case sensitive.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int CalcLevenshteinDistanceIgnoreCase(string a, string b)
    {
      if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
        return 0;

      if (String.IsNullOrEmpty(a))
        return b.Length;

      if (String.IsNullOrEmpty(b))
        return a.Length;

      int lengthA = a.Length;
      int lengthB = b.Length;
      var distances = new int[lengthA + 1, lengthB + 1];
      for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
      for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

      for (int i = 1; i <= lengthA; i++)
      {
        for (int j = 1; j <= lengthB; j++)
        {
          int cost = Char.ToUpperInvariant(b[j - 1]) == Char.ToUpperInvariant(a[i - 1]) ? 0 : 1;
          distances[i, j] = Math.Min
              (
              Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
              distances[i - 1, j - 1] + cost
              );
        }
      }

      return distances[lengthA, lengthB];
    }
  }
}
