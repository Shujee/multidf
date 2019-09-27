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
    public string Doc { get; set; }

    public override bool Equals(object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return this.Doc == ((QA)obj).Doc && this.Index == ((QA)obj).Index;    
    }

    public override int GetHashCode()
    {     
      return (this.Doc + this.Index.ToString()).GetHashCode();
    }
  }
}
