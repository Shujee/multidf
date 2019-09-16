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
    public const string FILTER_IMAGE_FILES_ALL_FILES = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif|All Files (*.*)|*.*";

    private static OpenFileDialog dlgOpen = new OpenFileDialog();
    private static SaveFileDialog dlgSave = new SaveFileDialog();

    public void ShowMessage(string msg, bool isError)
    {
        MessageBox.Show(msg, "DuplicateFinderMulti Word Add-in", MessageBoxButton.OK, isError? MessageBoxImage.Error : MessageBoxImage.Information);
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
      var helper = new WindowInteropHelper(w) { Owner = HWND };
    }

    public void OpenAboutWindow()
    {
      var w = new AboutWindow();
      MakeChild(w);
      w.ShowDialog();
    }
  }
}