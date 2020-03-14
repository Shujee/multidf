using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  /// <summary>
  /// Used for testing.
  /// </summary>
  internal class DummyCameraService : ICameraService
  {
    public object BestResCamera => null;

    public event Action SnapshotCaptured;

    public bool InitCam()
    {
      return true;
    }

    public Task<Bitmap> TakeCameraSnapshot()
    {
      SnapshotCaptured?.Invoke();
      return Task.FromResult(new Bitmap(@"C:\Users\Shujaat\Pictures\loco engine.png"));
    }
  }
}