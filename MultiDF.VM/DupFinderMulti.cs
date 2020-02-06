using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using System.Collections.Concurrent;
using System.IO;


namespace MultiDF.VM
{
  class DupFinderMulti : IDupFinder
  {
    public List<WordParagraph[]> Find(XMLDoc p, int maxDistance, bool ignoreCase, Action<int, int, string, bool> updateStatusLabel, CancellationToken tok)
    {
      Func<string, string, int> DistFunc;

      if (ignoreCase)
        DistFunc = CalcLevenshteinDistanceIgnoreCase;
      else
        DistFunc = CalcLevenshteinDistance;

      if (p.QAs.Count > 1)
      {
        updateStatusLabel(20, 100, "Filtering small paragraphs", true);

        Dictionary<WordParagraph, List<WordParagraph>> Neighbors = new Dictionary<WordParagraph, List<WordParagraph>>();
        
        updateStatusLabel(25, 100, "Performing fuzzy matching", true);

        Stopwatch MyStopwatch = new Stopwatch();
        MyStopwatch.Start();

        for (int i = 0; i < p.QAs.Count; i++)
        {
          var MyGroup = Neighbors.FirstOrDefault(n => (DistFunc(n.Key.Text, p.QAs[i].Question) / (float)p.QAs[i].Question.Length) * 100 < maxDistance);

          if (MyGroup.Key != null)
          {
            MyGroup.Value.Add(p.QAs[i]);
            paras[i].Distance = (int)Math.Round((DistFunc(MyGroup.Key.Text, p.QAs[i].Question) / (float)p.QAs[i].Question.Length) * 100, 0);
          }
          else
          {
            var NewList = new List<WordParagraph> { paras[i] };
            Neighbors.Add(paras[i], NewList);
            p.QAs[i].Distance = 0;
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

        if(p.QAs.Count > 0)
          NewList.Add(new[] { p.QAs[0] });

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


  
  class aabc
  {
    public partial class Form2 : Form
    {
      public string startfiledir { get; private set; }
      public string[] fileContent { get; private set; }
      public string saveFolder { get; private set; }
      public string filePath { get; private set; }

      private ConcurrentDictionary<string, StreamWriter> writers = new ConcurrentDictionary<string, StreamWriter>();

      OpenFileDialog openFileDialog = new OpenFileDialog();

      private void Button1_Click(object sender, EventArgs e)
      {

        this.button1.Enabled = false;
        Refresh();

        openFileDialog.InitialDirectory = startfiledir;
        openFileDialog.Filter = "txt files (*.txt)|*.txt";
        openFileDialog.FilterIndex = 2;
        openFileDialog.RestoreDirectory = true;

        openFileDialog.ShowDialog();

        //Get the path of specified file
        filePath = openFileDialog.FileName;

        fileContent = File.ReadAllLines(filePath);

        //show the button again
        this.button1.Enabled = Enabled;
        Refresh();


      }


      private void SplitDatabutton_Click(object sender, EventArgs e)
      {
        //float splitNum = Int32.Parse(numToSplit.Text);
        float splitNum = 100000;

        var Tasks = System.Threading.Tasks.Parallel.For(0, fileContent.Length, (i) =>
        {
          string MyFile = Path.Combine(saveFolder, ((int)(i / ((float)splitNum))).ToString("0000") + ".txt");
          writers.GetOrAdd(MyFile, File.AppendText(MyFile)).WriteLine(fileContent[i]);
        });

        foreach (var writer in writers)
        {
          writer.Value.Close();
        }
      }
    }
  }
}


