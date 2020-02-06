using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MultiDF.Views
{
  /// <summary>
  /// Interaction logic for InputBox.xaml
  /// </summary>
  public partial class InputBox : Window
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

    public string Value
    {
      get { return txtValue.Text; }
      set { txtValue.Text = value; }
    }

    public InputBox()
    {
      InitializeComponent();

      this.Title = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
    }

    public bool? ShowDialog(string description, string value = "", int maxLength = 100)
    {
      lblDescription.Text = description;
      txtValue.Text = value;
      txtValue.MaxLength = maxLength;

      return base.ShowDialog();
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
  }
}
