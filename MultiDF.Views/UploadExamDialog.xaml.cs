using MultiDF.VM;
using HFQOModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using MultiDFCommon;

namespace MultiDF.Views
{
  /// <summary>
  /// Interaction logic for InputBox.xaml
  /// </summary>
  public partial class UploadExamDialog : Window
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

    private ICollectionView QAs => (this.Resources["QACVS"] as CollectionViewSource).View;

    public UploadExamDialog(UploadExamVM vm)
    {
      InitializeComponent();

      this.DataContext = vm;
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      var VM = (this.DataContext as UploadExamVM);
      
      VM.CreateNew = (this.TabControl.SelectedIndex == 0);

      if (VM.CreateNew)
      {
        if (string.IsNullOrEmpty(VM.NewExamNumber) || string.IsNullOrEmpty(VM.NewExamName))
          ViewModelLocator.DialogService.ShowMessage($"Exam number and name must be supplied.", true);
        else
        {
          VM.CheckExamNumberExists().ContinueWith(t =>
          {
            if (t.Result)
              ViewModelLocator.DialogService.ShowMessage($"Specified Master File number '{VM.NewExamNumber}' already exists on the server. Please specify a different number.", true);
            else
            {
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
              {
                this.DialogResult = true;
                this.Close();
              });
            }
          });
        }
      }
      else
      {
        if (VM.SelectedExam == null)
        {
          ViewModelLocator.DialogService.ShowMessage("Please select a Master File from the list to update.", true);
        }
        else
        {
          this.DialogResult = true;
          this.Close();
        }
      }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      SearchBox.Text = "";

      //Refresh filtering
      QAs.Refresh();
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        //Refresh filtering
        QAs.Refresh();
      }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
      //Refresh filtering
      QAs.Refresh();
    }

    private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
    {
      if (e.Item is MasterFile MF)
      {
        e.Accepted =
                      MF.number.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                      MF.name.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                      MF.origfilename.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
      }
    }
  }
}
