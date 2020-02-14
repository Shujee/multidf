namespace Common
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

    bool ShowLogin();
  }
}