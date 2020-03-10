using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  public interface ICameraService
  {
    bool InitCam();
    void StopCam();
    Task<Bitmap> TakeCameraSnapshot();
  }
}