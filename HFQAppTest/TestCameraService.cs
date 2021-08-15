using HFQOVM;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HFQAppTest
{
  /// <summary>
  /// Used for testing.
  /// </summary>
  internal class TestCameraService : ICameraService
  {
    public object BestResCamera => null;

    public bool IsCameraShutdown { get; set; }

    public event Action SnapshotCaptured;

    public bool InitCam()
    {
      return true;
    }

    public Task<Bitmap> TakeCameraSnapshot()
    {
      if (IsCameraShutdown)
        return null;
      else
      {
        SnapshotCaptured?.Invoke();
        return Task.FromResult(new Bitmap(@"C:\Users\Shujaat\Pictures\loco engine.png"));
      }
    }
  }
}