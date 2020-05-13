namespace MultiDF.VM
{
  /// <summary>
  /// Represents a single "Fix Numbering (Automatic)" result
  /// </summary>
  public class AutoFixResult
  {
    public string DocPath { get; set; }
    public int OldIndex { get; set; }
    public int NewIndex { get; set; }
  }
}