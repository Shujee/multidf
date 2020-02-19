using Microsoft.Win32;
using System;

namespace Common
{
  public class LicenseGen
  {
    private const string DATE_FORMAT = "MMM-dd-yyyy";

    public static string GetUniqueMachineId()
    {
      try
      {
        using (var localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64))
        {
          RegistryKey sqlServerKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
          return (string)sqlServerKey.GetValue("MachineGuid");
        }
      }
      catch
      {
        throw new Exception("An error occurred while trying to identify the machine. Please report this error to the vendor.");
      }
    }

    /// <summary>
    /// Returns a license key for the specified email address, machine code and user type
    /// </summary>
    /// <param name="email"></param>
    /// <param name="machineCode"></param>
    /// <param name="ut"></param>
    /// <returns></returns>
    public static string CreateLicense(string email, string machineCode, DateTime expiry)
    {
      return Encryption.Encrypt(email.Trim() + ',' + expiry.ToString(DATE_FORMAT) + ',' + machineCode.Trim());
    }

    /// <summary>
    /// Returns license expiry date by parsing provided license key, machine code and email values.
    /// </summary>
    /// <param name="licenseKey"></param>
    /// <param name="email"></param>
    /// <param name="machineCode"></param>
    /// <returns></returns>
    public static DateTime? ParseLicense(string licenseKey, string email, string machineCode)
    {
      try
      {
        var Dec = Encryption.Decrypt(licenseKey);
        var Chunks = Dec.Split(',');

        if (Chunks != null && Chunks.Length == 3)
        {
          if (Chunks[0] == email.Trim() && Chunks[2] == machineCode.Trim())
          {
            var DT = DateTime.ParseExact(Chunks[1], DATE_FORMAT, System.Globalization.CultureInfo.CurrentCulture);
            return DateTime.SpecifyKind(DT, DateTimeKind.Utc); //convert it to UTC-based DateTime object
          }
          else
            return null;
        }
        else
          return null;
      }
      catch
      {
        return null;
      }
    }
  }
}
