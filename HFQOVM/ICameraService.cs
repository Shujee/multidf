using System.Drawing;

namespace HFQOVM
{
  public interface ICameraService
  {
    bool InitCam();
    Bitmap TakeCameraSnapshot();
  }
}