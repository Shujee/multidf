using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFinderMulti.VM;
using Microsoft.Office.Interop.Word;

namespace DuplicateFinderMulti.TestingShell
{
  class TestWordService : VM.IWordService
  {
    private readonly Application App;

    public TestWordService()
    {
      App = new Application();
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback)
    {
      if (!string.IsNullOrEmpty(docPath) && System.IO.File.Exists(docPath))
      {
        List<WordParagraph> Result = new List<WordParagraph>();
        Document Doc;
        int ParaCount;

        try
        {
          Doc = App.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: false);
        }
        catch (Exception ee)
        {
          ViewModelLocator.DialogService.ShowMessage("The following error occurred while trying to open specified document: " + ee.Message, true);
          return null;
        }

        ParaCount = Doc.Paragraphs.Count;
        for (int i = 1; i <= ParaCount; i++)
        {
          var PType = ParagraphType.Text;
          var Para = ((Paragraph)Doc.Paragraphs[i]).Range;

          if (Para.Tables.Count > 0)
          {
            if (Para.Tables[1].ApplyStyleHeadingRows && Para.Rows.First.Index == 1)
              PType = ParagraphType.TableHeader;
            else
              PType = ParagraphType.TableRow;
          }
          else if (Para.ListFormat.ListType == WdListType.wdListSimpleNumbering)
            PType = ParagraphType.NumberedList;


          Result.Add(new WordParagraph(Para.Text, Para.Start, Para.End, PType));

          if (i % 5 == 0) //keep indicating progress once in a while
            progressCallback?.Invoke(i, ParaCount);
        }

        Doc.Close(SaveChanges: false);
        progressCallback?.Invoke(ParaCount, ParaCount);

        return Result;
      }
      else
        return null;
    }

    public void GoToParagraph(int para)
    {
      if (App.ActiveDocument != null)
        App.Selection.Start = App.ActiveDocument.Paragraphs[para].Range.Start;
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      var Doc = App.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
        Doc = App.Documents.Open(docPath);

      if (start != null)
        App.Selection.Start = start.Value;

      if(end != null)
        App.Selection.End = end.Value;
    }
  }
}
