using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  /// <summary>
  /// Used for testing.
  /// </summary>
  internal class DummyCameraService : ICameraService
  {
    public bool InitCam()
    {
      return true;
    }

    public void StopCam()
    {
    }

    public Task<Bitmap> TakeCameraSnapshot()
    {
      return Task.FromResult(new Bitmap(@"C:\Users\Shujaat\Pictures\loco engine.png"));
    }
  }
}