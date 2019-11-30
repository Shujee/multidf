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
      if (string.IsNullOrEmpty(docPath) || !System.IO.File.Exists(docPath))
        return null;
      else
      {
        List<WordParagraph> Result = new List<WordParagraph>();
        Document Doc;
        int ParaCount;

        bool AlreadyOpen = false;
        Doc = App.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

        if (Doc == null)
        {
          try
          {
            Doc = App.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: false);
          }
          catch (Exception ee)
          {
            ViewModelLocator.DialogService.ShowMessage("The following error occurred while trying to open specified document: " + ee.Message, true);
            return null;
          }
        }
        else
          AlreadyOpen = true;

        int i = 0;
        ParaCount = Doc.Paragraphs.Count;
        foreach (Paragraph p in Doc.Paragraphs)
        {
          var R = p.Range;

          var PType = ParagraphType.Text;

          if (R.Tables.Count > 0)
          {
            try
            {
              if (R.Tables[1].ApplyStyleHeadingRows && R.Rows.First.Index == 1)
                PType = ParagraphType.TableHeader;
              else
                PType = ParagraphType.TableRow;
            }
            catch //tables with vertically merged rows can throw exception when trying to access R.Rows.First.Index 
            {
              PType = ParagraphType.TableRow;
            }
          }
          else if (R.ListFormat.ListType == WdListType.wdListSimpleNumbering || R.ListFormat.ListType == WdListType.wdListOutlineNumbering)
            PType = ParagraphType.NumberedList;


          Result.Add(new WordParagraph(R.Text, R.Start, R.End, PType));

          ViewModelLocator.Main.UpdateProgress(false, "Importing...", i / (float)ParaCount);
          progressCallback?.Invoke(i++, ParaCount);

          if (token.IsCancellationRequested)
            break;
        }

        if (!AlreadyOpen)
        {
          System.Threading.Tasks.Task.Run(() => Doc.Close(SaveChanges: false));
        }

        progressCallback?.Invoke(ParaCount, ParaCount);

        return Result;
      }
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

      if (end != null)
        App.Selection.End = end.Value;
    }
  }
}
