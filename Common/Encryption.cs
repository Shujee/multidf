using Microsoft.Win32;
using System;
using System.Security.Cryptography;

namespace Common
{
  public static class Encryption
  {
    private static readonly string KEY = "IH%!Q8337SisAE64AKEYJD687!@#$*77dkccdfysdf))ERIU((**@&#947"; //GetSysUUID(); 

    private static TripleDESCryptoServiceProvider CreateCryptoService()
    {
      using (var HashProvider = new MD5CryptoServiceProvider())
      {
        try
        {
          byte[] TDESKey = HashProvider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(KEY));

          // Step 2. Create a new TripleDESCryptoServiceProvider object
          TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider
          {

            // Step 3. Setup the encoder
            Key = TDESKey,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
          };

          return TDESAlgorithm;
        }
        catch (Exception)
        {
          return null;
        }
        finally
        {
          HashProvider.Clear();
        }
      }
    }

    /// <summary>
    /// Encrypts the specified string using Triple-DES algorithm and returns the encrypted string in BASE64 encoding. Used for small chunks of data
    /// such as usernames and passwords. 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Encrypt(string str)
    {
      using (var TDESAlgorithm = CreateCryptoService())
      {
        if (TDESAlgorithm == null)
          throw new Exception("Error creating CryptoService.");
        else
        {
          byte[] DataToEncrypt = System.Text.Encoding.UTF8.GetBytes(str);

          try
          {
            ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
            byte[] Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            return Convert.ToBase64String(Results);
          }
          finally
          {
            TDESAlgorithm.Clear();
          }
        }
      }
    }

    /// <summary>
    /// Decrypts the BASE64 encoded encrypted string using Triple-DES algorithm and returns the decrypted string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Decrypt(string str)
    {
      using (var TDESAlgorithm = CreateCryptoService())
      {
        if (TDESAlgorithm == null)
          throw new Exception("Error creating CryptoService.");
        else
        {
          byte[] DataToDecrypt = Convert.FromBase64String(str);

          try
          {
            ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
            byte[] Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            return System.Text.Encoding.UTF8.GetString(Results);
          }
          finally
          {
            TDESAlgorithm.Clear();
          }
        }
      }
    }

    /// <summary>
    /// Encrypts the specified byte array using Triple-DES algorithm and returns the encrypted byte array. Used for large blocks of data
    /// such as files.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] content)
    {
      using (var TDESAlgorithm = CreateCryptoService())
      {
        if (TDESAlgorithm == null)
          throw new Exception("Error creating CryptoService.");
        else
        {
          try
          {
            ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
            return Encryptor.TransformFinalBlock(content, 0, content.Length);
          }
          finally
          {
            TDESAlgorithm.Clear();
          }
        }
      }
    }

    /// <summary>
    /// Decrypts the specified byte array using Triple-DES algorithm and returns the decrypted byte array.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] content)
    {
      using (var TDESAlgorithm = CreateCryptoService())
      {
        if (TDESAlgorithm == null)
          throw new Exception("Error creating CryptoService.");
        else
        {
          try
          {
            ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
            return Decryptor.TransformFinalBlock(content, 0, content.Length);
          }
          finally
          {
            TDESAlgorithm.Clear();
          }
        }
      }
    }
  }
}