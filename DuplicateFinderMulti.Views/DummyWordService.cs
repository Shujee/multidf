using DuplicateFinderMulti.VM;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.Views
{
  public class DummyWordService : IWordService
  {
    public string ActiveDocumentPath => throw new NotImplementedException();

    public void CreateMergedDocument(string[] docs, string outputPath, bool closeAfterCreate)
    {
      throw new NotImplementedException();
    }

    public void ExportDocumentToFixedFormat(ExportFixedFormat format, string docPath, string outputPath, bool closeAfterDone)
    {
      throw new NotImplementedException();
    }

    public void FixQANumbers(string docPath, List<WordParagraph> delimiterParagraphs, bool closeAfterDone)
    {
      throw new NotImplementedException();
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true)
    {
      List<WordParagraph> Result = new List<WordParagraph>();

      var Lines = Properties.Resources.SampleText.Split('\n');

      int i = 1;
      foreach (var Line in Lines)
      {
        var WP = new WordParagraph(Line, i, i + Line.Length, ParagraphType.Text, 0, 0, 0, 0);
        i = WP.End + 1;

        progressCallback?.Invoke(i, Lines.Length);
        Result.Add(WP);
      }

      return Result;
    }

    public void GoToParagraph(int para)
    {
      throw new NotImplementedException();
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      throw new NotImplementedException();
    }
  }
}