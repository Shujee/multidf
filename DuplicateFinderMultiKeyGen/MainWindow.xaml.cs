using DuplicateFinderMultiCommon;
using System;
using System.Windows;

namespace DuplicateFinderMultiKeyGen
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

    private void txtGenerate_Click(object sender, RoutedEventArgs e)
    {
      if (txtEmail.Text.Trim() == "" || txtCode.Text.Trim() == "")
        MessageBox.Show("E-mail and Code must be provided.", "DuplicateFinderMultiKeyGen");
      else if(!ExpiryDatePicker.SelectedDate.HasValue)
        MessageBox.Show("Expiry date must be selected.", "DuplicateFinderMultiKeyGen");
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

    private void txtClose_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Are you sure you want to exit?", "DuplicateFinderMultiKeyGen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        this.Close();
    }

    private void txtCopy_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(txtLicenseKey.Text);
    }
  }
}
