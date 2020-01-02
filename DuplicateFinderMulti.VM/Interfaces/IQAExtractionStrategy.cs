using System.Collections.Generic;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a class that can take a list of WordParagraphs and identify and return individual question-answer blocks from it.
  /// </summary>
  public interface IQAExtractionStrategy
  {
    List<QA> ExtractQAs(List<WordParagraph> paragraphs, CancellationToken tok);
    List<WordParagraph> ExtractDelimiterParagraphs(List<WordParagraph> paragraphs, CancellationToken tok);
    int? ParseQuestionNumber(string paragraphText);
  }
}