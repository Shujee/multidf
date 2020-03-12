using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  public interface ICameraService
  {
    bool InitCam();
    Task<Bitmap> TakeCameraSnapshot();
  }
}