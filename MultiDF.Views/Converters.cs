using DiffPlex.DiffBuilder.Model;
using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace MultiDF.Views
{
  /// <summary>
  /// Returns Green if input value is True, otherwise Red. Used to show DB connection status.
  /// </summary>
  [ValueConversion(typeof(bool), typeof(Color))]
  public class ConnectionStatusConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool)value)
        return Colors.Green;
      else
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((Brush)value).Equals(Brushes.Green);
    }
  }

  /// <summary>
  /// Returns "Connected" if the specified value is True, "Not connected" otherwise.
  /// </summary>
  [ValueConversion(typeof(bool), typeof(string))]
  public class ConnectionStatusToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((bool)value)
        return "Connection Status: Connected";
      else
        return "Connection Status: Not connected";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((Brush)value).Equals(Brushes.Green);
    }
  }

  /// <summary>
  /// Returns a color between Red and Green showing relative dissimilarity of the item from its group.
  /// </summary>
  [ValueConversion(typeof(int), typeof(Color))]
  public class DistanceToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int v = (int)value;

      if (v < 50)
      {
        byte Green = (byte)(((100 - (float)v) / 100) * 0x44);
        return Color.FromArgb(0x44, 0, Green, 0);
      }
      else
      {
        byte Red = (byte)((v / 100) * 0x44);
        return Color.FromArgb(0x44, Red, 0, 0);
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return new NotSupportedException();
    }
  }

  public class DiffTypeToColorConverter : IValueConverter
  {
    private static Color DEL = Color.FromArgb(100, 255, 0, 0);
    private static Color INS = Color.FromArgb(100, 0, 255, 0);
    private static Color MOD = Color.FromArgb(100, 0, 0, 255);

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is ChangeType)
      {
        Color Result = Colors.Transparent;

        switch ((ChangeType)value)
        {
          case ChangeType.Deleted:
            Result =  DEL;
            break;
          case ChangeType.Inserted:
            Result = INS;
            break;
          case ChangeType.Modified:
            Result = MOD;
            break;
        }

        return Result;
      }
      else
      {
        throw new ArgumentException("value must be of type DiffType");
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class SubPiecesOrTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value is DiffPiece v)
      {
        if (v.SubPieces != null && v.SubPieces.Count > 0)
          return v.SubPieces;
        else
          return new[] { v };
      }
      else
      {
        throw new ArgumentException("value must be of type DiffPiece");
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Extracts and returns file name part from a path
  /// </summary>
  [ValueConversion(typeof(string), typeof(string))]
  public class PathToFileNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var Path = (string)value;

      if (!string.IsNullOrEmpty(Path))
        return System.IO.Path.GetFileName(Path);
      else
        return "<None>";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  /// Returns true if value is greater than 0, else false.
  /// </summary>
  [ValueConversion(typeof(string), typeof(string))]
  public class NonZeroToTrueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((int)value > 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  /// <summary>
  /// Returns Boolean inverse of the input value
  /// </summary>
  [ValueConversion(typeof(bool), typeof(bool))]
  public class NegationConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return !(bool)value;
    }
  }

  public class ByteArrayToXPSDocumentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return null;

      MemoryStream xpsStream = new MemoryStream((byte[])value);

      using (Package package = Package.Open(xpsStream))
      {
        //Create URI for Xps Package
        //Any Uri will actually be fine here. It acts as a place holder for the
        //Uri of the package inside of the PackageStore

        string inMemoryPackageName = "memorystream://myXps.xps";

        Uri packageUri = new Uri(inMemoryPackageName);

        //Add package to PackageStore
        PackageStore.AddPackage(packageUri, package);

        XpsDocument xpsDoc = new XpsDocument(package, CompressionOption.Maximum, inMemoryPackageName);

        FixedDocumentSequence fixedDocumentSequence = xpsDoc.GetFixedDocumentSequence();

        // Do operations on xpsDoc here
        //Note: Please note that you must keep the Package object in PackageStore until you
        //are completely done with it since certain operations on XpsDocument can trigger
        //delayed resource loading from the package.

        //PackageStore.RemovePackage(packageUri);
        //xpsDoc.Close();

        return fixedDocumentSequence;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

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