using System;
using System.Threading;
using System.Threading.Tasks;
using VMBase;

namespace MultiDF.VM
{
  public class QAComparedArgs : EventArgs
  {
    public QA QA1 { get; set; }
    public QA QA2 { get; set; }

    public double Distance { get; set; }
    public double PercentProgress { get; set; }
  }
    
  public delegate void QAComparedDelegate(object sender, QAComparedArgs e);
  
  public interface IDocComparer
  {
    DFResult Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token);

    event Action<XMLDoc, XMLDoc> DocCompareStarted;
    event QAComparedDelegate QACompared;
    event Action QASkipped;
    event Action<XMLDoc, XMLDoc> DocCompareCompleted;
  }
}