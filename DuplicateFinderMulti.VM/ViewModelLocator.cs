using GalaSoft.MvvmLight.Ioc;
using HFQOModel;
using NLog;
using NLog.Config;
using NLog.Targets;

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

      if (!GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
      {
        InitLogger();
      }

      GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();

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
      SimpleIoc.Default.Unregister<AuthVM>();
      SimpleIoc.Default.Unregister<MainVM>();
      SimpleIoc.Default.Unregister<HFQVM>();
      SimpleIoc.Default.Unregister<IQAExtractionStrategy>();
      SimpleIoc.Default.Unregister<IQAComparer>();
      SimpleIoc.Default.Unregister<IDocComparer>();
      
      

      SimpleIoc.Default.Register<RegisterVM>();
      SimpleIoc.Default.Register<DiffVM>();
      SimpleIoc.Default.Unregister<AuthVM>();
      SimpleIoc.Default.Register<MainVM>();
      SimpleIoc.Default.Unregister<HFQVM>();
      SimpleIoc.Default.Register<IQAExtractionStrategy, DefaultQAExtractionStrategy>();
      SimpleIoc.Default.Register<IQAComparer, DefaultQAComparer>();
      SimpleIoc.Default.Register<IDocComparer, DefaultDocComparer>();

      SimpleIoc.Default.Register<AboutVM>();
    }

    /// <summary>
    /// Detatches all "tick clients" from Timer's Tick event to  avoid memory leaks.
    /// </summary>
    public static void Cleanup()
    {
    }

    public static IDataService DataService => SimpleIoc.Default.GetInstance<IDataService>();
    public static IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();
    public static IWordService WordService => SimpleIoc.Default.GetInstance<IWordService>();
    public static DiffVM Diff => SimpleIoc.Default.GetInstanceWithoutCaching<DiffVM>();


    public static AuthVM Auth => SimpleIoc.Default.GetInstance<AuthVM>();
    public static MainVM Main => SimpleIoc.Default.GetInstance<MainVM>();
    public static HFQVM HFQ=> SimpleIoc.Default.GetInstance<HFQVM>();

    public static RegisterVM Register => SimpleIoc.Default.GetInstance<RegisterVM>();
    public static AboutVM About => SimpleIoc.Default.GetInstance<AboutVM>();

    public static IQAExtractionStrategy QAExtractionStrategy => SimpleIoc.Default.GetInstance<IQAExtractionStrategy>();
    public static IDocComparer DocComparer => SimpleIoc.Default.GetInstance<IDocComparer>();
    public static IQAComparer QAComparer => SimpleIoc.Default.GetInstance<IQAComparer>();

    /// <summary>
    /// This is the global logger object that can be used to write debugging information to addin's log file. The log file is named "ghwordaddin.log" and is
    /// located in add-in's installation folder. For ClickOnce installation, this folder is in %appdata%.
    /// </summary>
    public static Logger Logger => LogManager.GetLogger("DFMultiLogger");

    private static void InitLogger()
    {
      // Step 1. Create configuration object 
      var config = new LoggingConfiguration();

      // Step 2. Create target log file
      var fileTarget = new FileTarget("DFMultiLogger")
      {
        FileName = "${basedir}hfqo_app.log",
        Layout = "${longdate} ${level} ${message}  ${exception}"
      };
      config.AddTarget(fileTarget);


      // Step 3. Define rules
      config.AddRuleForAllLevels(fileTarget); // only errors to file

      // Step 4. Activate the configuration
      LogManager.Configuration = config;
    }
  }
}