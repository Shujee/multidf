using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a single Question-Answer block.
  /// </summary>
  public class QA
  {
    public int Index { get; set; }
    public string Delimiter { get; set; }
    public string Question { get; set; }
    public List<string> Choices { get; set; } = new List<string>();
    public string Answer { get; set; }

    public int Start { get; set; }
    public int End { get; set; }
    public XMLDoc Doc { get; set; }

    /// <summary>
    /// Vertical offset of the first character of this QA relative to page
    /// </summary>
    public float StartY { get; set; }

    /// <summary>
    /// Vertical offset of the last character of this QA relative to page
    /// </summary>
    public float EndY { get; set; }

    /// <summary>
    /// Page Number at which this QA starts
    /// </summary>
    public int StartPage { get; set; }

    /// <summary>
    /// Page Number at which this QA ends
    /// </summary>
    public int EndPage { get; set; }

    public override bool Equals(object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return this.Doc == ((QA)obj).Doc && this.Index == ((QA)obj).Index;    
    }

    public override int GetHashCode()
    {
      return (Doc.SourcePath + (Index * 397)).GetHashCode();
    }
  }
}
