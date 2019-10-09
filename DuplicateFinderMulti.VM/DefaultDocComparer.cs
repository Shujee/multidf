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
    public event Action<XMLDoc, XMLDoc> DocCompareStarted;
    public event QAComparedDelegate QACompared;
    public event Action<XMLDoc, XMLDoc> DocCompareCompleted;

    public Task<DFResult> Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token)
    {
      List<Task> Tasks = new List<Task>();
      DFResult Result = new DFResult(d1, d2, d1.QAs.Count, d2.QAs.Count);

      float TotalComparisons = d1.QAs.Count * d2.QAs.Count;

      //This list will keep record of pairs of question that have been dispatched for comparison, so that
      //we do not send them for comparison again. e.g. if X and Y have been dispatched for comparison, we will
      //not dispatch the inverse pair Y and X.
      List<DFResultRow> DispatchedItems = new List<DFResultRow>();

      int LoopCount = 0;

      DocCompareStarted?.Invoke(d1, d2);

      foreach (var q1 in d1.QAs)
      {
        foreach (var q2 in d2.QAs)
        {
          var DFR = new DFResultRow(q1, q2, 0);

          if (q1 != q2)
          {
            var Flag = false;

            lock (DispatchedItems)
            {
              if (!DispatchedItems.Any(i => i.Equals(DFR)))
              {
                DispatchedItems.Add(DFR);
                Flag = true;
              }
            }

            if (Flag)
            {
              Tasks.Add(Task.Run(() =>
              {
                if (!token.IsCancellationRequested)
                {
                  DFR.Distance = qaComparer.Distance(q1, q2, ignoreCase);

                  if (token.IsCancellationRequested)
                    return;

                  Result.Items.Add(DFR);

                  QACompared?.Invoke(this, new QAComparedArgs() { QA1 = q1, QA2 = q2, Distance = DFR.Distance, PercentProgress = 100 * (LoopCount / TotalComparisons) });
                }
              }, token));
            }
          }

          token.ThrowIfCancellationRequested();

          Interlocked.Increment(ref LoopCount);
        }
      }

      return Task.WhenAll(Tasks).ContinueWith(t =>
      {
        if (t.Exception == null)
        {
          DocCompareCompleted?.Invoke(d1, d2);
          return Result;
        }
        else
          throw t.Exception;
      });
    }
  }
}