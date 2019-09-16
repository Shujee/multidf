using DiffPlex.DiffBuilder.Model;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DuplicateFinderMulti.Views
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
      {
        return Colors.Green;
      }
      else
      {
        return Colors.Red;
      }
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
      {
        return "Status: Connected";
      }
      else
      {
        return "Status: Not connected";
      }
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
}