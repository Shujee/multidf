using DuplicateFinderMulti.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.Test
{
  class DialogServiceForAutomatedTesting : IDialogService
  {
    public bool AskBooleanQuestion(string msg)
    {
      throw new NotImplementedException();
    }

    public string AskStringQuestion(string msg, string default_value)
    {
      throw new NotImplementedException();
    }

    public bool? AskTernaryQuestion(string msg)
    {
      throw new NotImplementedException();
    }

    public void OpenAboutWindow()
    {
      System.Diagnostics.Debug.WriteLine("SHOWING ABOUT WINDOW");
    }

    public void OpenDiffWindow(string q1, string q2, List<string> a1, List<string> a2)
    {
      System.Diagnostics.Debug.WriteLine("SHOWING DIFF WINDOW");
    }

    public void OpenRegisterWindow()
    {
      System.Diagnostics.Debug.WriteLine("SHOWING REGISTER WINDOW");
    }

    public void OpenResultsWindow()
    {
      throw new NotImplementedException();
    }

    public bool ShowExamsListDialog()
    {
      throw new NotImplementedException();
    }

    public bool ShowLogin()
    {
      throw new NotImplementedException();
    }

    public void ShowMessage(string msg, bool isError)
    {
      System.Diagnostics.Debug.WriteLine((isError? "ERROR" : "MSG") + ": " + msg);
    }

    public string ShowOpen(string filter, string initDir = "", string title = "")
    {
      return @"F:\Office\Larry Gong\DuplicateFinder\Analysis\p20.xml";
    }

    public string[] ShowOpenMulti(string filter, string initDir = "", string title = "")
    {
      return new[] {
        @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 1 for DF.docx",
        @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 2 for DF.docx",
      };
    }

    public string ShowSave(string filter, string initDir = "", string title = "", string fileName = "")
    {
      return @"F:\Office\Larry Gong\DuplicateFinder\Analysis\p" + DateTime.Now.ToString("yyyymmdd-hhmmss") + ".xml";
    }

    public bool ShowUploadExamDialog()
    {
      throw new NotImplementedException();
    }
  }
}
