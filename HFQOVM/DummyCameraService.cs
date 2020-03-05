using System.Drawing;

namespace HFQOVM
{
  /// <summary>
  /// Used for testing.
  /// </summary>
  internal class DummyCameraService : ICameraService
  {
    public Bitmap TakeCameraSnapshot()
    {
      return new Bitmap(@"C:\Users\Shujaat\Pictures\loco engine.png");
    }
  }
}