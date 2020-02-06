using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MultiDF.VM
{
  public class DefaultDocComparer : IDocComparer
  {
    public event Action<XMLDoc, XMLDoc> DocCompareStarted;
    public event QAComparedDelegate QACompared;
    public event Action QASkipped;
    public event Action<XMLDoc, XMLDoc> DocCompareCompleted;

    //This list will keep record of pairs of question that have been dispatched for comparison, so that
    //we do not send them for comparison again. e.g. if X and Y have been dispatched for comparison, we will
    //not dispatch the inverse pair Y and X.
    List<DFResultRow> DispatchedItems = new List<DFResultRow>();

    public Task<DFResult> Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token)
    {
      DFResult Result = new DFResult(d1, d2, d1.QAs.Count, d2.QAs.Count);

      int LoopCount = 0;
      float TotalComparisons = d1.QAs.Count * d2.QAs.Count;

      DocCompareStarted?.Invoke(d1, d2);

      DispatchedItems.Clear();

      return Task.Run(() =>
      {
        Parallel.ForEach(d1.QAs, new ParallelOptions() { CancellationToken = token },
                q1 =>
                {
                  foreach (var q2 in d2.QAs)
                  {
                    var DFR = ProcessDFR(q1, q2, qaComparer, ignoreCase, token);
                    Interlocked.Increment(ref LoopCount);
                    var Prog = 100 * (LoopCount / TotalComparisons);

                    if (DFR != null)
                    {
                      lock(this){
                        Result.Items.Add(DFR.Value);
                        QACompared?.Invoke(this, new QAComparedArgs() { QA1 = q1, QA2 = q2, Distance = DFR.Value.Distance, PercentProgress = Prog });
                      }
                    }
                    else
                      QASkipped?.Invoke();
                   
                    token.ThrowIfCancellationRequested();
                  }
                }
              );
      }).ContinueWith(t =>
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


    private DFResultRow? ProcessDFR(QA q1, QA q2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token)
    {
      if (q1 != q2)
      {
        var DFR = new DFResultRow(q1, q2, 0);

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
          if (!token.IsCancellationRequested)
          {
            DFR.Distance = qaComparer.Distance(q1, q2, ignoreCase);

            if (!token.IsCancellationRequested)
              return DFR;
          }
        }
      }

      return null;
    }
  }
}