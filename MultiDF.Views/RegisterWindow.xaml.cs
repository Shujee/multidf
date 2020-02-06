using System.Windows;

namespace MultiDF.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class RegisterWindow : Window
  {
    public RegisterWindow()
    {
      InitializeComponent();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(((VM.RegisterVM)this.DataContext).MachineCode);
      VM.ViewModelLocator.DialogService.ShowMessage("Machine code has been copied to clipboard.", false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }
  }
}