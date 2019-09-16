namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a Word paragraph.
  /// </summary>
  public class WordParagraph
  {
    /// <summary>
    /// Text content of the paragraph
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Start of the paragraph's text range
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// End of the paragraph's text range
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Levenshtein distance of this paragraph from previous one.
    /// </summary>
    public int Distance { get; set; }
  }
}
