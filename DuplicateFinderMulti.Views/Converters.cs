﻿using DiffPlex.DiffBuilder.Model;
using DuplicateFinderMulti.VM;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
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
  /// Takes an undirected tagged QuickGraph object and returns all the tags associated with its edges. Used to convert processing results graph
  /// to list of DFResults for showing in Results Tree.
  /// </summary>
  [ValueConversion(typeof(UndirectedGraph<XMLDoc, OurEdge>), typeof(IEnumerable<DFResult>))]
  public class GraphToEdgeTagConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is UndirectedGraph<XMLDoc, OurEdge> g)
        return g.Edges.Select(e => e.Tag);
      else
        throw new ArgumentException("value must be of type UndirectedGraph<XMLDoc, OurEdge<XMLDoc, DFResult>>." + value.GetType());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}