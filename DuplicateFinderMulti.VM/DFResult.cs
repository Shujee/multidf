using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
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
  }

  /// <summary>
  /// Represents results of duplicate finding process.
  /// </summary>
  public class DFResult 
  {
    public XMLDoc Doc1 { get; set; }
    public XMLDoc Doc2 { get; set; }
    public int Count1 { get; set; }
    public int Count2 { get; set; }

    public SynchronizedCollection<DFResultRow> Items { get; set; } = new SynchronizedCollection<DFResultRow>();

    /// <summary>
    /// only for serialization. do not use this constructor.
    /// </summary>
    public DFResult()
    {

    }

    public DFResult(XMLDoc d1, XMLDoc d2, int c1, int c2)
    {
      Doc1 = d1;
      Doc2 = d2;
      Count1 = c1;
      Count2 = c2;
    }
  }
}
