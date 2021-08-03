using GalaSoft.MvvmLight.Ioc;

namespace HFQOVM
{
  /// <summary>
  /// This Class contains static references to all the view models in the
  /// application and provides an entry point for the bindings.
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class ViewModelLocator : VMBase.ViewModelLocatorBase
  {
    static ViewModelLocator()
    {
      if (!GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
      {
        var LogDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "HFQApp");
        if (!System.IO.Directory.Exists(LogDir))
          System.IO.Directory.CreateDirectory(LogDir);

        InitLogger("${specialfolder:folder=ApplicationData}/HFQApp/");
      }

      App = "HFQApp";
      AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

      SimpleIoc.Default.Unregister<HFQVM>();
      SimpleIoc.Default.Unregister<IHardwareHelper>();
      SimpleIoc.Default.Unregister<ICameraService>();
      
      SimpleIoc.Default.Register<HFQVM>();
      SimpleIoc.Default.Register<IHardwareHelper, HardwareHelper>();

#if(DEBUG)
      SimpleIoc.Default.Register<ICameraService, DummyCameraService>();
      //SimpleIoc.Default.Register<ICameraService, CameraService>();      
#else
      SimpleIoc.Default.Register<ICameraService, CameraService>();      
#endif
    }

    public static HFQVM HFQ => SimpleIoc.Default.GetInstance<HFQVM>();
    public static IDialogService DialogServiceHFQ => SimpleIoc.Default.GetInstance<IDialogService>();
    public static IHardwareHelper HardwareHelper => SimpleIoc.Default.GetInstance<IHardwareHelper>();
    public static ICameraService CameraService => SimpleIoc.Default.GetInstance<ICameraService>();
    public static IApplicationService ApplicationService => SimpleIoc.Default.GetInstance<IApplicationService>();
  }
}