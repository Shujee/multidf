using Microsoft.Office.Interop.Word;
using DuplicateFinderMulti.VM;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;

namespace DuplicateFinderMulti
{
  public partial class ThisAddIn : IWordService
  {
    private Document ActiveDoc => Globals.ThisAddIn.Application.Documents.Count > 0? Globals.ThisAddIn.Application.ActiveDocument : null;
    private Selection Sel => Globals.ThisAddIn.Application.Selection;

    public string ActiveDocumentPath => ActiveDoc?.FullName;

    public void ExportDocumentToXPS(string docPath, string xpsPath)
    {
      var OpenResult = GetOrOpenDocument(docPath, false);

      if (OpenResult.doc != null)
      {
        OpenResult.doc.ExportAsFixedFormat(
                                OutputFileName: xpsPath,
                                ExportFormat: WdExportFormat.wdExportFormatXPS,
                                OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForOnScreen);

        if (!OpenResult.alreadyOpen)
          OpenResult.doc.Close();
      }
    }

    /// <summary>
    /// Converts all paragraphs in the specified Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to the paragraph text.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback)
    {
      if (string.IsNullOrEmpty(docPath) || !System.IO.File.Exists(docPath))
        return null;
      else
      {
        List<WordParagraph> Result = new List<WordParagraph>();
        int ParaCount;

        var OpenResult = GetOrOpenDocument(docPath, false);

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
          else if (R.ListFormat.ListType == WdListType.wdListSimpleNumbering || R.ListFormat.ListType == WdListType.wdListOutlineNumbering)
            PType = ParagraphType.NumberedList;

          //Store the start and end positions of this range. We'll use it later in the XPS to highlight QAs.
          var StartY = (float)R.Characters.First.Information[WdInformation.wdVerticalPositionRelativeToPage];

          //There is no direct way of getting the EndY of this paragraph (bottom of the bounding rect of this paragraph). 
          //To compute the EndY, we'll check the next paragraph
          float EndY = 0;
          var NextPara = R.Next(WdUnits.wdParagraph);
          if (NextPara == null)
          {
            //if there is no next paragraph, just add the font height of the last character to its top to get the bottom.
            EndY = (float)R.Characters.Last.Information[WdInformation.wdVerticalPositionRelativeToPage] + R.Font.Size; 
          }
          else
            EndY = (float)NextPara.Information[WdInformation.wdVerticalPositionRelativeToPage];


          var StartPage = (int)R.Characters.First.Information[WdInformation.wdActiveEndPageNumber];
          var EndPage = (int)R.Characters.Last.Information[WdInformation.wdActiveEndPageNumber];

          Result.Add(new WordParagraph(R.Text, R.Start, R.End, PType, StartY, EndY, StartPage, EndPage));

          ViewModelLocator.Main.UpdateProgress(false, "Importing...", i / (float)ParaCount);

          //call progress callback every once in a while
          if ((i++) % 10 == 0)
            progressCallback?.Invoke(i, ParaCount);


          if (token.IsCancellationRequested)
            break;
        }

        if (!OpenResult.alreadyOpen)
        {
          System.Threading.Tasks.Task.Run(() => OpenResult.doc.Close(SaveChanges: false));
        }

        progressCallback?.Invoke(ParaCount, ParaCount);

        return Result;
      }
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      var OpenResult = GetOrOpenDocument(docPath, true);

      if (OpenResult.doc != null)
        OpenResult.doc.Activate();

      if (start != null)
      {
        this.Application.Selection.Start = start.Value;

        if (end != null)
          this.Application.Selection.End = end.Value;

        this.Application.ActiveWindow.ScrollIntoView(this.Application.Selection.Range);
      }
    }

    /// <summary>
    /// Returns the first Document that matches the specified path. If no match is found, Word tries to open the document from the specified path.
    /// Boolean flag tells whether the document was already open in Word or has been opened by this function.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    private (Document doc, bool alreadyOpen) GetOrOpenDocument(string docPath, bool visible)
    {
      var Doc = this.Application.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
      {
        try
        {
          Doc = this.Application.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: visible);
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
  }
}