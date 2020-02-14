using DiffPlex;
using DiffPlex.DiffBuilder;
using MultiDF.VM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace MultiDF.Views
{
  public class DialogPresenter : ViewsBase.DialogPresenter, IDialogService
  {
    public void OpenRegisterWindow()
    {
      var w = new RegisterWindow();
      MakeChild(w);
      w.ShowDialog();
    }

    public void OpenDiffWindow(string q1, string q2, List<string> a1, List<string> a2)
    {
      var w = new DiffWindow();
      ((DiffVM)w.DataContext).PerformDiffCommand.Execute((q1, q2, a1, a2));

      MakeChild(w);
      w.ShowDialog();
    }

    public void OpenResultsWindow()
    {
      var w = new ResultsWindow();

      MakeChild(w);
      w.ShowDialog();
    }

    public bool ShowUploadExamDialog(UploadExamVM vm)
    {
      var w = new UploadExamDialog(vm);
      MakeChild(w); //Show this dialog as child of Microsoft Word window.
      var Result = w.ShowDialog().Value;
      return Result;
    }

    public void OpenAboutWindow()
    {
      var w = new AboutWindow();
      MakeChild(w);
      w.ShowDialog();
    }
  }
}