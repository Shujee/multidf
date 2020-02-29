using Common;
using GalaSoft.MvvmLight;
using System;

namespace VMBase
{
  public class AboutVM : ViewModelBase
  {
    public AboutVM()
    {
      var Settings = VMBase.Properties.Settings.Default;

      if (string.IsNullOrEmpty(Settings.LicenseKey) || string.IsNullOrEmpty(Settings.RegEmail))
      {
        Status = "Not Registered";
        RegEmail = "N/A";
        Expiry = null;
      }
      else
      {
        var MachineCode = Encryption.Encrypt(LicenseGen.GetUniqueMachineId());

        var LocalLI = new LI()
        {
          app = ViewModelLocatorBase.App,
          code = MachineCode,
          email = Settings.RegEmail
        };

        var Expiry = LicenseGen.ParseLicense(Settings.LicenseKey, LocalLI);

        if (Expiry == null)
        {
          Status = "Not Registered";
          RegEmail = "N/A";
          this.Expiry = null;
        }
        else
        {
          if (Expiry >= DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc))
            Status = "Registered";
          else
            Status = "Expired";

          RegEmail = Settings.RegEmail;
          this.Expiry = Expiry.Value;
        }
      }
    }

    public string Status { get; private set; }
    public string RegEmail { get; private set; }
    public DateTime? Expiry { get; private set; }
  }
}