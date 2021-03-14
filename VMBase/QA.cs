using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace VMBase
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

    private string question;
    [JsonProperty(PropertyName = "question")]
    public string Question
    {
      get => question;
      set
      {
        question = value;
        QuestionUpper = value.ToUpperInvariant();
      }
    }

    private List<string> choices = new List<string>();

    [JsonProperty(PropertyName = "choices")]
    [JsonConverter(typeof(StringArrayJsonConverter))]
    public List<string> Choices
    {
      get => choices;
      set
      {
        choices = value;
        ChoicesUpper = value.Select(s => s.ToUpperInvariant()).ToList();
      }
    }

    [JsonProperty(PropertyName = "answer")]
    public string Answer { get; set; }

    [JsonIgnore]
    public string QuestionUpper { get; private set; }

    [JsonIgnore]
    public List<string> ChoicesUpper { get; private set; } = new List<string>();

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
      return obj != null && obj is QA qa && this.Doc == qa.Doc && this.Index == qa.Index;
    }

    public override int GetHashCode()
    {
      return (Doc.SourcePath + (Index * 397)).GetHashCode();
    }
  }
}
