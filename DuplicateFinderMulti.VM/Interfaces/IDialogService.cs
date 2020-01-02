using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  public interface IDialogService
  {
    void ShowMessage(string msg, bool isError);
    string AskStringQuestion(string msg, string default_value);
    bool AskBooleanQuestion(string msg);
    bool? AskTernaryQuestion(string msg);
    string ShowOpen(string filter, string initDir = "", string title = "");
    string[] ShowOpenMulti(string filter, string initDir = "", string title = "");
    string ShowSave(string filter, string initDir = "", string title = "", string fileName = "");

    void OpenRegisterWindow();
    void OpenDiffWindow(string q1, string q2, List<string> a1, List<string> a2);
    void OpenResultsWindow();
    void OpenAboutWindow();

    bool ShowLogin();
    bool ShowExamsListDialog();
    bool ShowUploadExamDialog();
  }
}