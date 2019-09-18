namespace DuplicateFinderMulti.VM
{
  public interface IDialogService
  {
    void ShowMessage(string msg, bool isError);
    string AskStringQuestion(string msg, string default_value);
    string ShowOpen(string filter, string initDir = "", string title = "");
    string[] ShowOpenMulti(string filter, string initDir = "", string title = "");
    string ShowSave(string filter, string initDir = "", string title = "", string fileName = "");

    void OpenRegisterWindow();
    void OpenDiffWindow(string text1, string text2);
    void OpenAboutWindow();
  }
}