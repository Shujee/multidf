using DuplicateFinderMulti.VM;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DuplicateFinderMulti.Views
{
  public partial class LoginWindow
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

    public LoginWindow()
    {
      // This call is required by the designer.
      InitializeComponent();

      //Password box cannot be bound in XAML
      PasswordTextBox.Password = ViewModelLocator.Auth.Password;
    }

    private void OK_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      ViewModelLocator.Auth.Password = PasswordTextBox.Password; //PasswordBox is not using Binding

      if(string.IsNullOrEmpty(ViewModelLocator.Auth.Email ) || string.IsNullOrEmpty(ViewModelLocator.Auth.Password))
      {
        ViewModelLocator.DialogService.ShowMessage("E-mail and Password fields must not be blank.", true);
        return;
      }

      this.DialogResult = true;
      this.Close();
    }

    private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }
  }
}