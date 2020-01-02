using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using QuickGraph;
using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// This static class adds extension method to 
  /// </summary>
  public static class StaticExtensions
  {
    /// <summary>
    /// Exports the contents of this list to XML.
    /// </summary>
    /// <param name="selectedOnly">If True, only selected objects are exported, otherwise the entire list is exported.</param>
    /// <returns></returns>
    /// <remarks></remarks>

    /// <summary>
    /// Returns an available temporary file path with the specified extension
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string GetTempFileName(string extension)
    {
      var TempFile = Path.GetTempFileName();
      var Orig = TempFile;
      TempFile = Path.ChangeExtension(TempFile, extension);

      while (File.Exists(TempFile))
        TempFile = Path.ChangeExtension(Path.GetTempFileName(), extension);

      File.Delete(Orig);
      return TempFile;
    }
  }
}