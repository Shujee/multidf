using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;

namespace VMBase
{
  public interface IRegistrationService : INotifyPropertyChanged
  {
    DateTime? ExpiryDate { get; }
    bool IsRegistered { get; }
    string LicenseKey { get; set; }
    string MachineCode { get; }
    string RegEmail { get; set; }
    RelayCommand RegisterCommand { get; }
  }
}