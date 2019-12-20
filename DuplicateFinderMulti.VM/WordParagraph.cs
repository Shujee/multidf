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
    /// Vertical offset of the first character of this range relative to page
    /// </summary>
    public float StartY { get; set; }

    /// <summary>
    /// Vertical offset of the last character of this range relative to page
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Page Number at which this paragraph starts
    /// </summary>
    public int StartPage { get; set; }

    /// <summary>
    /// Page Number at which this paragraph ends
    /// </summary>
    public int EndPage { get; set; }

    /// <summary>
    /// Whether this paragraph uses list style (bullet or numbering) in Word, or is part of a table. True for the paragraphs of "Choices" section.
    /// </summary>
    public ParagraphType Type { get; set; }

    public int Distance { get; set; }

    public WordParagraph(string text, int start, int end, ParagraphType type, float startY, float endY, int startPage, int endPage)
    {
      Text = text;
      Start = start;
      End = end;
      Type = type;
      StartY = startY;
      EndY = endY;
      StartPage = startPage;
      EndPage = endPage;
    }
  }
}