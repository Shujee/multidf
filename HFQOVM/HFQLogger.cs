using NLog;
using NLog.Config;
using NLog.Targets;

namespace HFQOVM
{
  public class HFQLogger : VMBase.ILogger
  {
    private string LoggerName = "HFQLogger";
    private Logger _logger; 

    public HFQLogger()
    {
      var LogDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "HFQApp");
      if (!System.IO.Directory.Exists(LogDir))
        System.IO.Directory.CreateDirectory(LogDir);

      InitLogger("${specialfolder:folder=ApplicationData}/HFQApp/");
      _logger = LogManager.GetLogger(LoggerName);
    }

    public void Error(string message)
    {
      _logger.Error(message);
    }

    public void Error(System.Exception ex, string message)
    {
      _logger.Error(ex, message);
    }

    public void Info(string message)
    {
      _logger.Info(message);
    }

    public void Warn(System.Exception ex, string message)
    {
      _logger.Warn(ex, message);
    }

    private void InitLogger(string logFolder)
    {
      // Step 1. Create configuration object 
      var config = new LoggingConfiguration();

      // Step 2. Create target log file
      var fileTarget = new FileTarget(LoggerName)
      {
        FileName = logFolder + "${shortdate}.log",
        Layout = "${longdate} ${level} ${message}  ${exception}"
      };
      config.AddTarget(fileTarget);

      // Step 3. Define rules
      config.AddRuleForAllLevels(fileTarget);

      // Step 4. Activate the configuration
      LogManager.Configuration = config;
    }
  }
}