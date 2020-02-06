using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiDF.VM
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
        DistFunc = Levenshtein.CalcLevenshteinDistanceIgnoreCase;
      else
        DistFunc = Levenshtein.CalcLevenshteinDistance;

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
  }
}