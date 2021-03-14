using System;

namespace MultiDF.VM
{
  internal static class Levenshtein
  {
    /// <summary>
    /// Computes Levenshtein Distance between specified strings. Levenshtein Distance is a measure of similarity between two string.
    /// </summary>
    /// <param name="a">First string</param>
    /// <param name="b">Second string</param>
    /// <param name="ignoreCase">Determines whether character casing should be ignored (A = a?)</param>
    /// <returns></returns>
    //public static int CalcLevenshteinDistance(string a, string b)
    //{
      //if (ignoreCase)
      //{
      //  a = a.ToUpperInvariant();
      //  b = b.ToUpperInvariant();
      //}

      //return Fastenshtein.Levenshtein.Distance(a, b);

      //if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
      //  return 0;

      //if (string.IsNullOrEmpty(a))
      //  return b.Length;

      //if (string.IsNullOrEmpty(b))
      //  return a.Length;

      //int lengthA = a.Length;
      //int lengthB = b.Length;
      //var distances = new int[lengthA + 1, lengthB + 1];
      //for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
      //for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

      //if (ignoreCase)
      //{
      //  a = a.ToUpperInvariant();
      //  b = b.ToUpperInvariant();
      //}

      //for (int i = 1; i <= lengthA; i++)
      //{
      //  for (int j = 1; j <= lengthB; j++)
      //  {
      //    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
      //    distances[i, j] = Math.Min
      //        (
      //        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
      //        distances[i - 1, j - 1] + cost
      //        );
      //  }
      //}

      //return distances[lengthA, lengthB];
    //}
  }
}
