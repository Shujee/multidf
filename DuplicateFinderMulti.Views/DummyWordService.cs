using DuplicateFinderMulti.VM;
using System.Collections.Generic;

namespace DuplicateFinderMulti.Views
{
  public class DummyWordService : IWordService
  {
    public List<WordParagraph> GetDocumentParagraphs(string docPath)
    {
      List<WordParagraph> Result = new List<WordParagraph>();

      var Lines = Properties.Resources.SampleText.Split('\n');

      int i = 1;
      foreach (var Line in Lines)
      {
        var WP = new WordParagraph()
        {
          Start = i,
          End = i + Line.Length,
          Text = Line
        };

        i = WP.End + 1;

        Result .Add(WP);
      }

      return Result;
    }

    public void OpenDocument(string docPath)
    {
      
    }
  }
}