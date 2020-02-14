using Common;
using GalaSoft.MvvmLight;

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
        Expiry = "N/A";
      }
      else
      {
        var MachineCode = Encryption.Encrypt(LicenseGen.GetUniqueMachineId());
        var E = LicenseGen.ParseLicense(Settings.LicenseKey, Settings.RegEmail, MachineCode);

        if (E == null)
        {
          Status = "Not Registered";
          RegEmail = "N/A";
          Expiry = "N/A";
        }
        else
        {
          var ExpiryDate = System.DateTime.ParseExact(E, "MMM-dd-yyyy", System.Globalization.CultureInfo.CurrentCulture);

          if (ExpiryDate >= System.DateTime.Today)
          {
            Status = "Registered";
            RegEmail = Settings.RegEmail;
            Expiry = E;
          }
          else
          {
            Status = "Expired";
            RegEmail = Settings.RegEmail;
            Expiry = E;
          }
        }
      }
    }

    public string Version { get; private set; }
    public string Status { get; private set; }
    public string RegEmail { get; private set; }
    public string Expiry { get; private set; }
  }
}