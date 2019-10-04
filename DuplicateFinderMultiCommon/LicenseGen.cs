using Microsoft.Win32;
using System;

namespace DuplicateFinderMultiCommon
{
  public class LicenseGen
  {
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
      return Encryption.Encrypt(email.Trim() + ',' + expiry.ToString("MMM-dd-yyyy") + ',' + machineCode.Trim());
    }

    /// <summary>
    /// Returns the user type by parsing provided license key, machine code and email values.
    /// </summary>
    /// <param name="licenseKey"></param>
    /// <param name="email"></param>
    /// <param name="machineCode"></param>
    /// <returns></returns>
    public static string ParseLicense(string licenseKey, string email, string machineCode)
    {
      try
      {
        var Dec = Encryption.Decrypt(licenseKey);
        var Chunks = Dec.Split(',');

        if (Chunks != null && Chunks.Length == 3)
        {
          if (Chunks[0] == email.Trim() && Chunks[2] == machineCode.Trim())
            return Chunks[1];
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
