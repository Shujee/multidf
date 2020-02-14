using GalaSoft.MvvmLight;
using System;

namespace VMBase
{
  /// <summary>
  /// Provides status message and progress properties. Used by MultiDF and HFQ as base VM.
  /// </summary>
  public partial class MainBase : ViewModelBase
  {
    #region "Status"
    public string ProgressMessage { get; private set; }
    public double ProgressValue { get; private set; }

    protected DateTime _ProgressStartTime = DateTime.Now;
    public TimeSpan ElapsedTime => DateTime.Now.Subtract(_ProgressStartTime);
    public TimeSpan EstimatedRemainingTime => ProgressValue == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds((ElapsedTime.TotalSeconds / ProgressValue) * (1 - ProgressValue));

    /// <summary>
    /// Updates main progress bar message and value. Runs code on UI thread because UI elements is bound to these properties.
    /// </summary>
    /// <param name="markStartTime"></param>
    /// <param name="msg"></param>
    /// <param name="value"></param>
    public void UpdateProgress(bool markStartTime, string msg, double value)
    {
      GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
      {
        if (markStartTime)
          _ProgressStartTime = DateTime.Now;

        if (msg != null)
          ProgressMessage = msg;

        ProgressValue = value;
      });
    }
    #endregion
  }
}