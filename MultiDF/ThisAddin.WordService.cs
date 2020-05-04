using Microsoft.Office.Interop.Word;
using MultiDF.VM;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;

namespace MultiDF
{
  public partial class ThisAddIn : IWordService
  {
    private Document ActiveDoc => Globals.ThisAddIn.Application.Documents.Count > 0 ? Globals.ThisAddIn.Application.ActiveDocument : null;
    private Selection Sel => Globals.ThisAddIn.Application.Selection;

    public string ActiveDocumentPath => ActiveDoc?.FullName;
    public int? SelectionStart => Sel?.Start;

    /// <summary>
    /// Returns the index of the paragraph where the caret is currently located.
    /// </summary>
    public int? CurrentParagraphNumber
    {
      get
      {
        if (ActiveDoc == null || Sel == null)
          return null;
        else
        {
          if (Sel.Start == 0)
            return 1;
          else
          {
            Range R = ActiveDoc.Range(1, Sel.Start);
            return R.Paragraphs.Count;
          }
        }
      }
    }

    public void ExportDocumentToFixedFormat(ExportFixedFormat format, string docPath, string outputPath, bool closeAfterDone)
    {
      var OpenResult = GetOrOpenDocument(docPath, false, true);

      if (OpenResult.doc != null)
      {
        OpenResult.doc.ExportAsFixedFormat(
                                OutputFileName: outputPath,
                                ExportFormat: (format == ExportFixedFormat.XPS ? WdExportFormat.wdExportFormatXPS : WdExportFormat.wdExportFormatPDF),
                                OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForOnScreen);

        if (!OpenResult.alreadyOpen || closeAfterDone)
          OpenResult.doc.Close(false);
      }
    }

    /// <summary>
    /// Converts all paragraphs in the specified Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to the paragraph text.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true)
    {
      if (string.IsNullOrEmpty(docPath) || !System.IO.File.Exists(docPath))
        return null;
      else
      {
        List<WordParagraph> Result = new List<WordParagraph>();
        int ParaCount;

        var OpenResult = GetOrOpenDocument(docPath, false, true);

        int i = 0;
        ParaCount = OpenResult.doc.Paragraphs.Count;
        foreach (Paragraph p in OpenResult.doc.Paragraphs)
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
          else
          {
            var ListType = R.ListFormat.ListType;

            if (ListType == WdListType.wdListSimpleNumbering || ListType == WdListType.wdListOutlineNumbering)
              PType = ParagraphType.NumberedList;
          }

          Range FirstChar, LastChar;
          FirstChar = R.Characters.First;
          LastChar = R.Characters.Last;

          //Store the start and end positions of this range. We'll use it later in the XPS to highlight QAs.
          var StartY = (float)R.Information[WdInformation.wdVerticalPositionRelativeToPage];

          //There is no direct way of getting the EndY of this paragraph (bottom of the bounding rect of this paragraph). 
          //To compute the EndY, we'll check the next paragraph
          float EndY = 0;
          var NextPara = R.Next(WdUnits.wdParagraph);
          if (NextPara == null)
          {
            //if there is no next paragraph, just add the font height of the last character to its top to get the bottom.
            EndY = (float)LastChar.Information[WdInformation.wdVerticalPositionRelativeToPage] + LastChar.Font.Size;
          }
          else
            EndY = (float)NextPara.Information[WdInformation.wdVerticalPositionRelativeToPage];

          var StartPage = (int)FirstChar.Information[WdInformation.wdActiveEndPageNumber];
          var EndPage = (int)LastChar.Information[WdInformation.wdActiveEndPageNumber];

          Result.Add(new WordParagraph(R.Text, R.Start, R.End, PType, StartY, EndY, StartPage, EndPage));

          ViewModelLocator.Main.UpdateProgress(false, "Importing...", i / (float)ParaCount);

          //call progress callback every once in a while
          if ((i++) % 10 == 0)
            progressCallback?.Invoke(i, ParaCount);

          if (token.IsCancellationRequested)
            break;
        }

        if (!OpenResult.alreadyOpen && closeAfterDone)
        {
          System.Threading.Tasks.Task.Run(() => OpenResult.doc.Close(SaveChanges: false));
        }

        progressCallback?.Invoke(ParaCount, ParaCount);

        return Result;
      }
    }

    public void OpenDocument(string docPath, bool openReadonly, int? start, int? end)
    {
      var OpenResult = GetOrOpenDocument(docPath, true, openReadonly);

      if (OpenResult.doc != null)
        OpenResult.doc.Activate();

      if (start != null)
      {
        Sel.Start = start.Value;

        if (end != null)
          Sel.End = end.Value;

        this.Application.ActiveWindow.ScrollIntoView(Sel.Range);
      }
    }

