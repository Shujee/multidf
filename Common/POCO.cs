namespace Common
{
  public enum UserType
  {
    None = 0,
    Admin = 1,
    Uploader = 2,
    Downloader = 3
  }

  public class IDName
  {
    public string ID { get; set; }
    public string Name { get; set; }
  }

  public class LoginResponse
  {
    public int type { get; set; }
    public LoginToken token { get; set; }
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
    /// ID of this master file
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// Exam number. Must be unique for new master files.
    /// </summary>
    public string number { get; set; }

    /// <summary>
    /// Exam name
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Master XPS File in Base64 encoding.
    /// </summary>
    public string xps { get; set; }

    /// <summary>
    /// Master XML File in Base64 encoding.
    /// </summary>
    public string xml { get; set; }

    /// <summary>
    /// Number of QAs in this master file.
    /// </summary>
    public int qa_count { get; set; }

    /// <summary>
    /// Name of the original DOCX file that was used to upload this exam.
    /// </summary>
    public string origfilename { get; set; }

    /// <summary>
    /// Represents the type of changes that are contained in this Master File. Can be one of the 3 values UPDATED, CORRECTIONS, FIXES. Null for new master files.
    /// </summary>
    public string remarks { get; set; }

    /// <summary>
    /// The date on which this Master File was last updated.
    /// </summary>
    public string updated_at { get; set; }
  }

  /// <summary>
  /// Represents an exam that can be downloaded by the currently logged in user. 
  /// </summary>
  public class AccessibleMasterFile
  {
    /// <summary>
    /// Access ID that allows currently logged in user to download an exam
    /// </summary>
    public int access_id { get; set; }

    /// <summary>
    /// ID of this master file
    /// </summary>
    public int exam_id { get; set; }

    /// <summary>
    /// Exam number. Must be unique for new master files.
    /// </summary>
    public string number { get; set; }

    /// <summary>
    /// Exam name
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Number of QAs in this master file.
    /// </summary>
    public int qa_count { get; set; }

    /// <summary>
    /// Represents the type of changes that are contained in this Master File. Can be one of the 3 values UPDATED, CORRECTIONS, FIXES. Null for new master files.
    /// </summary>
    public string remarks { get; set; }

    /// <summary>
    /// The date on which this Master File was last updated.
    /// </summary>
    public string updated_at { get; set; }

    /// <summary>
    /// The end date of this access after which current user will not be able to download this exam
    /// </summary>
    public string end { get; set; }
  }

  /// <summary>
  /// Represents a single result row that stores question number and index of up to 3 matching entries in the master file.
  /// </summary>
  public class HFQResultRow
  {
    public int q { get; set; }
    public int? a1 { get; set; }
    public int? a2 { get; set; }
    public int? a3 { get; set; }
  }
}