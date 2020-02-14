using GalaSoft.MvvmLight.Ioc;
using Model;
using Common;

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
      SimpleIoc.Default.Unregister<IDataService>();
      if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
      {
        SimpleIoc.Default.Register<IDataService, DesignDataService>();
      }
      else
      {
#if (DEBUG)
        SimpleIoc.Default.Register<IDataService, HFQODataService>();
#else
        SimpleIoc.Default.Register<IDataService, HFQODataService>();
#endif
      }

      SimpleIoc.Default.Unregister<HFQVM>();
      
      SimpleIoc.Default.Register<HFQVM>();
    }

    public static HFQVM HFQ => SimpleIoc.Default.GetInstance<HFQVM>();
    public static IDialogService DialogServiceHFQ => SimpleIoc.Default.GetInstance<IDialogService>();
  }
}