    /// <summary>
    /// Returns the first Document that matches the specified path. If no match is found, Word tries to open the document from the specified path.
    /// Boolean flag tells whether the document was already open in Word or has been opened by this function.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    private (Document doc, bool alreadyOpen) GetOrOpenDocument(string docPath, bool visible, bool openReadonly)
    {
      var Doc = this.Application.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
      {
        try
        {
          Doc = this.Application.Documents.Open(docPath, ReadOnly: openReadonly, AddToRecentFiles: false, Visible: visible);
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

    /// <summary>
    /// Returns the number of paragraphs in the active Word document.
    /// </summary>
    /// <returns></returns>
    public int? GetActiveDocumentParagraphsCount()
    {
      return this.Application.ActiveDocument?.Paragraphs?.Count;
    }

    /// <summary>
    /// Creates a single Word document by merge content of all the specified documents. This function also updates QA numbering to make it continuous.
    /// </summary>
    /// <param name="docs"></param>
    /// <returns></returns>
    public void CreateMergedDocument(string[] docs, string outputPath, bool closeAfterCreate)
    {
      var MergeDoc = this.Application.Documents.Add(Visible: false);
      Range R = MergeDoc.Range();

      ViewModelLocator.Main.UpdateProgress(true, "Merging documents into one file", 0);

      for (int i = 0; i < docs.Length; i++)
      {
        var SourceDoc = this.Application.Documents.Open(docs[i], Visible: false);
        SourceDoc.Content.Copy();
        R.PasteAndFormat(WdRecoveryType.wdPasteDefault);
        SourceDoc.Close(false);

        //Start a new pageafter each file except for the last file.
        if (i < docs.Length - 1)
        {
          R.InsertParagraphAfter();
          R = MergeDoc.Paragraphs.Last.Range;
          R.InsertBreak(WdBreakType.wdPageBreak);         
        }

        ViewModelLocator.Main.UpdateProgress(true, "Merging documents into one file", ((float)(i+1) / docs.Length) * 100);
      }

      MergeDoc.SaveAs(outputPath);

      if (closeAfterCreate)
        MergeDoc.Close(false);

      ViewModelLocator.Main.UpdateProgress(true, "Merging documents into one file", 100);
    }

    /// <summary>
    /// Assigns ordered question numbers to all QAs in the specified document. Returns a Dictionary containing all corrected question numbers (old and new value of each QA number).
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="newText"></param>
    public Dictionary<int, int> FixAllQANumbers(string docPath, List<WordParagraph> delimiterParagraphs, bool closeAfterDone)
    {
      var OpenResult = GetOrOpenDocument(docPath, true, false);

      int ExpectedIndex = 1;
      Dictionary<int ,  int> Fixes =  new Dictionary<int, int>();

      if (OpenResult.doc != null)
      {
        //Extra characters that we insert in the loop below will affect the start and end of all subsequent paragraph ranges. 
        //The following offset will keep track of the number of characters that we have inserted so far.
        int Offset = 0;

        foreach (var Para in delimiterParagraphs)
        {
          Range R = OpenResult.doc.Range(Para.Start + Offset, Para.End + Offset);

          if (R != null)
          {
            var QIndex = ViewModelLocator.QAExtractionStrategy.ParseQuestionNumber(Para.Text);

            if (QIndex == null || QIndex != ExpectedIndex)
            {
              //if question number starts with the letter Q, move one character ahead to skip letter Q
              if (OpenResult.doc.Range(Para.Start + Offset).Characters[1].Text.ToUpper() == "Q")
                Offset++;

              //Non-inline shapes present a strange behavior. Word treats those shapes as part of the pervious Range
              //even though these are physically placed after the range. So if we modify the Text property of previous Range,
              //those shapes get overwritten. Here we deal with this problem by selectively removing one character at a time.
              R = OpenResult.doc.Range(Para.Start + Offset, Para.Start + Offset);

              for (int i = 0; i < QIndex.ToString().Length; i++)
                R.Delete();

              R.Text = ExpectedIndex.ToString();
              Fixes.Add(QIndex.Value, ExpectedIndex);

              Offset += (ExpectedIndex.ToString().Length - QIndex.ToString().Length);
            }
          }

          ExpectedIndex++;

          ViewModelLocator.Main.UpdateProgress(false, null, (((float)ExpectedIndex) / delimiterParagraphs.Count) * 100);
        }
      }

      OpenResult.doc.Save();

      if (!OpenResult.alreadyOpen || closeAfterDone)
      {
        OpenResult.doc.Close(false);
      }

      return Fixes;
    }
  }
}