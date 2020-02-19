using Common;
using GalaSoft.MvvmLight;
using System;

namespace MultiDF.VM
{
  public class AboutVM : ViewModelBase
  {
    public AboutVM()
    {
      var Settings = VM.Properties.Settings.Default;

      Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

      if (string.IsNullOrEmpty(Settings.LicenseKey) || string.IsNullOrEmpty(Settings.RegEmail))
      {
        Status = "Not Registered";
        RegEmail = "N/A";
        Expiry = null;
      }
      else
      {
        var MachineCode = Encryption.Encrypt(LicenseGen.GetUniqueMachineId());
        var Expiry = LicenseGen.ParseLicense(Settings.LicenseKey, Settings.RegEmail, MachineCode);

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

    public string Version { get; private set; }
    public string Status { get; private set; }
    public string RegEmail { get; private set; }
    public DateTime? Expiry { get; private set; }
  }
}