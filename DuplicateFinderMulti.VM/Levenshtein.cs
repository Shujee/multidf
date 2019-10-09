using System;

namespace DuplicateFinderMulti.VM
{
  internal static class Levenshtein
  {
    /// <summary>   bnvLevenshtein Distance is a measure of similarity between two string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int CalcLevenshteinDistance(string a, string b)
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
    public static int CalcLevenshteinDistanceIgnoreCase(string a, string b)
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
