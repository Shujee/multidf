using GalaSoft.MvvmLight.Ioc;
using Common;
using NLog;
using NLog.Config;
using NLog.Targets;
using Model;

namespace VMBase
{
  /// <summary>
  /// This Class contains static references to all the view models in the
  /// application and provides an entry point for the bindings.
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class ViewModelLocatorBase
  {
    static ViewModelLocatorBase()
    {
      if (Properties.Settings.Default.UpgradeRequired)
      {
        Properties.Settings.Default.Upgrade();
        Properties.Settings.Default.UpgradeRequired = false;
        Properties.Settings.Default.Save();
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

      SimpleIoc.Default.Unregister<AuthVM>();
      SimpleIoc.Default.Unregister<RegisterVM>();
      SimpleIoc.Default.Unregister<AboutVM>();

      SimpleIoc.Default.Register<AuthVM>();
      SimpleIoc.Default.Register<RegisterVM>();
      SimpleIoc.Default.Register<AboutVM>();
    }

    /// <summary>
    /// Detatches all "tick clients" from Timer's Tick event to  avoid memory leaks.
    /// </summary>
    public static void Cleanup()
    {
    }
    
    /// <summary>
    /// This property will be set by child classes. RegisterVM and AboutVM use this property to decipher license keys.
    /// </summary>
    public static string App { get; set; }
    public static string AppVersion { get; set; }

    public static IDataService DataService => SimpleIoc.Default.GetInstance<IDataService>();
    public static IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

    public static AuthVM Auth => SimpleIoc.Default.GetInstance<AuthVM>();
    public static RegisterVM Register => SimpleIoc.Default.GetInstance<RegisterVM>();
    public static AboutVM About => SimpleIoc.Default.GetInstance<AboutVM>();

    /// <summary>
    /// This is the global logger object that can be used to write debugging information to addin's log file. The log file is named "activity.log" and is
    /// located in add-in's installation folder. For ClickOnce installation, this folder is in %appdata%.
    /// </summary>
    public static Logger Logger => LogManager.GetLogger("MultiDFLogger");

    protected static void InitLogger(string logPath)
    {
      // Step 1. Create configuration object 
      var config = new LoggingConfiguration();

      // Step 2. Create target log file
      var fileTarget = new FileTarget("MultiDFLogger")
      {
        FileName = logPath,
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