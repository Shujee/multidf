using GalaSoft.MvvmLight.Ioc;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// This Class contains static references to all the view models in the
  /// application and provides an entry point for the bindings.
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class ViewModelLocator
  {
    static ViewModelLocator()
    {
      if (Properties.Settings.Default.UpgradeRequired)
      {
        Properties.Settings.Default.Upgrade();
        Properties.Settings.Default.UpgradeRequired = false;
        Properties.Settings.Default.Save();
      }

      GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();

      SimpleIoc.Default.Unregister<RegisterVM>();
      //SimpleIoc.Default.Unregister<DiffVM>();
      SimpleIoc.Default.Unregister<AboutVM>();
      SimpleIoc.Default.Unregister<MainVM>();
      //SimpleIoc.Default.Unregister<IDupFinder>();
      SimpleIoc.Default.Unregister<IQAExtractionStrategy>();
      SimpleIoc.Default.Unregister<IQAComparer>();
      SimpleIoc.Default.Unregister<IDocComparer>();

      SimpleIoc.Default.Register<RegisterVM>();
      //SimpleIoc.Default.Register<DiffVM>();

      SimpleIoc.Default.Register<MainVM>();
      SimpleIoc.Default.Register<IQAExtractionStrategy, DefaultQAExtractionStrategy>();
      SimpleIoc.Default.Register<IQAComparer, DefaultQAComparer>();
      SimpleIoc.Default.Register<IDocComparer, DefaultDocComparer>();

      SimpleIoc.Default.Register<AboutVM>();
      //SimpleIoc.Default.Register<IDupFinder, DupFinder>();
    }

    /// <summary>
    /// Detatches all "tick clients" from Timer's Tick event to  avoid memory leaks.
    /// </summary>
    public static void Cleanup()
    {
    }

    public static IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();
    public static IWordService WordService => SimpleIoc.Default.GetInstance<IWordService>();
    //public static IDupFinder DupFinderService => SimpleIoc.Default.GetInstance<IDupFinder>();
    //public static DiffVM Diff => SimpleIoc.Default.GetInstanceWithoutCaching<DiffVM>();

    public static MainVM Main => SimpleIoc.Default.GetInstance<MainVM>();

    public static RegisterVM Register => SimpleIoc.Default.GetInstance<RegisterVM>();
    public static AboutVM About => SimpleIoc.Default.GetInstance<AboutVM>();

    public static IQAExtractionStrategy QAExtractionStrategy => SimpleIoc.Default.GetInstance<IQAExtractionStrategy>();
    public static IDocComparer DocComparer => SimpleIoc.Default.GetInstance<IDocComparer>();
    public static IQAComparer QAComparer => SimpleIoc.Default.GetInstance<IQAComparer>();
  }
}