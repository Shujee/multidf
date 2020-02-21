using HFQOVM;
using System.Windows;

namespace HFQOApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      if (!ViewModelLocator.HardwareHelper.IsLaptop())
      {
        ViewModelLocator.DialogService.ShowMessage("This program can only be run on a laptop.", true);
        return;
      }
      else if (!ViewModelLocator.HardwareHelper.IsVM())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot be run on virtual machines.", true);
        return;
      }
      else if (!ViewModelLocator.HardwareHelper.HasMultipleScreens())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot run because this machine has multiple monitors or projector attached to it. ", true);
        return;
      }
      else
      {
        MainWindow window = new MainWindow();
        window.Show();
      }
    }
  }
}