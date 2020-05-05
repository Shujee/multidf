using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiDF.VM;
using Microsoft.Office.Interop.Word;

namespace MultiDF.TestingShell
{
  class TestWordService : VM.IWordService
  {
    private readonly Application App;
    private Selection Sel => App.Selection;

    public TestWordService()
    {
      App = new Application();
    }

    public string ActiveDocumentPath => (App.Documents.Count > 0 ? App.ActiveDocument.FullName : null);

    public int? SelectionStart => throw new NotImplementedException();

    public int? CurrentParagraphNumber => throw new NotImplementedException();

    /// <summary>
    /// Creates a single Word document by merge content of all the specified documents. This function also updates QA numbering to make it continuous.
    /// </summary>
    /// <param name="docs"></param>
    /// <returns></returns>
    public void CreateMergedDocument(string[] docs, string outputPath, bool closeAfterCreate)
    {
      var MergeDoc = App.Documents.Add(Visible: false);
      Range R = MergeDoc.Range();

      for (int i = 0; i < docs.Length; i++)
      {
        R.InsertFile(docs[i]);

        //Add a paragraph mark after each file except for the last file.
        if (i < docs.Length - 1)
          R.InsertParagraphAfter();

        R = R.Paragraphs.Last.Range;
      }

      MergeDoc.SaveAs(outputPath);

      if (closeAfterCreate)
        MergeDoc.Close();
    }

    public void ExportDocumentToFixedFormat(ExportFixedFormat format, string docPath, string outputPath, bool closeAfterDone)
    {
      var OpenResult = GetOrOpenDocument(docPath, false);

      if (OpenResult.doc != null)
      {
        OpenResult.doc.ExportAsFixedFormat(
                                OutputFileName: outputPath,
                                ExportFormat: (format == ExportFixedFormat.XPS ? WdExportFormat.wdExportFormatXPS : WdExportFormat.wdExportFormatPDF), 
                                OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForOnScreen);

        if (!OpenResult.alreadyOpen)
          OpenResult.doc.Close();
      }
    }

    private (Document doc, bool alreadyOpen) GetOrOpenDocument(string docPath, bool visible)
    {
      var Doc = App.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
      {
        try
        {
          Doc = App.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: visible);
          return (Doc, false);
        }
        catch (Exception ee)
        {
          ViewModelLocator.DialogService.ShowMessage("The following error occurred while trying to open specified document: " + ee.Message, true);
          return (null, false);
        }
      }
      else
        return (Doc, true);
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true)
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


          Result.Add(new WordParagraph(R.Text, R.Start, R.End, PType, 0, 0, 0, 0));

          ViewModelLocator.Main.UpdateProgress(false, "Importing...", i / (float)ParaCount);
          progressCallback?.Invoke(i++, ParaCount);

          if (token.IsCancellationRequested)
            break;
        }

        if (!AlreadyOpen)
        {
          System.Threading.Tasks.Task.Run(() => Doc.Close(SaveChanges: false));
        }

        if (!AlreadyOpen && closeAfterDone)
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
        Sel.Start = App.ActiveDocument.Paragraphs[para].Range.Start;
    }

    public void OpenDocument(string docPath, bool openReadonly, int? start, int? end)
    {
      var Doc = App.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
        Doc = App.Documents.Open(docPath);

      if (start != null)
        Sel.Start = start.Value;

      if (end != null)
        Sel.End = end.Value;
    }

    public Dictionary<int,int> FixAllQANumbers(string docPath, List<WordParagraph> delimiterParagraphs)
    {
      throw new NotImplementedException();
    }
  }
}
