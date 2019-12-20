using System;
using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  public interface IWordService
  {
    /// <summary>
    /// Exports specified Word document to XPS format and returns the path of generated xps file.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    void ExportDocumentToXPS(string docPath, string xpsPath);


    /// <summary>
    /// Converts all paragraphs in the specified Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to the paragraph text.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback);

    /// <summary>
    /// Opens a document in Microsoft Word. If the document is already open, the document window is activated. Optionally takes a 
    /// start/end range values to scroll the document to that position.
    /// </summary>
    /// <param name="docPath"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void OpenDocument(string docPath, int? start, int? end);
    
    string ActiveDocumentPath { get; }
  }
}