using System;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using Microsoft.Office.Core;

namespace MultiDF
{
  public class WordHelper
  {
    internal static bool WordHasFocus
    {
      get
      {
        IntPtr wordHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        IntPtr focusedWindow = NativeMethods.GetForegroundWindow();
        return wordHandle == focusedWindow;
      }
    }

    public static object GetWindowSafe(Microsoft.Office.Tools.CustomTaskPane ctp)
    {
      try
      {
        return ctp.Window;
      }
      catch
      {
        return null;
      }
    }

    internal static void WriteDocumentProperty(Document doc, string prop, string value)
    {
      var properties = (DocumentProperties)doc.CustomDocumentProperties;

      var ExistingProperty = ReadDocumentProperty(doc, prop);
      if (ExistingProperty != null) properties[prop].Delete();

      properties.Add(prop, false, Microsoft.Office.Core.MsoDocProperties.msoPropertyTypeString, value);
    }

    internal static string ReadDocumentProperty(Document doc, string propertyName)
    {
      DocumentProperties properties;
      properties = (DocumentProperties)doc.CustomDocumentProperties;

      foreach (DocumentProperty prop in properties)
      {
        if (prop.Name == propertyName)
          return prop.Value.ToString();
      }

      return null;
    }

    internal static void WriteBuiltInDocumentProperty(Document doc, string prop, string value)
    {
      DocumentProperties documentProperties = doc.BuiltInDocumentProperties;
      documentProperties[prop].Value = value;
    }

    internal static string ReadBuiltInDocumentProperty(Document doc, string prop)
    {
      DocumentProperties documentProperties = doc.BuiltInDocumentProperties;
      return documentProperties[prop].Value;
    }
  }
}