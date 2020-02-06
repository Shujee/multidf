using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultiDF.VM
{
  /// <summary>
  /// Represents a single Question-Answer block.
  /// </summary>
  public class QA
  {  
    [JsonProperty(PropertyName = "index")]
    public int Index { get; set; }

    [JsonIgnore]
    public string Delimiter { get; set; }

    [JsonProperty(PropertyName = "question")]
    public string Question { get; set; }

    [JsonProperty(PropertyName = "choices")]
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public List<string> Choices { get; set; } = new List<string>();

    [JsonProperty(PropertyName = "answer")]
    public string Answer { get; set; }

    [JsonIgnore]
    public int Start { get; set; }
    
    [JsonIgnore]
    public int End { get; set; }

    [JsonIgnore]
    public XMLDoc Doc { get; set; }

    /// <summary>
    /// Vertical offset of the first character of this QA relative to page
    /// </summary>
    [JsonIgnore]
    public float StartY { get; set; }

    /// <summary>
    /// Vertical offset of the last character of this QA relative to page
    /// </summary>
    [JsonIgnore]
    public float EndY { get; set; }

    /// <summary>
    /// Page Number at which this QA starts
    /// </summary>
    [JsonIgnore]
    public int StartPage { get; set; }

    /// <summary>
    /// Page Number at which this QA ends
    /// </summary>
    [JsonIgnore]
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
