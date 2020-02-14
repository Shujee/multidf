using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VMBase;

namespace MultiDF.VM
{
  public partial class MainVM : VMBase.MainBase
  {
    public const string FILTER_IMAGE_FILES_ALL_FILES = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif|All Files (*.*)|*.*";
    public const string FILTER_XML_FILES = "XML Files (*.xml)|*.xml";

    public MainVM()
    {
      if (VM.Properties.Settings.Default.MRU == null)
        VM.Properties.Settings.Default.MRU = new System.Collections.Specialized.StringCollection();
    }

    public void Init()
    {
      _ProgressStartTime = DateTime.Now;

      if (IsInDesignModeStatic)
      {
        _SelectedProject = new Project()
        {
          IsDirty = false,
          Name = "Project 1",
        };

        _SelectedProject.AllXMLDocs.Add(new XMLDoc()
        {
          LastModified = System.DateTime.Now.AddDays(-1),
          Size = 123456,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\Sample Question for DF Multi.docx",
        });

        _SelectedProject.AllXMLDocs.Add(new XMLDoc()
        {
          LastModified = System.DateTime.Now.AddDays(-8),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 1 for DF.docx",
        });

        _SelectedProject.AllXMLDocs.Add(new XMLDoc()
        {
          LastModified = System.DateTime.Now.AddDays(-15),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 2 for DF.docx",
        });

        _SelectedProject.AllXMLDocs.Add(new XMLDoc()
        {
          LastModified = System.DateTime.Now.AddDays(-1),
          Size = 123456,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 3 for DF.docx",
        });

        _SelectedProject.AllXMLDocs.Add(new XMLDoc()
        {
          LastModified = System.DateTime.Now.AddDays(-8),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\Doc that does not exist.docx",
        });

        var Doc1 = new XMLDoc() { SourcePath = "Doc1" };
        _SelectedProject.Graph.AddVertex(Doc1);
        var Doc2 = new XMLDoc() { SourcePath = "Doc2" };
        _SelectedProject.Graph.AddVertex(Doc2);

        var DFRes1 = new DFResult(Doc1, Doc2, 31, 12);
        DFRes1.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 51, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 78, Question = "Another very long string" }, 1.4));
        DFRes1.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 51, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 78, Question = "Another very long string" }, 1.4));
        DFRes1.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 51, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 78, Question = "Another very long string" }, 1.4));
        _SelectedProject.Graph.AddEdge(new OurEdge(Doc1, Doc2, DFRes1));

        var DFRes2 = new DFResult(Doc1, Doc2, 200, 155);
        DFRes2.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 23, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 5, Question = "Another very long string" }, 0.6434));
        DFRes2.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 23, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 5, Question = "Another very long string" }, 0.6434));
        _SelectedProject.Graph.AddEdge(new OurEdge(Doc1, Doc2, DFRes2));

        var DFRes3 = new DFResult(Doc1, Doc2, 144, 27);
        DFRes3.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 36, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 154, Question = "Another very long string" }, 0.8763));
        DFRes3.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 36, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 154, Question = "Another very long string" }, 0.8763));
        DFRes3.Items.Add(new DFResultRow(new QA() { Doc = Doc1, Index = 36, Question = "Some very long string" }, new QA() { Doc = Doc2, Index = 154, Question = "Another very long string" }, 0.8763));
        _SelectedProject.Graph.AddEdge(new OurEdge(Doc1, Doc2, DFRes3));
      }
      else
      {
        NewCommand.Execute(null);
      }

      ViewModelLocator.Auth.PropertyChanged += Auth_PropertyChanged;
    }

    /// <summary>
    /// If user logs in or logs out, update the enabled state of all commands that require login.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Auth_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(AuthVM.IsLoggedIn))
      {
        if (this._SelectedProject != null)
          _SelectedProject.UploadExamCommand.RaiseCanExecuteChanged();
      }
    }

    public List<string> MRU => VM.Properties.Settings.Default.MRU.Cast<string>().Select(x => x).ToList();

    protected Project _SelectedProject;
    public Project SelectedProject
    {
      get => _SelectedProject;
      set
      {
        //unbind old project's event listener
        if (_SelectedProject != null)
          _SelectedProject.PropertyChanged -= SelectedProject_PropertyChanged;

        Set(ref _SelectedProject, value);

        NewCommand.RaiseCanExecuteChanged();
        OpenCommand.RaiseCanExecuteChanged();

        //bind new project's event listener
        if (_SelectedProject != null)
        {
          _SelectedProject.PropertyChanged += SelectedProject_PropertyChanged;
          _SelectedProject.UploadExamCommand.RaiseCanExecuteChanged();
          _SelectedProject.MergeAsPDFCommand.RaiseCanExecuteChanged();
          _SelectedProject.CheckSyncWithSourceCommand.RaiseCanExecuteChanged();
        }
      }
    }

    private void SelectedProject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(Project.IsProcessing) || e.PropertyName == nameof(Project.IsExtractingQA))
      {
        GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
          NewCommand.RaiseCanExecuteChanged();
          OpenCommand.RaiseCanExecuteChanged();
        });
      }
    }

    private RelayCommand _NewCommand;
    public RelayCommand NewCommand
    {
      get
      {
        if (_NewCommand == null)
        {
          _NewCommand = new RelayCommand(() =>
          {
            if (SelectedProject != null && SelectedProject.IsDirty)
            {
              var Res = ViewModelLocator.DialogService.AskTernaryQuestion("Active project contains unsaved changes. Do you want to save these changes before proceeding?");
              if (Res == null)
                return;
              else if (Res.Value)
                SelectedProject.SaveCommand.Execute(null);
            }

            SelectedProject = new Project() { Name = "New Project" };
            _SelectedProject.SavePath = null;
          },
          () => SelectedProject == null || (!SelectedProject.IsProcessing && !SelectedProject.IsExtractingQA));
        }

        return _NewCommand;
      }
    }

    private RelayCommand<string> _OpenCommand;
    public RelayCommand<string> OpenCommand
    {
      get
      {
        if (_OpenCommand == null)
        {
          _OpenCommand = new RelayCommand<string>((file) =>
          {
            if (SelectedProject != null && SelectedProject.IsDirty)
            {
              var Res = ViewModelLocator.DialogService.AskTernaryQuestion("Active project contains unsaved changes. Do you want to save these changes before proceeding?");
              if (Res == null)
                return;
              else if (Res.Value)
                SelectedProject.SaveCommand.Execute(null);
            }

            string ProjectPath = file;
            if (string.IsNullOrEmpty(ProjectPath))
              ProjectPath = ViewModelLocator.DialogService.ShowOpen(FILTER_XML_FILES);

            if (!string.IsNullOrEmpty(ProjectPath))
            {
              if (!System.IO.File.Exists(ProjectPath))
                ViewModelLocator.DialogService.ShowMessage("Specified project file does not exist.", true);
              else
              {
                var ProjectXML = System.IO.File.ReadAllText(ProjectPath);
                var TempSelectedProject = Project.FromXML(ProjectXML);
                TempSelectedProject.SavePath = ProjectPath;
                bool ProjectFileChanged = false;

                foreach (var Doc in TempSelectedProject.AllXMLDocs)
                {
                  //check if SourcePath contains only file name, not full path. If so, check if the document resides in project's folder (support for relative path).
                  //If both of these conditions are true, then update SourcePath to fully qualified path using project path.
                  if (!string.IsNullOrEmpty(Doc.SourcePath) && Path.GetFileName(Doc.SourcePath) == Doc.SourcePath)
                  {
                    if (File.Exists(Path.Combine(Path.GetDirectoryName( ProjectPath), Doc.SourcePath)))
                    {
                      Doc.SourcePath = Path.Combine(Path.GetDirectoryName(ProjectPath), Doc.SourcePath);
                      ProjectFileChanged = true;
                    }
                  }

                  if (string.IsNullOrEmpty(Doc.SourcePath) || !File.Exists(Doc.SourcePath))
                  {
                    var Res = ViewModelLocator.DialogService.AskTernaryQuestion($"Document '{Doc.SourcePath}' does not exist on the disk. Do you want to locate it manually?");

                    if (Res.HasValue)
                    {
                      if (Res.Value)
                      {
                        var NewDocx = ViewModelLocator.DialogService.ShowOpen("Word documents (*.doc, *.docx, *.docm)|*.doc;*.docx;*.docm", System.IO.Path.GetDirectoryName(ProjectPath), "Select Word Document");

                        if (NewDocx != null)
                        {
                          Doc.SourcePath = NewDocx;
                          ProjectFileChanged = true;
                        }
                      }
                    }
                    else
                      return;
                  }
                }

                if (ProjectFileChanged)
                  TempSelectedProject.SaveCommand.Execute(null);

                SelectedProject = TempSelectedProject;

                UpdateMRU(ProjectPath);

                RaisePropertyChanged(nameof(MRU));
              }
            }
          },
          (file) => SelectedProject == null || (!SelectedProject.IsProcessing && !SelectedProject.IsExtractingQA));
        }

        return _OpenCommand;
      }
    }

    internal void UpdateMRU(string newFile)
    {
      //if the newly added file was already available in MRU, remove the old instance.
      while (VM.Properties.Settings.Default.MRU.Contains(newFile))
        VM.Properties.Settings.Default.MRU.Remove(newFile);

      //now add new entry at the top
      VM.Properties.Settings.Default.MRU.Insert(0, newFile);

      //we keep track of last 5 files, so remove older items
      while (VM.Properties.Settings.Default.MRU.Count > 5)
        VM.Properties.Settings.Default.MRU.RemoveAt(5);

      VM.Properties.Settings.Default.Save();
    }
  }
}