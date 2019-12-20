using System.Collections.Generic;
using System.Threading.Tasks;

namespace HFQOModel
{
  public interface IDataService
  {
    /// <summary>
    /// Tries to login to the server using specified email address and password. Returns true for this session if successful. Authentication token returned
    /// by the server is saved for subsequent server calls.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> Login(string email, string password);

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
    /// Creates a new exam on the server. Can only be done by the admin.
    /// </summary>
    /// <param name="xpsPath"></param>
    /// <param name="xmlPath"></param>
    /// <param name="machine_name"></param>
    bool UploadExam(string xpsPath, string xmlPath, string exam_name, int qa_count);

    /// <summary>
    /// Returns the list of Master Files that are accessible to currently logged in user.
    /// </summary>
    /// <returns></returns>
    Task<Dictionary<string, string>> GetExams();
  }
}