using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;

namespace HFQAppTest
{
  internal class TestRegistrationService : VMBase.IRegistrationService
  {
    public DateTime? ExpiryDate => DateTime.Now.AddYears(1);

    public bool IsRegistered => true;

    public string LicenseKey
    {
      get => ""; 
      set
      {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(LicenseKey)));
      }
    }

    public string MachineCode => "";

    public string RegEmail { get => "a@b.com"; set { } }

    public RelayCommand RegisterCommand
    {
      get
      {
        return new RelayCommand(() =>
        {
          Console.WriteLine("Registered successfully.");
        },
        () => true);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}