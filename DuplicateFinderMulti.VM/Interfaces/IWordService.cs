﻿using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  public interface IWordService
  {
    //string GetActiveDocumentText();
    //IEnumerable<WordParagraph> GetActiveDocumentParagraphs();
    //int? GetActiveDocumentParagraphsCount();
    //void SelectRange(int start, int end);
    //string GetRangeText(int start, int end);

    List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token);
    void OpenDocument(string docPath);
  }
}