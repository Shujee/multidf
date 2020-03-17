using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HFQOViews
{
  /// <summary>
  /// Interaction logic for InputBox.xaml
  /// </summary>
  public partial class ExamsListDialog : Window
  {
    #region Hide System Menu and Close Button
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //Do not show Close button
      var hwnd = new WindowInteropHelper(this).Handle;
      SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }
    #endregion

    public ExamsListDialog()
    {
      InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }

    private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
    {
      //(sender as ComboBox).IsDropDownOpen = true;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as HFQOVM.HFQVM).RefreshExamsList();
    }

    private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      //Larry wants to disable mouse wheel effect in exams dropdown.
      e.Handled = true;
    }
  }
}
