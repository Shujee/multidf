using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiDF.VM;
using Microsoft.Office.Interop.Word;

namespace MultiDF.Test
{
  class TestWordService : IWordService
  {
    private readonly Application App;

    public TestWordService()
    {
      App = new Application();
    }

    public string ActiveDocumentPath => (App.Documents.Count > 0 ? App.ActiveDocument.FullName : null);

    public void CreateMergedDocument(string[] docs, string outputPath)
    {
      throw new NotImplementedException();
    }

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


          Result.Add(new WordParagraph(p.Range.Text, p.Range.Start, p.Range.End, PType, 0, 0, 0, 0));
          progressCallback?.Invoke(i, Doc.Paragraphs.Count);
        }

        Doc.Close(SaveChanges: false);

        return Result;
      }
      else
        return null;
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true)
    {
      throw new NotImplementedException();
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
