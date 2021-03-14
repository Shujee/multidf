using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using VMBase;

namespace MultiDF.VM
{
  public class DefaultDocComparer : IDocComparer
  {
    private const float MAX_DIFF_THRESHOLD = 0.5f;

    public event Action<XMLDoc, XMLDoc> DocCompareStarted;
    public event QAComparedDelegate QACompared;
    public event Action QASkipped;
    public event Action<XMLDoc, XMLDoc> DocCompareCompleted;

    //This list will keep record of pairs of question that have been dispatched for comparison, so that
    //we do not send them for comparison again. e.g. if X and Y have been dispatched for comparison, we will
    //not dispatch the inverse pair Y and X.
    //HashSet<(QA, QA)> DispatchedItems;

    public DFResult Compare(XMLDoc d1, XMLDoc d2, IQAComparer qaComparer, bool ignoreCase, CancellationToken token)
    {
      DFResult Result = new DFResult(d1, d2, d1.QAs.Count, d2.QAs.Count);

      int LoopCount = 0;
      float TotalComparisons = d1.QAs.Count * d2.QAs.Count;

      DocCompareStarted?.Invoke(d1, d2);

      //DispatchedItems = new HashSet<(QA, QA)>();

      try
      {
        var AllComparisons = Parallel.ForEach(d1.QAs, new ParallelOptions() { CancellationToken = token, MaxDegreeOfParallelism = 3 },
          (q1) =>
          {

            foreach (var q2 in d2.QAs)
            {
              Interlocked.Increment(ref LoopCount);
              var Prog = 100 * (LoopCount / TotalComparisons);

              if (!q1.Equals(q2))
              {
                //var DFR = ProcessDFR(q1, q2, qaComparer, ignoreCase);

                var DFR = new DFResultRow(q1, q2, 0);
                DFR.Distance = qaComparer.Distance(q1, q2, ignoreCase);


                if (DFR != null && DFR.Distance < MAX_DIFF_THRESHOLD)
                {
                  Result.Items.Add(DFR);
                  QACompared?.Invoke(this, new QAComparedArgs() { QA1 = q1, QA2 = q2, Distance = DFR.Distance, PercentProgress = Prog });
                }
                else
                  QASkipped?.Invoke();

                token.ThrowIfCancellationRequested();
              }
            }

            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
          });
      }
      finally
      {
        DocCompareCompleted?.Invoke(d1, d2);
      }

      return Result;
    }

    private DFResultRow? ProcessDFR(QA q1, QA q2, IQAComparer qaComparer, bool ignoreCase)
    {
      //if (!DispatchedItems.Contains((q1, q2)))
      //{
      //  lock (DispatchedItems)
      //  {
      //    DispatchedItems.Add((q1, q2));
      //  }

      var DFR = new DFResultRow(q1, q2, 0);
      DFR.Distance = qaComparer.Distance(q1, q2, ignoreCase);
      return DFR;
      //}
      //else
      //{
      //  System.Diagnostics.Debug.Print("Already dispatched");
      //  return null;
      //}
    }
  }
}