using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;

namespace VMBase
{
  public class RegisterVM : ViewModelBase
  {
    public RegisterVM()
    {
      _RegEmail = Properties.Settings.Default.RegEmail;
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
          var LocalLI = new LI()
          {
            app = ViewModelLocatorBase.App,
            code = MachineCode,
            email = _RegEmail
          };

          var Expiry = LicenseGen.ParseLicense(_LicenseKey, LocalLI);

          if (Expiry == null)
            return false;
          else
          {
            return Expiry >= DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
          }
        }
      }
    }

    public DateTime? ExpiryDate
    {
      get
      {
        if (string.IsNullOrEmpty(_RegEmail.Trim()) || string.IsNullOrEmpty(_LicenseKey.Trim()))
          return null;
        else
        {
          var LocalLI = new LI()
          {
            app = ViewModelLocatorBase.App,
            code = MachineCode,
            email = _RegEmail
          };

          return LicenseGen.ParseLicense(_LicenseKey, LocalLI);
        }
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
              ViewModelLocatorBase.DialogService.ShowMessage("E-mail and License Key must be provided.", true);
            else
            {
              var LocalLI = new LI()
              {
                app = ViewModelLocatorBase.App,
                code = MachineCode,
                email = _RegEmail
              };

              var Expiry = LicenseGen.ParseLicense(_LicenseKey, LocalLI);

              if (Expiry != null)
              {
                if (Expiry.Value < DateTime.Now)
                  ViewModelLocatorBase.DialogService.ShowMessage("This license key has expired. Please contact vendor.", true);
                else
                {
                  var Setting = Properties.Settings.Default;
                  Setting.RegEmail = _RegEmail.Trim();
                  Setting.LicenseKey = _LicenseKey.Trim();
                  Setting.Save();

                  ViewModelLocatorBase.DialogService.ShowMessage("Congratulations. You have successfully registered the product. You can now close this window and start using the product.", false);

                  RaisePropertyChanged(nameof(ExpiryDate));
                  RaisePropertyChanged(nameof(IsRegistered));
                }
              }
              else
                ViewModelLocatorBase.DialogService.ShowMessage("License Key and/or e-mail address is incorrect. Please contact vendor.", true);
            }
          },
          () => true);
        }

        return _RegisterCommand;
      }
    }
  }
}