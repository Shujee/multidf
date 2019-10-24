﻿using DuplicateFinderMulti.Views;
using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace DuplicateFinderMulti
{
  public partial class ThisAddIn : IWordService
  {
    private void ThisAddIn_Startup(object sender, System.EventArgs e)
    {
      //Services injection
      SimpleIoc.Default.Register<IWordService>(() => this);
      SimpleIoc.Default.Register<IDialogService, DialogPresenter>();

      var ExpiryDate = ViewModelLocator.Register.ExpiryDate;

      if (ExpiryDate != null)
      {
        var ExpiryDays = ExpiryDate.Value.Subtract(System.DateTime.Now).TotalDays;
        if (ExpiryDays > 0 && ExpiryDays < 7)
        {
          ViewModelLocator.DialogService.ShowMessage($"DuplicateFinderMulti will expire in {(int)Math.Ceiling(ExpiryDays)} day(s). You should get a new license key if you want to continue using the add-in.", false);
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
        if (ViewModelLocator.DialogService.AskBooleanQuestion("Save changes to active Multi-DF project?"))
          ViewModelLocator.Main.SelectedProject.SaveCommand.Execute(null);
      }

      RemoveAllTaskPanes();

      Application.DocumentOpen -= Application_DocumentOpen;
    }

    private static void LogException(Exception e, string additionalInfo = null, bool suppressUI = false)
    {
      string LogFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DuplicateFinderMulti", "ErrorLog.txt");

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

    /// <summary>
    /// Selects specified range in the active Word document.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    //public void SelectRange(int start, int end)
    //{
    //  if (this.Application.ActiveDocument != null)
    //  {
    //    this.Application.ActiveDocument.Range(start, end).Select();
    //  }
    //}

    /// <summary>
    /// Returns text content of the specified range from active Word document.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    //public string GetRangeText(int start, int end)
    //{
    //  if (this.Application.ActiveDocument != null)
    //  {
    //    return this.Application.ActiveDocument.Range(start, end).Text;
    //  }
    //  else
    //    return null;
    //}

    /// <summary>
    /// Returns the entire text of active Word document.
    /// </summary>
    /// <returns></returns>
    //public string GetActiveDocumentText()
    //{
    //  return this.Application.ActiveDocument?.Content?.Text;
    //}

    /// <summary>
    /// Converts all paragraphs of the active Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to its text.
    /// </summary>
    /// <returns></returns>
    //public IEnumerable<WordParagraph> GetActiveDocumentParagraphs()
    //{
    //  if (this.Application.ActiveDocument != null)
    //  {
    //    foreach (Word.Paragraph p in this.Application.ActiveDocument.Paragraphs)
    //    {
    //      yield return new WordParagraph()
    //      {
    //        Start = p.Range.Start,
    //        End = p.Range.End,
    //        Text = p.Range.Text
    //      };
    //    }
    //  }
    //  else
    //    yield return null;
    //}

    /// <summary>
    /// Converts all paragraphs in the specified Word document to a List of WordParagraph objects that store the start and end position of each paragraph
    /// in addition to the paragraph text.
    /// </summary>
    /// <param name="docPath"></param>
    /// <returns></returns>
    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback)
    {
      if(!string.IsNullOrEmpty(docPath) && System.IO.File.Exists(docPath))
      {
        List<WordParagraph> Result = new List<WordParagraph>();
        Document Doc;
        int ParaCount;

        bool AlreadyOpen = false;
        Doc = this.Application.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

        if (Doc == null)
        {
          try
          {
            Doc = this.Application.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: false);
          }
          catch (Exception ee)
          {
            ViewModelLocator.DialogService.ShowMessage("The following error occurred while trying to open specified document: " + ee.Message, true);
            return null;
          }
        }
        else
          AlreadyOpen = true;

        int i = 0;
        ParaCount = Doc.Paragraphs.Count;
        foreach (Word.Paragraph p in Doc.Paragraphs)
        {
          var PType = ParagraphType.Text;

          if (p.Range.Tables.Count > 0)
          {
            try
            {
              if (p.Range.Tables[1].ApplyStyleHeadingRows && p.Range.Rows.First.Index == 1)
                PType = ParagraphType.TableHeader;
              else
                PType = ParagraphType.TableRow;
            }
            catch //tables with vertically merged rows can throw exception when trying to access p.Range.Rows.First.Index 
            {
              PType = ParagraphType.TableRow;
            }
          }
          else if (p.Range.ListFormat.ListType == WdListType.wdListSimpleNumbering || p.Range.ListFormat.ListType == WdListType.wdListOutlineNumbering)
            PType = ParagraphType.NumberedList;
          

          Result.Add(new WordParagraph(p.Range.Text, p.Range.Start, p.Range.End, PType));

          progressCallback?.Invoke(i++, ParaCount);

          if (token.IsCancellationRequested)
            break;
        }

        if (!AlreadyOpen)
        {
          System.Threading.Tasks.Task.Run(() => Doc.Close(SaveChanges: false));
        }

        progressCallback?.Invoke(ParaCount, ParaCount);

        return Result;
      }
      else
        return null;
    }

    /// <summary>
    /// Returns the number of paragraphs in the active Word document.
    /// </summary>
    /// <returns></returns>
    public int? GetActiveDocumentParagraphsCount()
    {
      return this.Application.ActiveDocument?.Paragraphs?.Count;
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      var Doc = this.Application.Documents.Cast<Document>().FirstOrDefault(d => d.FullName == docPath);

      if (Doc == null)
        Doc = this.Application.Documents.Open(docPath);
      else
        Doc.Activate();

      if (start != null)
      {
        this.Application.Selection.Start = start.Value;

        if (end != null)
          this.Application.Selection.End = end.Value;

        this.Application.ActiveWindow.ScrollIntoView(this.Application.Selection.Range);
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
