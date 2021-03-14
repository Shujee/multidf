using Microsoft.Office.Tools;
using System.Linq;
using Office = Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;

namespace MultiDF
{
  /// <summary>
  /// This file contains code that manages/syncs task panes across multiple document windows
  /// </summary>
  partial class ThisAddIn
  {
    public bool IsPaneVisible { get; set; } = false;

    // Add a custom task pane to all open document windows
    public void AddAllTaskPanes()
    {
      // First check if there are any open documents.
      if (Application.Documents.Count > 0)
      {
        // If Show all windows in the Taskbar is selected, then each open document has its own window.  
        // If Show all windows in the Taskbar is not selected, then Word displays each open document in the same window.  
        if (this.Application.ShowWindowsInTaskbar)
        {
          // Loop through each open document window
          foreach (Word.Document _doc in this.Application.Documents)
            AddTaskPane(_doc); // Pass this document as a parameter to AddCustomTaskPane
        }
        else
        {
          if (!IsPaneVisible)
            AddTaskPane(this.Application.ActiveDocument);
        }

        IsPaneVisible = true;
      }
    }

    // Add a custom task pane consisting of a AudioPlayer control 
    public void AddTaskPane(Word.Document doc)
    {
      if (!VM.ViewModelLocator.Register.IsRegistered)
        return;

      try
      {
        if (this.CustomTaskPanes.Any(ctp => WordHelper.GetWindowSafe(ctp) == doc.ActiveWindow))
          return;
        
        // Create a new custom task pane and add it to the collection of custom task panes belonging to this add-in.
        // The first two arguments of the Add method specify a control to add to the custom task pane and the title to display on the task pane. 
        // The third argument, which is optional, specifies the parent window for the custom task pane. 
        var ucAP = new MultiDFPaneUC();
        var NewTaskPane = this.CustomTaskPanes.Add(ucAP, $"MultiDF (ver: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()})", doc.ActiveWindow);

        NewTaskPane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionLeft;
        NewTaskPane.Width = 575;

        // Display the custom task pane.
        NewTaskPane.Visible = true;
      }
      catch (System.Exception ee)
      {
        LogException(ee);
      }
    }


    // Add a custom task pane consisting of a AudioPlayer control 
    public void AddSeqErrorsTaskPane(Word.Document doc)
    {
      if (!VM.ViewModelLocator.Register.IsRegistered)
        return;

      try
      {
        // Create a new custom task pane and add it to the collection of custom task panes belonging to this add-in.
        // The first two arguments of the Add method specify a control to add to the custom task pane and the title to display on the task pane. 
        // The third argument, which is optional, specifies the parent window for the custom task pane. 
        var ucAP = new SeqErrorsPaneUC(doc);
        var NewTaskPane = this.CustomTaskPanes.Add(ucAP, $"MultiDF Sequence Errors", doc.ActiveWindow);

        NewTaskPane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
        NewTaskPane.Width = 300;

        // Display the custom task pane.
        NewTaskPane.Visible = true;
      }
      catch (System.Exception ee)
      {
        LogException(ee);
      }
    }

    public void RemoveAllTaskPanes()
    {
      // First check if there are any open documents.
      if (Application.Documents.Count > 0)
      {
        try
        {
          // Loop through each custom task pane belonging to the add-in
          for (int i = this.CustomTaskPanes.Count; i > 0; i--)
          {
            var ctp = this.CustomTaskPanes[i - 1];

            if (ctp.Title.StartsWith("MultiDF"))
              this.CustomTaskPanes.RemoveAt(i - 1); // If this is our task pane, remove it
          }

        }
        catch { }

        IsPaneVisible = false;
      }
    }

    // This code is correct for its purpose, but it is not used here.
    // If Application.BeforeDocumentClose or Document.Close events
    // occurred after the document was closed, this code could be called
    // to remove task pane from the document window. However,
    // user can cancel those events. To avoid removing the task pane 
    // from a window that user did not close, RemoveOrphanedTaskPanes 
    // method is called from Application_DocumentChange event handler. 
    public void RemoveTaskPane(Word.Document doc)
    {
      // Loop through each custom task pane belonging to the add-in
      foreach (Microsoft.Office.Tools.CustomTaskPane _ctp in this.CustomTaskPanes)
      {
        try
        {
          // Get a reference to the window hosting the task pane
          var ctpWindow = (Word.Window)WordHelper.GetWindowSafe(_ctp);
          if (_ctp.Title.StartsWith("MultiDF") && ctpWindow == doc.ActiveWindow)
          {
            // If the title of this task pane is Select a date 
            // and the currently active window is hosting it, remove it
            this.CustomTaskPanes.Remove(_ctp);
            break;
          }
        }
        catch (System.Exception ee)
        {
          LogException(ee);
        }
      }

      IsPaneVisible = false;
    }

    private void RemoveOrphanedTaskPanes()
    {
      // Loop through each custom task pane belonging to the add-in
      for (int i = this.CustomTaskPanes.Count; i > 0; i--)
      {
        try
        {
          CustomTaskPane ctp = this.CustomTaskPanes[i - 1];
          if (this.CustomTaskPanes.Contains(ctp) && WordHelper.GetWindowSafe(ctp) == null)
          {
            this.CustomTaskPanes.Remove(ctp);
          }
        }
        catch
        {
        }
      }
    }

    private void Application_NewDocument(Word.Document Doc)
    {
      try
      {
        if (!IsPaneVisible && this.Application.ShowWindowsInTaskbar && !this.CustomTaskPanes.Any(cp => WordHelper.GetWindowSafe(cp) == Doc.ActiveWindow))
          AddTaskPane(Doc);
      }
      catch (System.Exception ee)
      {
        LogException(ee);
      }
    }

    private void Application_DocumentChange()
    {
      RemoveOrphanedTaskPanes();
    }
  }
}
