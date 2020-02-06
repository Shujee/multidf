using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MultiDF.VM
{
  public struct DFResultRow
  {
    public QA Q1 { get; set; }
    public QA Q2 { get; set; }
    public double Distance { get; set; }

    public DFResultRow(QA q1, QA q2, double dist)
    {
      Q1 = q1;
      Q2 = q2;
      Distance = dist;
    }

    /// <summary>
    /// This overload is used by unit tests only.
    /// </summary>
    /// <param name="parentResult"></param>
    /// <param name="i1"></param>
    /// <param name="i2"></param>
    /// <param name="dist"></param>
    public DFResultRow(int i1, int i2, double dist)
    {
      Q1 = new QA() { Doc = new XMLDoc() { SourcePath = "D1.docx" }, Index = i1 };
      Q2 = new QA() { Doc = new XMLDoc() { SourcePath = "D2.docx" }, Index = i2 };
      Distance = dist;
    }

    public override bool Equals(object obj)
    {
      if (obj == null || typeof(DFResultRow) != obj.GetType())
        return false;
      else
      {
        var Other = (DFResultRow)obj;
        return (Q1.Equals(Other.Q1) && Q2.Equals(Other.Q2)) ||
               (Q2.Equals(Other.Q1) && Q1.Equals(Other.Q2));
      }
    }

    public override int GetHashCode()
    {
      return Q1.GetHashCode() ^  Q2.GetHashCode();
    }

    public static bool operator ==(DFResultRow dr1, DFResultRow dr2)
    {
      return dr1.Equals(dr2);
    }

    public static bool operator !=(DFResultRow dr1, DFResultRow dr2)
    {
      return !dr1.Equals(dr2);
    }
  }

  /// <summary>
  /// Represents results of duplicate finding process.
  /// </summary>
  [Serializable]
  [KnownType(typeof(XMLDoc))]
  [KnownType(typeof(DFResultRow))]
  [KnownType(typeof(SynchronizedCollection<DFResultRow>))]
  public class DFResult : ObservableObject, ISerializable
  {
    public XMLDoc Doc1 { get; set; }
    public XMLDoc Doc2 { get; set; }
    public int Count1 { get; set; }
    public int Count2 { get; set; }

    private double _DiffThreshold;
    public double DiffThreshold
    {
      get => _DiffThreshold;
      set
      {
        _DiffThreshold = value;
        RaisePropertyChanged(nameof(FilteredItems));
      }
    }
    
    public SynchronizedCollection<DFResultRow> Items { get; set; } = new SynchronizedCollection<DFResultRow>();

    public IEnumerable<DFResultRow> FilteredItems => Items.Where(i => i.Distance <= DiffThreshold).OrderBy(i => i.Distance);

    /// <summary>
    /// only for serialization. do not use this constructor.
    /// </summary>
    public DFResult()
    {

    }

    protected DFResult(SerializationInfo info, StreamingContext context)
    {
      // Perform your deserialization here...
      this.Doc1 = (XMLDoc)info.GetValue("Doc1", typeof(XMLDoc));
      this.Doc2 = (XMLDoc)info.GetValue("Doc2", typeof(XMLDoc));
      this.Count1 = (int)info.GetValue("Count1", typeof(int));
      this.Count2 = (int)info.GetValue("Count2", typeof(int));
      this.DiffThreshold = (double)info.GetValue("DiffThreshold", typeof(double));

      this.Items = (SynchronizedCollection<DFResultRow>)info.GetValue("Items", typeof(SynchronizedCollection<DFResultRow>));
    }

    public DFResult(XMLDoc d1, XMLDoc d2, int c1, int c2)
    {
      Doc1 = d1;
      Doc2 = d2;
      Count1 = c1;
      Count2 = c2;
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        return;

      info.AddValue("Doc1", Doc1);
      info.AddValue("Doc2", Doc2);
      info.AddValue("Count1", Count1);
      info.AddValue("Count2", Count2);
      info.AddValue("DiffThreshold", DiffThreshold);
      
      info.AddValue("Items", Items);
    }

    public string ToHtml()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendLine("<table class=\"table table-bordered table-striped\">");

      if (this.Doc1 == this.Doc2)
      {
        var SI = this.Doc1.SyncInfo;
        sb.AppendLine($"<thead><tr><th colspan=\"2\">{this.Doc1.Name} (QAs: {this.Count1})<br />" +
                      $"Source: {SI.Size1KB} / {SI.LastModified1}<br />" +
                      $"Local: {SI.Size2KB} / {SI.LastModified2}" +
                      $"</th><th>Diff</th></tr></thead>");
      }
      else
      {
        var SI1 = this.Doc1.SyncInfo;
        var SI2 = this.Doc2.SyncInfo;
        sb.AppendLine($"<thead><tr><th>{this.Doc1.Name} (QAs: {this.Count1})<br />" +
                      $"Source: {SI1.Size1KB} / {SI1.LastModified1}<br />" +
                      $"Local: {SI1.Size2KB} / {SI1.LastModified2}" +
                      $"</th><th>{this.Doc2.Name} (QAs: {this.Count2})<br />" +
                      $"Source: {SI2.Size1KB} / {SI2.LastModified1}<br />" +
                      $"Local: {SI2.Size2KB} / {SI2.LastModified2}" +
                      $"</th><th>Diff</th></tr></thead>");
      }

      sb.AppendLine("<tbody>");
      foreach (var item in FilteredItems)
      {
        sb.AppendLine($"<tr><td>Question {item.Q1.Index}</td><td>Question {item.Q2.Index}</td><td>{item.Distance.ToString("P0")}</td></tr>");
      }
      sb.AppendLine("</tbody></table>");

      return sb.ToString();
    }
  }
}
