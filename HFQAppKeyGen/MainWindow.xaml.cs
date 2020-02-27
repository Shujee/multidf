using Common;
using System;
using System.Windows;

namespace HFQAppKeyGen
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
      CopiedLabel.Visibility = Visibility.Hidden;

      if (txtEmail.Text.Trim() == "" || txtCode.Text.Trim() == "")
        MessageBox.Show("E-mail and Code must be provided.", "KeyGen");
      else if(!ExpiryDatePicker.SelectedDate.HasValue)
        MessageBox.Show("Expiry date must be selected.", "KeyGen");
      else
      {
        try
        {
          var li = new LI()
          {
            app = "HFQApp",
            email = txtEmail.Text.Trim(),
            code = txtCode.Text.Trim(),
            expiry = DateTime.SpecifyKind(ExpiryDatePicker.SelectedDate.Value.Date, DateTimeKind.Utc)
          };

          txtLicenseKey.ItemsSource = LicenseGen.CreateLicense(li).ToCharArray();
        }
        catch (Exception ee)
        {
          MessageBox.Show(ee.Message);
        }
      }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Are you sure you want to exit?", "HFQApp KeyGen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        this.Close();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
      if (txtLicenseKey.Items.SourceCollection is char[] c && c != null)
      {
        Clipboard.SetText(string.Join("", c));
        CopiedLabel.Visibility = Visibility.Visible;
      }
    }
  }
}
