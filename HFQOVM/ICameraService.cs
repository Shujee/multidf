using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  public interface ICameraService
  {
    object BestResCamera { get; }
    Task<Bitmap> TakeCameraSnapshot();
  }
}