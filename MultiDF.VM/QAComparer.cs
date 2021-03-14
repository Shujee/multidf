using System;
using System.Collections.Generic;
using System.Linq;
using VMBase;

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
      var QDist = (Fastenshtein.Levenshtein.Distance(q1.QuestionUpper, q2.QuestionUpper) / (float)Math.Max(q1.QuestionUpper.Length, q2.QuestionUpper.Length));

      var ChoicesDist = CalcSetDistance(q1.ChoicesUpper, q2.ChoicesUpper, Fastenshtein.Levenshtein.Distance);

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