using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a class that can take a list of WordParagraphs and identify and return individual question-answer blocks from it.
  /// </summary>
  public interface IQAExtractionStrategy
  {
    List<QA> Extract(List<WordParagraph> paragraphs, CancellationToken tok);
  }
}