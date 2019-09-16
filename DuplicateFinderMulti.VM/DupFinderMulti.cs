using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  class DupFinder : IDupFinder
  {
    public List<WordParagraph[]> Find(List<WordParagraph> paras, int maxDistance, bool ignoreCase, int? minParaLength, Action<int, int, string, bool> updateStatusLabel, CancellationToken tok)
    {
      Func<string, string, int> DistFunc;

      if (ignoreCase)
        DistFunc = CalcLevenshteinDistanceIgnoreCase;
      else
        DistFunc = CalcLevenshteinDistance;

      if (paras.Count > 1)
      {
        updateStatusLabel(20, 100, "Filtering small paragraphs", true);

        if (minParaLength.HasValue)
        {
          //Drop all paragraphs that are shorter than min. paragraph length setting.
          paras.RemoveAll(p => p.Text.Length < minParaLength.Value);
        }

        Dictionary<WordParagraph, List<WordParagraph>> Neighbors = new Dictionary<WordParagraph, List<WordParagraph>>();
        var diffBuilder = new InlineDiffBuilder(new Differ());

        updateStatusLabel(25, 100, "Performing fuzzy matching", true);

        Stopwatch MyStopwatch = new Stopwatch();
        MyStopwatch.Start();

        for (int i = 0; i < paras.Count; i++)
        {
          var MyGroup = Neighbors.FirstOrDefault(n => (DistFunc(n.Key.Text, paras[i].Text) / (float)paras[i].Text.Length) * 100 < maxDistance);

          if (MyGroup.Key != null)
          {
            MyGroup.Value.Add(paras[i]);
            paras[i].Distance = (int)Math.Round((DistFunc(MyGroup.Key.Text, paras[i].Text) / (float)paras[i].Text.Length) * 100, 0);
          }
          else
          {
            var NewList = new List<WordParagraph> { paras[i] };
            Neighbors.Add(paras[i], NewList);
            paras[i].Distance = 0;
          }

          var Timespent = MyStopwatch.Elapsed.ToString(@"hh\:mm\:ss");
          updateStatusLabel((int)(25 + (i / (float)paras.Count) * 70), 100, $"Matching ({Timespent})", false);

          tok.ThrowIfCancellationRequested();
        }

        MyStopwatch.Stop();
        updateStatusLabel(100, 100, "Completed", true);

        if (!tok.IsCancellationRequested)
          return Neighbors.Where(i => i.Value.Count > 1).Select(i => i.Value.ToArray()).ToList();
        else
          return null;
      }
      else
      {
        List<WordParagraph[]> NewList = new List<WordParagraph[]>();

        if(paras.Count > 0)
          NewList.Add(new[] { paras[0] });

        return NewList;
      }
    }

    /// <summary>
    /// Computes Levenshtein Distance between specified strings. Levenshtein Distance is a measure of similarity between two string.
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
