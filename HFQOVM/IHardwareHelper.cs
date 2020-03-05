namespace HFQOVM
{
  public interface IHardwareHelper
  {
    bool IsWindows10();
    bool HasMultipleScreens();
    bool IsLaptop();
    bool IsVM();
  }
}