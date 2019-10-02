using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DuplicateFinderMulti.VM
{
  public class DefaultDocComparer : IDocComparer
  {
    public event Action<QAComparedArgs> QACompared;

    public Task<DFResult> Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token)
    {
      List<Task> Tasks = new List<Task>();
      DFResult Result = new DFResult(d1, d2, d1.QAs.Count, d2.QAs.Count);

      float TotalComparisons = d1.QAs.Count * d2.QAs.Count;

      foreach (var q1 in d1.QAs)
      {
        foreach (var q2 in d2.QAs)
        {
          Tasks.Add(Task.Run(() =>
          {
            if (!token.IsCancellationRequested)
            {
              //if (!Result.ContainsKey(new DFResultRow(q1, q2)))
              //{
              var Dist = qaComparer.Distance(q1, q2, ignoreCase);

              if (token.IsCancellationRequested)
                return;

              Result.Items.Add(new DFResultRow(q1, q2, Dist));

              QACompared?.Invoke(new QAComparedArgs() { QA1 = q1, QA2 = q2, Distance = Dist, PercentProgress = 100 * (Result.Items.Count / TotalComparisons) });
              //}
              //else
              //  System.Diagnostics.Debug.WriteLine($"========================================================={q1.Doc} ({q1.Index}) - {q2.Doc} ({q2.Index})=========================================================");
            }
          }, token));

          token.ThrowIfCancellationRequested();
        }
      }

      return Task.WhenAll(Tasks).ContinueWith(t =>
      {
        if (t.Exception == null)
          return Result;
        else
          throw t.Exception;
      });
    }
  }
}