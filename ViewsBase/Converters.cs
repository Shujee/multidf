﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ViewsBase
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
}