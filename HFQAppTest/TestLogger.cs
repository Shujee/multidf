using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HFQOVM
{
  public class TestLogger : VMBase.ILogger
  {
    private TestContext context;

    public TestLogger(TestContext testContext)
    {
      context = testContext;
    }

    public void Error(string message)
    {
      context.WriteLine("ERROR: " + message);
    }

    public void Error(System.Exception ex, string message)
    {
      context.WriteLine("EXCEPTION: " + ex.Message + "\n" + message);
    }

    public void Info(string message)
    {
      context.WriteLine("INFO: " + message);
    }

    public void Warn(System.Exception ex, string message)
    {
      context.WriteLine("WARNING: " + ex.Message + "\n" + message);
    }
  }
}