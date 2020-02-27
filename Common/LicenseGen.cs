using Microsoft.Win32;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
  public class LI
  {
    public string email { get; set; }
    public string code { get; set; }
    public DateTime expiry { get; set; }
    public string app { get; set; }

    // override object.Equals
    public override bool Equals(object obj)
    {
      if (obj == null || GetType() != obj.GetType())
      {
        return false;
      }

      var objLI = obj as LI;

      return objLI.app.Trim() == this.app.Trim() &&
            objLI.email.Trim() == this.email.Trim() &&
            objLI.code.Trim() == this.code.Trim();
    }

    public override int GetHashCode()
    {
      return (this.app.Trim() + this.email.Trim() + this.code.Trim()).GetHashCode();
    }
  }

  public static class LicenseGen
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
    public static string CreateLicense(LI li)
    {
      var liXML = li.Serialize();
      return Encryption.Encrypt(liXML);
    }

    /// <summary>
    /// Returns license expiry date by parsing provided license key, machine code and email values.
    /// </summary>
    /// <param name="licenseKey"></param>
    /// <param name="email"></param>
    /// <param name="machineCode"></param>
    /// <returns></returns>
    public static DateTime? ParseLicense(string licenseKey, LI local)
    {
      try
      {
        var Dec = Encryption.Decrypt(licenseKey);

        var li = Dec.Deserialize<LI>();

        if (li.Equals(local))
        {
          return DateTime.SpecifyKind(li.expiry.Date, DateTimeKind.Utc); //convert it to UTC-based DateTime object
        }
        else
          return null;
      }
      catch
      {
        return null;
      }
    }

    private static string Serialize<T>(this T value)
    {
      if (value == null)
        return string.Empty;
      else
      {
        var xmlserializer = new XmlSerializer(typeof(T));
        var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter))
        {
          xmlserializer.Serialize(writer, value);
          return stringWriter.ToString();
        }
      }
    }

    private static T Deserialize<T>(this string xml)
    {
      using (TextReader reader = new StringReader(xml))
      {
        return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
      }
    }
  }
}
