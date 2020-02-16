using GalaSoft.MvvmLight.Ioc;
using Microsoft.Office.Interop.Word;
using MultiDF.Views;
using MultiDF.VM;
using System;
using System.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace MultiDF
{
  public partial class ThisAddIn : IWordService
  {
    private readonly DialogPresenter DLG = new DialogPresenter("MultiDF Word Add-in");

    private void ThisAddIn_Startup(object sender, System.EventArgs e)
    {
      //Services injection
      SimpleIoc.Default.Register<IWordService>(() => this);

      SimpleIoc.Default.Register<Common.IDialogService>(() => DLG);
      SimpleIoc.Default.Register<MultiDF.VM.IDialogService>(() => DLG);

      var ExpiryDate = ViewModelLocator.Register.ExpiryDate;

      if (ExpiryDate != null)
      {
        var ExpiryDays = ExpiryDate.Value.Subtract(System.DateTime.Now).TotalDays;
        if (ExpiryDays > 0 && ExpiryDays < 7)
        {
          ViewModelLocator.DialogService.ShowMessage($"MultiDF will expire in {(int)Math.Ceiling(ExpiryDays)} day(s). You should get a new license key if you want to continue using the add-in.", false);
        }
      }

      try
      {
        //These lines make sure WPF application engine has started before we make any calls into it.
        if (System.Windows.Application.Current == null)
          new System.Windows.Application();

        System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

        GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();

        ((ApplicationEvents4_Event)Application).NewDocument += Application_NewDocument;
        Application.DocumentOpen += Application_DocumentOpen;
        Application.DocumentChange += Application_DocumentChange;

        AddAllTaskPanes();
      }
      catch (Exception ee)
      {
        LogException(ee);
      }
    }

    private void Application_DocumentOpen(Word.Document Doc)
    {
      try
      {
        //Remove all our TaskPanes before quitting.
        RemoveOrphanedTaskPanes();
        if (IsPaneVisible && this.Application.ShowWindowsInTaskbar)
          AddTaskPane(Doc);
      }
      catch (Exception ee)
      {
        LogException(ee);
      }
    }

    private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject.IsDirty)
      {
        if (ViewModelLocator.DialogService.AskBooleanQuestion("Save changes to active MultiDF project?"))
          ViewModelLocator.Main.SelectedProject.SaveCommand.Execute(null);
      }

      RemoveAllTaskPanes();

      Application.DocumentOpen -= Application_DocumentOpen;
    }

    private static void LogException(Exception e, string additionalInfo = null, bool suppressUI = false)
    {
      string LogFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiDF", "ErrorLog.txt");

      if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(LogFilePath)))
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(LogFilePath));

      using (var sw = new System.IO.StreamWriter(LogFilePath))
      {
        sw.WriteLine(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
        sw.WriteLine(DateTime.Now.ToString("--------------------"));
        sw.WriteLine("Exception: " + e.Message);

        if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
          sw.WriteLine("Inner Exception: " + e.InnerException.Message);

        if (!string.IsNullOrEmpty(e.StackTrace))
          sw.WriteLine("Stack Trace: " + e.StackTrace);

        if (!string.IsNullOrEmpty(additionalInfo))
          sw.WriteLine("Additional Info: " + additionalInfo);
      }

      if (!suppressUI)
      {
        MessageBox.Show("The following error occurred: " + e.Message + Environment.NewLine + Environment.NewLine +
                        "Detailed information about the error has been added to error log.", System.Windows.Forms.Application.ProductName,
                        MessageBoxButton.OK, MessageBoxImage.Error);

        if (e.InnerException != null)
        {
          MessageBox.Show(e.InnerException.Message);

          if (e.InnerException.InnerException != null)
          {
            MessageBox.Show(e.InnerException.InnerException.Message);

            if (e.InnerException.InnerException.InnerException != null)
            {
              MessageBox.Show(e.InnerException.InnerException.InnerException.Message);

              if (e.InnerException.InnerException.InnerException.InnerException != null)
              {
                MessageBox.Show(e.InnerException.InnerException.InnerException.InnerException.Message);
              }
            }
          }
        }
      }
    }

    #region VSTO generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup()
    {
      this.Startup += new System.EventHandler(ThisAddIn_Startup);
      this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
    }
    #endregion
  }
}
