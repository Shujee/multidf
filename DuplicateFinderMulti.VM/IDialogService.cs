namespace DuplicateFinderMulti.VM
{
  public interface IDialogService
  {
    void ShowMessage(string msg, bool isError);

    void OpenRegisterWindow();
    void OpenDiffWindow(string text1, string text2);
    void OpenAboutWindow();
  }
}