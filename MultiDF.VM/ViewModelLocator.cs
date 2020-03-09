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
      App = "MultiDF";
      AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

      SimpleIoc.Default.Unregister<DiffVM>();
      SimpleIoc.Default.Unregister<UploadExamVM>();
      SimpleIoc.Default.Unregister<MainVM>();
      SimpleIoc.Default.Unregister<IQAExtractionStrategy>();
      SimpleIoc.Default.Unregister<IQAComparer>();
      SimpleIoc.Default.Unregister<IDocComparer>();
      
      SimpleIoc.Default.Register<DiffVM>();
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

    public static UploadExamVM UploadExam => SimpleIoc.Default.GetInstanceWithoutCaching<UploadExamVM>();

    public static IQAExtractionStrategy QAExtractionStrategy => SimpleIoc.Default.GetInstance<IQAExtractionStrategy>();
    public static IDocComparer DocComparer => SimpleIoc.Default.GetInstance<IDocComparer>();
    public static IQAComparer QAComparer => SimpleIoc.Default.GetInstance<IQAComparer>();
  }
}