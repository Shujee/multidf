using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFinderMulti.VM;
using Microsoft.Office.Interop.Word;

namespace DuplicateFinderMulti.Test
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

        int i = 0;
        foreach (Paragraph p in Doc.Paragraphs)
        {
          var PType = ParagraphType.Text;

          if (p.Range.Tables.Count > 0)
          {
            if (p.Range.Tables[1].ApplyStyleHeadingRows && p.Range.Rows.First.Index == 1)
              PType = ParagraphType.TableHeader;
            else
              PType = ParagraphType.TableRow;
          }
          else if (p.Range.ListFormat.ListType == WdListType.wdListSimpleNumbering)
            PType = ParagraphType.NumberedList;


          Result.Add(new WordParagraph(p.Range.Text, p.Range.Start, p.Range.End, PType));
          progressCallback?.Invoke(i, Doc.Paragraphs.Count);
        }

        Doc.Close(SaveChanges: false);

        return Result;
      }
      else
        return null;
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
