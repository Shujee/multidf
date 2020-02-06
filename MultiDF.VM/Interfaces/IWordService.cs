using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiDF.VM
{
  public enum ExportFixedFormat
  {
    PDF,
    XPS
  }

  public interface IWordService
  {
    /// <summary>
    /// Exports specified Word document to XPS format and returns the path of generated xps file.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    void ExportDocumentToFixedFormat(ExportFixedFormat format, string docPath, string outputPath, bool closeAfterDone);

    /// <summary>
    /// Converts all paragraphs in the specified Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to the paragraph text.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true);

    /// <summary>
    /// Opens a document in Microsoft Word. If the document is already open, the document window is activated. Optionally takes a 
    /// start/end range values to scroll the document to that position.
    /// </summary>
    /// <param name="docPath"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void OpenDocument(string docPath, int? start, int? end);

    /// <summary>
    /// Assigns ordered question numbers to all QAs in the specified document.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="newText"></param>
    void FixQANumbers(string docPath, List<WordParagraph> delimiterParagraphs, bool closeAfterDone);

    /// <summary>
    /// Creates a single Word document by merge content of all the specified documents.
    /// </summary>
    /// <param name="docs"></param>
    /// <returns></returns>
    void CreateMergedDocument(string[] docs, string outputPath, bool closeAfterCreate);
    
    string ActiveDocumentPath { get; }
  }
}