using Common;
using System;
using System.Threading.Tasks;

namespace HFQAppTest
{
  internal class TestDialogService : HFQOVM.IDialogService
  {
    public bool AskBooleanQuestion(string msg)
    {
      Console.WriteLine(msg);
      return true;
    }

    public string AskStringQuestion(string msg, string default_value)
    {
      Console.WriteLine(msg);
      return "asdf";
    }

    public bool? AskTernaryQuestion(string msg)
    {
      Console.WriteLine(msg);
      return true;
    }

    public void OpenAboutWindow()
    {
      Console.WriteLine("Showing about window");
    }

    public void OpenRegisterWindow()
    {
      Console.WriteLine("Showing register window");
    }

    public Task<AccessibleMasterFile> ShowExamsListDialog()
    {
      Console.WriteLine("Showing exams list dialog");
      return Task.FromResult(new AccessibleMasterFile() { access_id = 324 });
    }

    public bool ShowLogin()
    {
      Console.WriteLine("Showing login window");
      return true;
    }

    public void ShowMessage(string msg, bool isError)
    {
      Console.WriteLine(msg + ", Error = " + isError.ToString());
    }

    public void ShowMessage(Exception ee)
    {
      Console.WriteLine("Exception: " + ee.Message);
    }

    public string ShowOpen(string filter, string initDir = "", string title = "")
    {
      Console.WriteLine("Showing open file dialog");
      return "c:\\somefile.txt";
    }

    public string[] ShowOpenMulti(string filter, string initDir = "", string title = "")
    {
      Console.WriteLine("Showing open file (multiple) dialog");
      return new[] { "c:\\somefile.txt", "c:\\somefile2.txt" };
    }

    public string ShowSave(string filter, string initDir = "", string title = "", string fileName = "")
    {
      Console.WriteLine("Showing save file dialog");
      return "c:\\somefile.txt";
    }
  }
}