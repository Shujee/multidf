using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Common;

namespace HFQOVM
{
  public class HFQResultRowVM : ObservableObject
  {
    public HFQResultRowVM()
    {

    }

    public HFQResultRowVM(HFQResultRow resultRow)
    {
      _Q = resultRow.q;
      _A1 = resultRow.a1;
      _A2 = resultRow.a2;
      _A3 = resultRow.a3;
    }

    private int _Q;
    public int Q
    {
      get => _Q;
      set => Set(ref _Q, value);
    }

    private int? _A1;
    public int? A1
    {
      get => _A1;
      set
      {
        Set(ref _A1, value);
      }
    }

    private int? _A2;
    public int? A2
    {
      get => _A2;
      set
      {
        Set(ref _A2, value);
      }
    }

    private int? _A3;
    public int? A3
    {
      get => _A3;
      set
      {
        Set(ref _A3, value);
      }
    }

    /// <summary>
    /// Returns a new HFQResultRow object (the model object) that can then be passed to the data service.
    /// </summary>
    /// <returns></returns>
    public HFQResultRow ToHFQResultRow()
    {
      return new HFQResultRow()
      {
        q = _Q,
        a1 = _A1,
        a2 = _A2,
        a3 = _A3
      };
    }
  }
}