﻿using GalaSoft.MvvmLight.Ioc;

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
      App = "HFQApp";
      AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
     
      SimpleIoc.Default.Register<HFQVM>();
      SimpleIoc.Default.Register<IHardwareHelper, HardwareHelper>();

#if (DEBUG)
      SimpleIoc.Default.Register<ICameraService, DummyCameraService>();
#else
      SimpleIoc.Default.Register<ICameraService, CameraService>();
#endif
    }

    public static HFQVM HFQ => SimpleIoc.Default.GetInstance<HFQVM>();
    public static HFQOVM.IDialogService DialogServiceHFQ => SimpleIoc.Default.GetInstance<HFQOVM.IDialogService>();
    public static IHardwareHelper HardwareHelper => SimpleIoc.Default.GetInstance<IHardwareHelper>();
    public static ICameraService CameraService => SimpleIoc.Default.GetInstance<ICameraService>();
    public static IApplicationService ApplicationService => SimpleIoc.Default.GetInstance<IApplicationService>();
  }
}