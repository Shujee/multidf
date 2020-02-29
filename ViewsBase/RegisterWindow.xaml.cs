using System.Windows;

namespace ViewsBase
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
      Clipboard.SetText(((VMBase.RegisterVM)this.DataContext).MachineCode);
      VMBase.ViewModelLocatorBase.DialogService.ShowMessage("Machine code has been copied to clipboard.", false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as VMBase.RegisterVM).RegisterCommand.Execute(null);
    }
  }
}