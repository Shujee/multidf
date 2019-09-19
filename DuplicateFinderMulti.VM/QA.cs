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
  }
}
