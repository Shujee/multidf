using System.Collections.Generic;

namespace MultiDF.VM
{
  public interface IDialogService : MultiDFCommon.IDialogService
  {
    void OpenRegisterWindow();
    void OpenAboutWindow();

    void OpenDiffWindow(string q1, string q2, List<string> a1, List<string> a2);
    void OpenResultsWindow();
    bool ShowUploadExamDialog(UploadExamVM vm);
  }
}