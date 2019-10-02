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
    public string AskStringQuestion(string msg, string default_value)
    {
      throw new NotImplementedException();
    }

    public void OpenAboutWindow()
    {
      System.Diagnostics.Debug.WriteLine("SHOWING ABOUT WINDOW");
    }

    public void OpenDiffWindow(string text1, string text2)
    {
      System.Diagnostics.Debug.WriteLine("SHOWING DIFF WINDOW");
    }

    public void OpenRegisterWindow()
    {
      System.Diagnostics.Debug.WriteLine("SHOWING REGISTER WINDOW");
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
  }
}
