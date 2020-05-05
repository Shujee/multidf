using MultiDF.VM;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System.Linq;

namespace MultiDF
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Scope = "module", Justification = "Built-in arguments 'sender' and 'e' are not needed in the event handlers below")]
  public partial class MultiDFRibbon
  {
    /// <summary>
    /// Need to keep a reference to current Project object so that when a new Project is opened, we
    /// could detatch previous object's event listener.
    /// </summary>
    private Project _SelectedProject;

    private void MultiDFRibbon_Load(object sender, RibbonUIEventArgs e)
    {
      ViewModelLocator.Auth.PropertyChanged += Auth_PropertyChanged;

      ViewModelLocator.Main.NewCommand.CanExecuteChanged += (sender2, e2) => btnNewProject.Enabled = ViewModelLocator.Main.NewCommand.CanExecute(null);
      ViewModelLocator.Main.OpenCommand.CanExecuteChanged += (sender2, e2) => btnOpenProject.Enabled = ViewModelLocator.Main.OpenCommand.CanExecute(null);
      ViewModelLocator.Main.PropertyChanged += Main_PropertyChanged;

      var ExpiryDate = VM.ViewModelLocator.Register.ExpiryDate;

      if (ExpiryDate == null)
      {
        btnShowHidePane.Visible = false;
        btnRegister.Visible = true;
      }
      else
      {
        var IsExpired = (ExpiryDate.Value.Subtract(System.DateTime.Now).TotalDays < 7);
        btnShowHidePane.Visible = !IsExpired;
      }

      btnLogin.Enabled = !(ViewModelLocator.Auth.IsLoggedIn || ViewModelLocator.Auth.IsCommunicating);
      btnLogout.Enabled = ViewModelLocator.Auth.IsLoggedIn;

      //Update Ribbon Buttons upon startup
      btnNewProject.Enabled = ViewModelLocator.Main.NewCommand.CanExecute(null);
      btnOpenProject.Enabled = ViewModelLocator.Main.OpenCommand.CanExecute(null);

      _SelectedProject = ViewModelLocator.Main.SelectedProject;

      if (_SelectedProject != null)
      {
        btnSaveProject.Enabled = _SelectedProject.SaveCommand.CanExecute(null);
        btnExport.Enabled = _SelectedProject.ExportCommand.CanExecute(null);
        btnAddSourceDoc.Enabled = _SelectedProject.AddDocsCommand.CanExecute(null);
        btnRemoveSourceDoc.Enabled = _SelectedProject.RemoveSelectedDocCommand.CanExecute(null);
        btnUpdateQAs.Enabled = _SelectedProject.UpdateQAsCommand.CanExecute(null);
        btnCheckSyncWithSource.Enabled = _SelectedProject.CheckSyncWithSourceCommand.CanExecute(null);
        btnProcess.Enabled = _SelectedProject.ProcessCommand.CanExecute(null);
        btnStopProcess.Enabled = _SelectedProject.AbortProcessCommand.CanExecute(null);
        btnOpenResultsWindow.Enabled = _SelectedProject.OpenResultsWindowCommand.CanExecute(null);
        btnExportResults.Enabled = _SelectedProject.ExportResultsCommand.CanExecute(null);
        btnMergeAsDOCX.Enabled = _SelectedProject.MergeAsDOCXCommand.CanExecute(null);
        btnMergeAsPDF.Enabled = _SelectedProject.MergeAsPDFCommand.CanExecute(null);
        btnUpload.Enabled = _SelectedProject.UploadExamCommand.CanExecute(null);
      }
      else
      {
        btnSaveProject.Enabled = false;
        btnExport.Enabled = false;
        btnAddSourceDoc.Enabled = false;
        btnRemoveSourceDoc.Enabled = false;
        btnUpdateQAs.Enabled = false;
        btnCheckSyncWithSource.Enabled = false;
        btnProcess.Enabled = false;
        btnStopProcess.Enabled = false;
        btnOpenResultsWindow.Enabled = false;
        btnExportResults.Enabled = false;
        btnMergeAsDOCX.Enabled = false;
        btnMergeAsPDF.Enabled = false;
        btnUpload.Enabled = false;
      }

      PopulateMRUs();
      ViewModelLocator.Main.Init();
    }

    private void PopulateMRUs()
    {
      if (ViewModelLocator.Main.MRU != null && ViewModelLocator.Main.MRU.Count > 0)
      {
        foreach (var mru in ViewModelLocator.Main.MRU)
        {
          var DDI = Factory.CreateRibbonButton();

          DDI.Label = System.IO.Path.GetFileNameWithoutExtension(mru); //show only file name in the menu text
          DDI.Tag = mru; //store full project file path in Tag. This will be used later in Click event to pass project file path to OpenCommand.
          DDI.OfficeImageId = "PageOrientationPortrait";
          DDI.Click += DDI_Click;

          btnOpenProject.Items.Add(DDI);
        }
      }

      var Sep = Factory.CreateRibbonSeparator();
      btnOpenProject.Items.Add(Sep);

      var BrowseDDI = Factory.CreateRibbonButton();

      BrowseDDI.Label = "Browse...";
      BrowseDDI.Tag = "BrowseButton";
      BrowseDDI.OfficeImageId = "FileOpen";
      BrowseDDI.Click += DDI_Click;

      btnOpenProject.Items.Add(BrowseDDI);
    }

    private void DDI_Click(object sender, RibbonControlEventArgs e)
    {
      //Control Tag property holds full path of the Project file.
      if (sender is RibbonButton btn)
      {
        if ((string)btn.Tag == "BrowseButton")
          ViewModelLocator.Main.OpenCommand.Execute(null);
        else
          ViewModelLocator.Main.OpenCommand.Execute(btn.Tag);
      }
    }

    private void Main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MainVM.SelectedProject))
      {
        //Detatch previous Project's event listener
        if (_SelectedProject != null)
        {
          _SelectedProject.SaveCommand.CanExecuteChanged -= (sender2, e2) => btnSaveProject.Enabled = _SelectedProject.SaveCommand.CanExecute(null);
          _SelectedProject.ExportCommand.CanExecuteChanged -= (sender2, e2) => btnExport.Enabled = _SelectedProject.ExportCommand.CanExecute(null);
          _SelectedProject.AddDocsCommand.CanExecuteChanged -= (sender2, e2) => btnAddSourceDoc.Enabled = _SelectedProject.AddDocsCommand.CanExecute(null);
          _SelectedProject.RemoveSelectedDocCommand.CanExecuteChanged -= (sender2, e2) => btnRemoveSourceDoc.Enabled = _SelectedProject.RemoveSelectedDocCommand.CanExecute(null);
          _SelectedProject.UpdateQAsCommand.CanExecuteChanged -= (sender2, e2) => btnUpdateQAs.Enabled = _SelectedProject.UpdateQAsCommand.CanExecute(null);
          _SelectedProject.CheckSyncWithSourceCommand.CanExecuteChanged -= (sender2, e2) => btnCheckSyncWithSource.Enabled = _SelectedProject.CheckSyncWithSourceCommand.CanExecute(null);
          _SelectedProject.ProcessCommand.CanExecuteChanged -= (sender2, e2) => btnProcess.Enabled = _SelectedProject.ProcessCommand.CanExecute(null);
          _SelectedProject.AbortProcessCommand.CanExecuteChanged -= (sender2, e2) => btnStopProcess.Enabled = _SelectedProject.AbortProcessCommand.CanExecute(null);
          _SelectedProject.OpenResultsWindowCommand.CanExecuteChanged -= (sender2, e2) => btnOpenResultsWindow.Enabled = _SelectedProject.OpenResultsWindowCommand.CanExecute(null);
          _SelectedProject.ExportResultsCommand.CanExecuteChanged -= (sender2, e2) => btnExportResults.Enabled = _SelectedProject.ExportResultsCommand.CanExecute(null);
          _SelectedProject.MergeAsDOCXCommand.CanExecuteChanged -= (sender2, e2) => btnMergeAsDOCX.Enabled = _SelectedProject.MergeAsDOCXCommand.CanExecute(null);
          _SelectedProject.MergeAsPDFCommand.CanExecuteChanged -= (sender2, e2) => btnMergeAsPDF.Enabled = _SelectedProject.MergeAsPDFCommand.CanExecute(null);
          _SelectedProject.UploadExamCommand.CanExecuteChanged -= (sender2, e2) => btnUpload.Enabled = _SelectedProject.UploadExamCommand.CanExecute(null);
        }

        _SelectedProject = ViewModelLocator.Main.SelectedProject;

        //Now attach new Project's event listener
        if (ViewModelLocator.Main.SelectedProject != null)
        {
          _SelectedProject.SaveCommand.CanExecuteChanged += (sender2, e2) => btnSaveProject.Enabled = _SelectedProject.SaveCommand.CanExecute(null);
          _SelectedProject.ExportCommand.CanExecuteChanged += (sender2, e2) => btnExport.Enabled = _SelectedProject.ExportCommand.CanExecute(null);
          _SelectedProject.AddDocsCommand.CanExecuteChanged += (sender2, e2) => btnAddSourceDoc.Enabled = _SelectedProject.AddDocsCommand.CanExecute(null);
          _SelectedProject.RemoveSelectedDocCommand.CanExecuteChanged += (sender2, e2) => btnRemoveSourceDoc.Enabled = _SelectedProject.RemoveSelectedDocCommand.CanExecute(null);
          _SelectedProject.UpdateQAsCommand.CanExecuteChanged += (sender2, e2) => btnUpdateQAs.Enabled = _SelectedProject.UpdateQAsCommand.CanExecute(null);
          _SelectedProject.CheckSyncWithSourceCommand.CanExecuteChanged += (sender2, e2) => btnCheckSyncWithSource.Enabled = _SelectedProject.CheckSyncWithSourceCommand.CanExecute(null);
          _SelectedProject.ProcessCommand.CanExecuteChanged += (sender2, e2) => btnProcess.Enabled = _SelectedProject.ProcessCommand.CanExecute(null);
          _SelectedProject.AbortProcessCommand.CanExecuteChanged += (sender2, e2) => btnStopProcess.Enabled = _SelectedProject.AbortProcessCommand.CanExecute(null);
          _SelectedProject.OpenResultsWindowCommand.CanExecuteChanged += (sender2, e2) => btnOpenResultsWindow.Enabled = _SelectedProject.OpenResultsWindowCommand.CanExecute(null);
          _SelectedProject.ExportResultsCommand.CanExecuteChanged += (sender2, e2) => btnExportResults.Enabled = _SelectedProject.ExportResultsCommand.CanExecute(null);
          _SelectedProject.MergeAsDOCXCommand.CanExecuteChanged += (sender2, e2) => btnMergeAsDOCX.Enabled = _SelectedProject.MergeAsDOCXCommand.CanExecute(null);
          _SelectedProject.MergeAsPDFCommand.CanExecuteChanged += (sender2, e2) => btnMergeAsPDF.Enabled = _SelectedProject.MergeAsPDFCommand.CanExecute(null);
          _SelectedProject.UploadExamCommand.CanExecuteChanged += (sender2, e2) => btnUpload.Enabled = _SelectedProject.UploadExamCommand.CanExecute(null);
        }
      }
    }

    private void Auth_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ViewModelLocator.Auth.IsLoggedIn) || e.PropertyName == nameof(ViewModelLocator.Auth.IsCommunicating))
      {
        btnLogin.Enabled = !(ViewModelLocator.Auth.IsLoggedIn || ViewModelLocator.Auth.IsCommunicating);
        btnLogout.Enabled = ViewModelLocator.Auth.IsLoggedIn;
      }
    }

    private void ShowHidePaneButton_Click(object sender, RibbonControlEventArgs e)
    {
      if (Globals.ThisAddIn.IsPaneVisible)
        Globals.ThisAddIn.RemoveAllTaskPanes();
      else
        Globals.ThisAddIn.AddAllTaskPanes();
    }

    private void RegisterButton_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.DialogService.OpenRegisterWindow();

      btnShowHidePane.Visible = VM.ViewModelLocator.Register.IsRegistered;

      Globals.ThisAddIn.AddAllTaskPanes();

      foreach (Document Doc in Globals.ThisAddIn.Application.Documents)
      {
        var DupTaskPanes = Globals.ThisAddIn.CustomTaskPanes.Where(tp => tp.Title.StartsWith("MultiDF"));

        foreach (var TP in DupTaskPanes)
        {
          if (TP.Control is MultiDFPaneUC UC)
          {
            if (UC.EH.Child is Views.MainView MV)
            {
              ((VM.MainVM)MV.DataContext).SelectedProject.ProcessCommand.RaiseCanExecuteChanged();
            }
          }
        }
      }
    }

    private void AboutButton_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.DialogService.OpenAboutWindow();
    }

    private void btnLogin_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Auth.LoginCommand.Execute(null);
    }

    private void btnLogout_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Auth.LogoutCommand.Execute(null);
    }

    private void btnUploadExam_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Main.SelectedProject?.UploadExamCommand?.Execute(null);
    }

    private void btnNewProject_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Main.NewCommand.Execute(null);
    }

    private void btnOpenProject_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Main.OpenCommand.Execute(null);
    }

    private void btnSaveProject_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.SaveCommand.Execute(null);
    }

    private void btnExport_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.ExportCommand.Execute(null);
    }

    private void btnMergeAsDOCX_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.MergeAsDOCXCommand.Execute(null);
    }

    private void btnMergeAsPDF_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.MergeAsPDFCommand.Execute(null);
    }

    private void btnUpload_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.UploadExamCommand.Execute(null);
    }

    private void btnAddSourceDoc_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.AddDocsCommand.Execute(null);
    }

    private void btnRemoveSourceDoc_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.RemoveSelectedDocCommand.Execute(null);
    }

    private void btnUpdateQAs_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.UpdateQAsCommand.Execute(null);
    }

    private void btnCheckSyncWithSource_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.CheckSyncWithSourceCommand.Execute(null);
    }

    private void btnProcess_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.ProcessCommand.Execute(null);
    }

    private void btnStopProcess_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.AbortProcessCommand.Execute(null);
    }

    private void btnOpenResultsWindow_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.OpenResultsWindowCommand.Execute(null);
    }

    private void btnExportResults_Click(object sender, RibbonControlEventArgs e)
    {
      if (ViewModelLocator.Main.SelectedProject != null)
        ViewModelLocator.Main.SelectedProject.ExportResultsCommand.Execute(null);
    }

    private void btnShowSeqErrorsPane_Click(object sender, RibbonControlEventArgs e)
    {
      if (Globals.ThisAddIn.Application.ActiveDocument != null)
        Globals.ThisAddIn.AddSeqErrorsTaskPane(Globals.ThisAddIn.Application.ActiveDocument);
      else
        ViewModelLocator.DialogService.ShowMessage("There is no active document.", true);
    }
  }
}