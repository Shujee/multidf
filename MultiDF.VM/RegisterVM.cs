using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;

namespace MultiDF.VM
{
  public class RegisterVM : ViewModelBase
  {
    public RegisterVM()
    {      
      _RegEmail= Properties.Settings.Default.RegEmail;
      _LicenseKey = Properties.Settings.Default.LicenseKey;
    }

    public string MachineCode => Encryption.Encrypt(LicenseGen.GetUniqueMachineId());

    private string _RegEmail;
    public string RegEmail
    {
      get { return _RegEmail; }
      set { Set(ref _RegEmail, value); }
    }

    private string _LicenseKey;
    public string LicenseKey
    {
      get { return _LicenseKey; }
      set { Set(ref _LicenseKey, value); }
    }

    public bool IsRegistered
    {
      get
      {
        if (string.IsNullOrEmpty(_RegEmail.Trim()) || string.IsNullOrEmpty(_LicenseKey.Trim()))
          return false;
        else
        {
          var Expiry = LicenseGen.ParseLicense(_LicenseKey, _RegEmail, MachineCode);

          if (Expiry == null)
            return false;
          else
          {
            return Expiry >= DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
          }
        }
      }
    }

    public System.DateTime? ExpiryDate
    {
      get
      {
        if (string.IsNullOrEmpty(_RegEmail.Trim()) || string.IsNullOrEmpty(_LicenseKey.Trim()))
          return null;
        else
          return LicenseGen.ParseLicense(_LicenseKey, _RegEmail, MachineCode);
      }
    }

    private RelayCommand _RegisterCommand;
    public RelayCommand RegisterCommand
    {
      get
      {
        if (_RegisterCommand == null)
        {
          _RegisterCommand = new RelayCommand(() =>
          {
            if (string.IsNullOrEmpty(_RegEmail.Trim()) || string.IsNullOrEmpty( _LicenseKey.Trim()))
              ViewModelLocator.DialogService.ShowMessage("E-mail and License Key must be provided.", true);
            else
            {
              var Expiry = LicenseGen.ParseLicense(_LicenseKey, _RegEmail, MachineCode);
              RaisePropertyChanged(nameof(IsRegistered));

              if (Expiry != null)
              {
                var Setting = Properties.Settings.Default;
                Setting.RegEmail = _RegEmail.Trim();
                Setting.LicenseKey = _LicenseKey.Trim();
                Setting.Save();

                ViewModelLocator.DialogService.ShowMessage("Congratulations. You have successfully registered the product. You can now close this window and start using the product.", false);
              }
              else
                ViewModelLocator.DialogService.ShowMessage("License Key and/or e-mail address is incorrect. Please contact vendor.", true);
            }
          },
          () => true);
        }

        return _RegisterCommand;
      }
    }
  }
}