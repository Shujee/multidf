﻿using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System;
using System.Collections;
using DuplicateFinderMultiCommon;

namespace DuplicateFinderMulti.VM
{
  public partial class MainVM : ViewModelBase
  {
    public const string FILTER_IMAGE_FILES_ALL_FILES = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif|All Files (*.*)|*.*";
    public const string FILTER_XML_FILES = "XML Files (*.xml)|*.xml";

    public MainVM()
    {
      _ProgressStartTime = DateTime.Now;

      if (IsInDesignModeStatic)
      {
        _SelectedProject = new Project()
        {
          IsDirty = false,
          Name = "Project 1",
          AllXMLDocs = new System.Collections.ObjectModel.ObservableCollection<XMLDoc>()
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

      if (VM.Properties.Settings.Default.MRU == null)
        VM.Properties.Settings.Default.MRU = new System.Collections.Specialized.StringCollection();
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
          _SelectedProject.PropertyChanged += SelectedProject_PropertyChanged;
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
                SelectedProject = Project.FromXML(ProjectXML);
                _SelectedProject.SavePath = ProjectPath;

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

    private RelayCommand _UploadExamCommand;
    /// <summary>
    /// Creates a new Exam on the server using the Master File selected by the user. Can only be performed by the Admin.
    /// </summary>
    public RelayCommand UploadExamCommand
    {
      get
      {
        if (_UploadExamCommand == null)
        {
          _UploadExamCommand = new RelayCommand(() =>
          {
            var DocPath = ViewModelLocator.DialogService.ShowOpen("Word Documents (*.docx)|*.docx");
            if (!string.IsNullOrEmpty(DocPath) && System.IO.File.Exists(DocPath))
            {
              UploadExamInternal(DocPath);
            }
          },
          () => true);
        }

        return _UploadExamCommand;
      }
    }

    private void UploadExamInternal(string DocPath)
    {
      string ExamName = ViewModelLocator.DialogService.AskStringQuestion("Please provide a name for this master file:", System.IO.Path.GetFileNameWithoutExtension(DocPath));

      if (!string.IsNullOrEmpty(ExamName))
      {
        //Create the XPS file
        var XPSFile = StaticExtensions.GetTempFileName(".xps");
        ViewModelLocator.WordService.ExportDocumentToXPS(DocPath, XPSFile);

        //Encrypt the XPS file
        var XPSFileEncrypted = Encryption.Encrypt(System.IO.File.ReadAllBytes(XPSFile));
        System.IO.File.WriteAllBytes(XPSFile, XPSFileEncrypted);

        //Create the XML file
        var XMLDoc = new XMLDoc() { SourcePath = DocPath };
        XMLDoc.UpdateQAs().ContinueWith(t =>
        {
          if (t.IsCompleted && !t.IsFaulted)
          {
            if (XMLDoc.QAs != null)
            {
              var XMLFile = StaticExtensions.GetTempFileName(".xml");
              System.IO.File.WriteAllText(XMLFile, XMLDoc.Serialize());

              //Encrypt the XML file
              var XMLFileEncrypted = Encryption.Encrypt(System.IO.File.ReadAllBytes(XMLFile));
              System.IO.File.WriteAllBytes(XMLFile, XMLFileEncrypted);

              try
              {
                ViewModelLocator.Auth.IsCommunicating = true;
                ViewModelLocator.DataService.UploadExam(XPSFile, XMLFile, ExamName, XMLDoc.QAs.Count);
                ViewModelLocator.DialogService.ShowMessage("Master file was uploaded successfully.", false);
              }
              catch (Exception ee)
              {
                var msg = ee.Message;

                if (ee.Data.Count > 0)
                {
                  msg += Environment.NewLine;

                  foreach (DictionaryEntry Err in ee.Data)
                  {
                    foreach (var Msg in (string[])Err.Value)
                      msg += Environment.NewLine + Msg;
                  }
                }

                ViewModelLocator.DialogService.ShowMessage(msg, true);
              }
              finally
              {
                ViewModelLocator.Auth.IsCommunicating = false;
              }

            }
            else
              ViewModelLocator.DialogService.ShowMessage("Could not extract QAs from the document.", true);
          }
          else
            ViewModelLocator.DialogService.ShowMessage(t.Exception.Message, true);
        }).Wait();
      }
    }

    private RelayCommand _UploadActiveExamCommand;
    public RelayCommand UploadActiveExamCommand
    {
      get
      {
        if (_UploadActiveExamCommand == null)
        {
          _UploadActiveExamCommand = new RelayCommand(() =>
          {
            if (ViewModelLocator.WordService.ActiveDocumentPath == null)
              ViewModelLocator.DialogService.ShowMessage("You must open a Word Document to use this command.", true);
            else
            {
              try
              {
                ViewModelLocator.Auth.IsCommunicating = true;
                UploadExamInternal(ViewModelLocator.WordService.ActiveDocumentPath);
              }
              finally
              {
                ViewModelLocator.Auth.IsCommunicating = false;
              }
            }
          },
          () => ViewModelLocator.Auth.IsLoggedIn && ViewModelLocator.WordService.ActiveDocumentPath != null);
        }

        return _UploadActiveExamCommand;
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

    #region "Status"
    private string _ProgressMessage;
    public string ProgressMessage
    {
      get { return _ProgressMessage; }
    }

    private double _ProgressValue;
    public double ProgressValue
    {
      get { return _ProgressValue; }
    }

    private DateTime _ProgressStartTime =DateTime.Now;
    public TimeSpan ElapsedTime => DateTime.Now.Subtract(_ProgressStartTime);
    public TimeSpan EstimatedRemainingTime => _ProgressValue == 0? TimeSpan.Zero : TimeSpan.FromSeconds((ElapsedTime.TotalSeconds / _ProgressValue) * (1 - _ProgressValue));

    public void UpdateProgress(bool isStarting, string msg, double value)
    {
      if (isStarting)
        _ProgressStartTime = DateTime.Now;

      _ProgressMessage = msg;
      _ProgressValue = value;
    }
    #endregion
  }
}