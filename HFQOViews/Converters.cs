using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Xps.Packaging;

namespace HFQOViews
{
  [ValueConversion(typeof(string), typeof(System.Windows.Documents.FixedDocumentSequence))]
  public class PathToFixedDocumentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || (value is string s && s == ""))
        return null;
      else
      {
        XpsDocument doc = new XpsDocument((string)value, FileAccess.Read);
        return doc.GetFixedDocumentSequence();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
