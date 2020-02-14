using GalaSoft.MvvmLight.Ioc;
using Model;
using Common;

namespace MultiDF.VM
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

      SimpleIoc.Default.Unregister<RegisterVM>();
      SimpleIoc.Default.Unregister<DiffVM>();
      SimpleIoc.Default.Unregister<AboutVM>();
      SimpleIoc.Default.Unregister<UploadExamVM>();
      SimpleIoc.Default.Unregister<MainVM>();
      SimpleIoc.Default.Unregister<IQAExtractionStrategy>();
      SimpleIoc.Default.Unregister<IQAComparer>();
      SimpleIoc.Default.Unregister<IDocComparer>();
      
      SimpleIoc.Default.Register<RegisterVM>();
      SimpleIoc.Default.Register<DiffVM>();
      SimpleIoc.Default.Register<AboutVM>();
      SimpleIoc.Default.Register<UploadExamVM>();
      SimpleIoc.Default.Register<MainVM>();
      SimpleIoc.Default.Register<IQAExtractionStrategy, DefaultQAExtractionStrategy>();
      SimpleIoc.Default.Register<IQAComparer, DefaultQAComparer>();
      SimpleIoc.Default.Register<IDocComparer, DefaultDocComparer>();
    }

    public static IDialogService DialogServiceMultiDF => SimpleIoc.Default.GetInstance<IDialogService>();
    public static IWordService WordService => SimpleIoc.Default.GetInstance<IWordService>();
    public static DiffVM Diff => SimpleIoc.Default.GetInstanceWithoutCaching<DiffVM>();

    public static MainVM Main => SimpleIoc.Default.GetInstance<MainVM>();

    public static RegisterVM Register => SimpleIoc.Default.GetInstance<RegisterVM>();
    public static UploadExamVM UploadExam => SimpleIoc.Default.GetInstanceWithoutCaching<UploadExamVM>();
    public static AboutVM About => SimpleIoc.Default.GetInstance<AboutVM>();

    public static IQAExtractionStrategy QAExtractionStrategy => SimpleIoc.Default.GetInstance<IQAExtractionStrategy>();
    public static IDocComparer DocComparer => SimpleIoc.Default.GetInstance<IDocComparer>();
    public static IQAComparer QAComparer => SimpleIoc.Default.GetInstance<IQAComparer>();
  }
}