using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiDF.Views
{
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
            Result = DEL;
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