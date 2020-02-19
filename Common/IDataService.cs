using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
  public interface IDataService
  {
    /// <summary>
    /// Pings HFQ server to see if it is available.
    /// </summary>
    /// <returns></returns>
    bool IsAlive();

    /// <summary>
    /// Tries to login to the server using specified email address and password. Returns true for this session if successful. Authentication token returned
    /// by the server is saved for subsequent server calls.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<User> Login(string email, string password);

    /// <summary>
    /// Clears locally saved authentication token.
    /// </summary>
    Task Logout();

    /// <summary>
    /// Downloads XML and XPS files of the master file of specified Access from the server. User must have been granted access by the admin to download this master file.
    /// </summary>
    /// <param name="machine_name"></param>
    /// <param name="access_id"></param>
    /// <returns></returns>
    MasterFile DownloadExam(int access_id, string machine_name);

    /// <summary>
    /// Creates a new exam on the server. Can only be done by "uploader"-type users.
    /// </summary>
    /// <param name="xpsPath"></param>
    /// <param name="xmlPath"></param>
    /// <param name="machine_name"></param>
    bool UploadExam(string xpsPath, string xmlPath, string exam_number, string exam_name, string description, int qa_count, string qa_json, string origfilename);

    /// <summary>
    /// Updates the XPS and XML files of an existing exam. Can only be done by the uploader user who originally uploaded he exam.
    /// </summary>
    /// <param name="xpsPath"></param>
    /// <param name="xmlPath"></param>
    /// <param name="exam_id"></param>
    /// <param name="qa_count"></param>
    /// <returns></returns>
    bool UpdateExamFiles(string xpsPath, string xmlPath, int exam_id, int qa_count, string qa_json, string remarks, string origfilename);

    /// <summary>
    /// Returns the list of Master Files that are accessible to currently logged in user.
    /// </summary>
    /// <returns></returns>
    Task<AccessibleMasterFile[]> GetExamsDL();
    
    /// <summary>
    /// Returns the list of Master Files that were uploaded by the currently logged in user.
    /// </summary>
    /// <returns></returns>
    Task<MasterFile[]> GetExamsUL();

    Task<bool> UploadResult(int exam_id, string machine_name, IEnumerable<HFQResultRow> result);

    Task<bool> ExamNumberExists(string number);
  }
}