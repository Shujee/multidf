using DiffPlex;
using DiffPlex.DiffBuilder;
using DuplicateFinderMulti.VM;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace DuplicateFinderMulti.Views
{
  public class DialogPresenter : IDialogService
  {
    private readonly static OpenFileDialog dlgOpen = new OpenFileDialog();
    private readonly static SaveFileDialog dlgSave = new SaveFileDialog();

    public void ShowMessage(string msg, bool isError)
    {
        MessageBox.Show(msg, "DuplicateFinderMulti Word Add-in", MessageBoxButton.OK, isError? MessageBoxImage.Error : MessageBoxImage.Information);
    }

    /// <summary>
    /// Prompts user for a single value input. First parameter specifies the message to be displayed in the dialog 
    /// and the second string specifies the default value to be displayed in the input box.
    /// </summary>
    /// <param name="m"></param>
    public string AskStringQuestion(string msg, string default_value)
    {
      string Result = null;

      InputBox w = new InputBox();
      MakeChild(w);
      if (w.ShowDialog(msg, default_value).Value)
        Result = w.Value;

      return Result;
    }

    /// <summary>
    /// Displays Open dialog. User can specify file filter, initial directory and dialog title. Returns full path of the selected file if
    /// user clicks Open button. Returns null if user clicks Cancel button.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="initDir"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public string ShowOpen(string filter, string initDir = "", string title = "")
    {
      if (!string.IsNullOrEmpty(title))
        dlgOpen.Title = title;
      else
        dlgOpen.Title = "Open";

      dlgOpen.Multiselect = false;
      dlgOpen.Filter = filter;
      if (!string.IsNullOrEmpty(initDir))
        dlgOpen.InitialDirectory = initDir;

      if (dlgOpen.ShowDialog().Value)
        return dlgOpen.FileName;
      else
        return null;
    }

    /// <summary>
    /// Displays Open dialog and allows multiple files selection. User can specify file filter, initial directory and dialog title. Returns full paths of the selected files if
    /// user clicks Open button. Returns null if user clicks Cancel button.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="initDir"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public string[] ShowOpenMulti(string filter, string initDir = "", string title = "")
    {
      if (!string.IsNullOrEmpty(title))
        dlgOpen.Title = title;
      else
        dlgOpen.Title = "Open";

      dlgOpen.Multiselect = true;
      dlgOpen.Filter = filter;
      if (!string.IsNullOrEmpty(initDir))
        dlgOpen.InitialDirectory = initDir;

      if (dlgOpen.ShowDialog().Value)
        return dlgOpen.FileNames;
      else
        return null;
    }

    /// <summary>
    /// Displays Save dialog. User can specify file filter, initial directory and dialog title. Returns full path of the selected file if
    /// user clicks Save button. Returns null if user clicks Cancel button.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="initDir"></param>
    /// <param name="title"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string ShowSave(string filter, string initDir = "", string title = "", string fileName = "")
    {
      if (!string.IsNullOrEmpty(title))
        dlgSave.Title = title;
      else
        dlgSave.Title = "Save";

      if (!string.IsNullOrEmpty(fileName))
        dlgSave.FileName = fileName;
      else
        dlgSave.FileName = "";

      dlgSave.Filter = filter;
      if (!string.IsNullOrEmpty(initDir))
        dlgSave.InitialDirectory = initDir;

      if (dlgSave.ShowDialog().Value)
        return dlgSave.FileName;
      else
        return null;
    }

    public void OpenRegisterWindow()
    {
      var w = new RegisterWindow();
      MakeChild(w);
      w.ShowDialog();
    }

    public void OpenDiffWindow(string text1, string text2)
    {
      var w = new DiffWindow();
      ((DiffVM)w.DataContext).PerformDiffCommand.Execute((text1, text2));

      MakeChild(w);
      w.ShowDialog();
    }

    /// <summary>
    /// Sets Word window as parent of the specified window.
    /// </summary>
    /// <param name="w"></param>
    private static void MakeChild(System.Windows.Window w)
    {
      IntPtr HWND = Process.GetCurrentProcess().MainWindowHandle;
      _ = new WindowInteropHelper(w) { Owner = HWND };
    }

    public void OpenAboutWindow()
    {
      var w = new AboutWindow();
      MakeChild(w);
      w.ShowDialog();
    }
  }
}