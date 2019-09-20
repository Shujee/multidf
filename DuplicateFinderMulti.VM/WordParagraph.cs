namespace DuplicateFinderMulti.VM
{
  public enum ParagraphType
  {
    Text,
    NumberedList,
    BulletedList,
    TableHeader,
    TableRow
  }

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
    /// Whether this paragraph uses list style (bullet or numbering) in Word, or is part of a table. True for the paragraphs of "Choices" section.
    /// </summary>
    public ParagraphType Type { get; set; }

    public int Distance { get; set; }

    public WordParagraph(string text, int start, int end, ParagraphType type)
    {
      Text = text;
      Start = start;
      End = end;
      Type = type;
    }
  }
}
