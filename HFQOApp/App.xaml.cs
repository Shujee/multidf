using GalaSoft.MvvmLight.Ioc;
using HFQOViews;
using HFQOVM;
using System;
using System.Windows;

namespace HFQOApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, IApplicationService
  {
    private readonly DialogPresenter DLG = new DialogPresenter("HFQ Client App");

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      SimpleIoc.Default.Register<Common.IDialogService>(() => DLG);
      SimpleIoc.Default.Register<HFQOVM.IDialogService>(() => DLG);

      SimpleIoc.Default.Register<IApplicationService>(() => this);

#if (!DEBUG)
      if (!ViewModelLocator.HardwareHelper.IsWindows10())
      {
        ViewModelLocator.DialogService.ShowMessage("This program can only be run on Windows version 10.0 or newer.", true);
        Shutdown(1);
        return;
      }

      if (!ViewModelLocator.HardwareHelper.IsLaptop())
      {
        ViewModelLocator.DialogService.ShowMessage("This program can only be run on a laptop.", true);
        Shutdown(1);
        return;
      }

      if (ViewModelLocator.HardwareHelper.IsVM())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot be run on virtual machines.", true);
        Shutdown(1);
        return;
      }

      if (ViewModelLocator.HardwareHelper.HasMultipleScreens())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot run because this machine has multiple monitors or projector attached to it. ", true);
        Shutdown(1);
        return;
      }
#endif
      if (!ViewModelLocator.CameraService.InitCam())
      {
        ViewModelLocator.DialogService.ShowMessage("Camera service could not be initialized. This program cannot run.", true);
        Shutdown(1);
        return;
      }

      this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }

    void IApplicationService.Shutdown()
    {
      Dispatcher.Invoke(() => base.Shutdown(1));
    }
  }
}