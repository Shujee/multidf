using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents results of duplicate finding process.
  /// </summary>
  class DFResult : System.Collections.Concurrent.ConcurrentDictionary<string, double>
  {
    public void AddResultRow(QA q1, QA q2, double distance)
    {
      this.TryAdd(q1.Doc + q1.Index + q2.Doc + q2.Index, distance);
      
    }

    public void RemoveResultRow(QA q1, QA q2)
    {
      _ = this.TryRemove(q1.Doc + q1.Index + q2.Doc + q2.Index, out double _);
    }
  }
}
