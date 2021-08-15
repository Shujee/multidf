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
    private readonly DialogPresenter DLG = new DialogPresenter(null, "HFQ Client App");

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      SimpleIoc.Default.Register<VMBase.ILogger, HFQOVM.HFQLogger>();
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

      if (ViewModelLocator.CameraService.BestResCamera == null)
      {
        ViewModelLocator.DialogService.ShowMessage("Camera service could not be initialized. This program cannot run.", true);
        Shutdown(1);
        return;
      }
#endif

      this.DispatcherUnhandledException += App_DispatcherUnhandledException;
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      string msg = "";

      if (e.IsTerminating)
        msg += "TERMINATION: ";

      if (e.ExceptionObject is Exception ee)
      {
        msg += ee.Message;
        msg += ee.StackTrace;

        ViewModelLocator.Logger.Error(msg);
        ViewModelLocator.DialogService.ShowMessage("The following unhandled exception occurred in HFQ application: " + msg, true);
      }
      else
        ViewModelLocator.DialogService.ShowMessage("An unhandled exception occurred in HFQ application. The type of ExceptionObject is " + e.ExceptionObject.GetType().Name, true);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      string msg = "TERMINATION: ";

      msg += e.Exception.Message;
      msg += e.Exception.StackTrace;

      ViewModelLocator.Logger.Error(msg);
      ViewModelLocator.DialogService.ShowMessage("The following unhandled exception occurred in HFQ application: " + msg, true);
    }

    void IApplicationService.Shutdown()
    {
      Dispatcher.Invoke(() => base.Shutdown(1));
    }
  }
}