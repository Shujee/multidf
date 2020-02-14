using GalaSoft.MvvmLight.Ioc;
using Common;
using NLog;
using NLog.Config;
using NLog.Targets;

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

      if (!GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
      {
        InitLogger();
      }

      GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();

      SimpleIoc.Default.Unregister<AuthVM>();
      
      SimpleIoc.Default.Register<AuthVM>();
    }

    /// <summary>
    /// Detatches all "tick clients" from Timer's Tick event to  avoid memory leaks.
    /// </summary>
    public static void Cleanup()
    {
    }

    public static IDataService DataService => SimpleIoc.Default.GetInstance<IDataService>();
    public static IDialogService DialogService => SimpleIoc.Default.GetInstance<IDialogService>();

    public static AuthVM Auth => SimpleIoc.Default.GetInstance<AuthVM>();

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