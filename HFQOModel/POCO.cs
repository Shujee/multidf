namespace HFQOModel
{
  public class IDName
  {
    public string ID { get; set; }
    public string Name { get; set; }
  }

  public class LoginToken
  {
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
  }

  /// <summary>
  /// A master file represents a single examination paper. XPS property contains Base64-encoded, encyrpted version of the XPS file
  /// that is used to display this exam in the client app. XML property contains actual QA text used to perform searching.
  /// </summary>
  public class MasterFile
  {
    /// <summary>
    /// Master XPS File in Base64 encoding.
    /// </summary>
    public string xps { get; set; }

    /// <summary>
    /// Master XML File in Base64 encoding.
    /// </summary>
    public string xml { get; set; }
  }
}