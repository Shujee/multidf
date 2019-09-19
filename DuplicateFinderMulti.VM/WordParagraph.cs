namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a text paragraph.
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
    /// Whether this paragraph uses list style (bullet or numbering) in Word. True for the paragraphs of "Choices" section.
    /// </summary>
    public bool IsSimpleNumberingListStyle { get; set; }

    public int Distance { get; set; }

    public WordParagraph(string text, int start, int end, bool listStyle)
    {
      Text = text;
      Start = start;
      End = end;
      IsSimpleNumberingListStyle = listStyle;
    }
  }
}
