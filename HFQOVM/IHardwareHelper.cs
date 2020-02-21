using System.Drawing;

namespace HFQOVM
{
  public interface IHardwareHelper
  {
    bool HasMultipleScreens();
    bool IsLaptop();
    bool IsVM();
    Bitmap TakeCameraSnapshot();
  }
}