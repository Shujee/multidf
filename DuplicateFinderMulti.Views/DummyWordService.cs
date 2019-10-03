using DuplicateFinderMulti.VM;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.Views
{
  public class DummyWordService : IWordService
  {
    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback)
    {
      List<WordParagraph> Result = new List<WordParagraph>();

      var Lines = Properties.Resources.SampleText.Split('\n');

      int i = 1;
      foreach (var Line in Lines)
      {
        var WP = new WordParagraph(Line, i, i + Line.Length, ParagraphType.Text);
        i = WP.End + 1;

        progressCallback?.Invoke(i, Lines.Length);
        Result.Add(WP);
      }

      return Result;
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      
    }
  }
}