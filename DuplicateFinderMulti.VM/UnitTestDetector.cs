using System;
using System.Linq;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Detects if we are running inside a unit test.
  /// </summary>
  public static class UnitTestDetector
  {
    private const string testAssemblyName = "Microsoft.VisualStudio.TestPlatform.TestFramework";
    public static bool IsInUnitTest => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith(testAssemblyName));
  }
}
