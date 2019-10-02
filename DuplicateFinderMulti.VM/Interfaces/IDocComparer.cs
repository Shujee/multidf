using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class QAComparedArgs
  {
    public QA QA1 { get; set; }
    public QA QA2 { get; set; }

    public double Distance { get; set; }
    public double PercentProgress { get; set; }
  }

  public interface IDocComparer
  {
    Task<DFResult> Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token);
    event System.Action<QAComparedArgs> QACompared;
  }
}