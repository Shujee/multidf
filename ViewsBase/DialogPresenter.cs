using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using Common;
using GalaSoft.MvvmLight.Threading;

namespace ViewsBase
{
  public class DialogPresenter : IDialogService
  {
    private readonly string _Title = "";

    public Window Owner { get; set; }

    private readonly static OpenFileDialog dlgOpen = new OpenFileDialog();
    private readonly static SaveFileDialog dlgSave = new SaveFileDialog();

    public DialogPresenter(Window owner, string title)
    {
      Owner = owner;
      _Title = title;
    }

    public void ShowMessage(string msg, bool isError)
    {
      DispatcherHelper.UIDispatcher.Invoke(() =>
      {
        if (Owner == null)
          MessageBox.Show(msg, _Title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
        else
          MessageBox.Show(Owner, msg, _Title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
      });
    }

    public void ShowMessage(Exception ee)
    {
      string Msg = ee.Message;

      if (ee.Data.Count > 0)
      {
        Msg += Environment.NewLine + Environment.NewLine;

        foreach (System.Collections.DictionaryEntry err in ee.Data)
          Msg = err.Key + ": " + err.Value;
      }

      DispatcherHelper.UIDispatcher.Invoke(() =>
      {
        if (Owner == null)
          MessageBox.Show(Msg, _Title, MessageBoxButton.OK, MessageBoxImage.Error);
        else
          MessageBox.Show(Owner, Msg, _Title, MessageBoxButton.OK, MessageBoxImage.Error);
      });
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
    /// Shows message prompt with Yes, No and Cancel options. Used in situations like "Do you want to save before closing?".
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool? AskTernaryQuestion(string msg)
    {
      var Res = MessageBox.Show(Owner, msg, _Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

      if (Res == MessageBoxResult.Yes)
        return true;
      else if (Res == MessageBoxResult.No)
        return false;
      else
        return null;
    }

    public bool AskBooleanQuestion(string msg)
    {
      var Res = MessageBox.Show(Owner, msg, _Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
      return Res == MessageBoxResult.Yes;
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

      if (dlgOpen.ShowDialog(Owner).Value)
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

      if (dlgOpen.ShowDialog(Owner).Value)
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

      if (dlgSave.ShowDialog(Owner).Value)
        return dlgSave.FileName;
      else
        return null;
    }

    /// <summary>
    /// Shows HFQO Login dialog.
    /// </summary>
    /// <returns>true if User clicks OK button, otherwise false.</returns>
    public bool ShowLogin()
    {
      var w = new LoginWindow();
      MakeChild(w); //Show this dialog as child of Microsoft Word window.
      var Result = w.ShowDialog().Value;
      return Result;
    }

    public void OpenRegisterWindow()
    {
      var w = new RegisterWindow();
      MakeChild(w);
      w.ShowDialog();
    }

    public void OpenAboutWindow()
    {
      var w = new AboutWindow();
      MakeChild(w);
      w.ShowDialog();
    }

    /// <summary>
    /// Sets Word window as parent of the specified window.
    /// </summary>
    /// <param name="w"></param>
    protected static void MakeChild(System.Windows.Window w)
    {
      IntPtr HWND = Process.GetCurrentProcess().MainWindowHandle;
      _ = new WindowInteropHelper(w) { Owner = HWND };
    }
  }
}