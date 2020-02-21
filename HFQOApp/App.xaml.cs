﻿using GalaSoft.MvvmLight.Ioc;
using HFQOViews;
using HFQOVM;
using System;
using System.Windows;

namespace HFQOApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private readonly DialogPresenter DLG = new DialogPresenter("HFQ Client App");

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      SimpleIoc.Default.Register<Common.IDialogService>(() => DLG);
      SimpleIoc.Default.Register<HFQOVM.IDialogService>(() => DLG);

      if (!ViewModelLocator.HardwareHelper.IsLaptop())
      {
        ViewModelLocator.DialogService.ShowMessage("This program can only be run on a laptop.", true);
        Shutdown(1);
        return;
      }
      else if (!ViewModelLocator.HardwareHelper.IsVM())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot be run on virtual machines.", true);
        Shutdown(1);
        return;
      }
      else if (!ViewModelLocator.HardwareHelper.HasMultipleScreens())
      {
        ViewModelLocator.DialogService.ShowMessage("This program cannot run because this machine has multiple monitors or projector attached to it. ", true);
        Shutdown(1);
        return;
      }
      else
      {
        this.StartupUri = new Uri("Window1.xaml", UriKind.Relative);
      }
    }
  }
}