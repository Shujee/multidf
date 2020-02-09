using MultiDFCommon;
using System;
using System.Windows;

namespace MultiDFKeyGen
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
      if (txtEmail.Text.Trim() == "" || txtCode.Text.Trim() == "")
        MessageBox.Show("E-mail and Code must be provided.", "MultiDF KeyGen");
      else if(!ExpiryDatePicker.SelectedDate.HasValue)
        MessageBox.Show("Expiry date must be selected.", "MultiDF KeyGen");
      else
      {
        try
        {
          txtLicenseKey.Text = LicenseGen.CreateLicense(txtEmail.Text, txtCode.Text, ExpiryDatePicker.SelectedDate.Value.Date);
        }
        catch (Exception ee)
        {
          MessageBox.Show(ee.Message);
        }
      }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Are you sure you want to exit?", "MultiDF KeyGen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        this.Close();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(txtLicenseKey.Text);
    }
  }
}
