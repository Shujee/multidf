using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a DuplicateFinderMulti project. A project is a set of input files and ad hoc/complete results.
  /// </summary>
  [KnownType(typeof(OurEdge))]
  [KnownType(typeof(OurGraph))]
  public class Project : GalaSoft.MvvmLight.ObservableObject, IDisposable
  {
    //Token will be used to cancel running comparisons.
    private readonly CancellationTokenSource _TokenSource = new CancellationTokenSource();
    private CancellationToken token;

    public Project()
    {
      token = _TokenSource.Token;
    }

    private string _SavePath;
    public string SavePath
    {
      get => _SavePath;
      set => Set(ref _SavePath, value);
    }

    private string _Name;
    public string Name
    {
      get => _Name;
      set => Set(ref _Name, value);
    }

    private bool _IsDirty = false;

    public bool IsDirty
    {
      get => _IsDirty;
      set
      {
        Set(ref _IsDirty, value);

        GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
          SaveCommand.RaiseCanExecuteChanged();
        });
      }
    }

    private ObservableCollection<XMLDoc> _AllXMLDocs = new ObservableCollection<XMLDoc>();
    public ObservableCollection<XMLDoc> AllXMLDocs
    {
      get { return _AllXMLDocs; }
      set => Set(ref _AllXMLDocs, value);
    }

    private XMLDoc _SelectedDoc;
    public XMLDoc SelectedDoc
    {
      get => _SelectedDoc;
      set
      {
        Set(ref _SelectedDoc, value);
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
      }
    }

    //This undirected graph stores comparison results
    private OurGraph graph = new OurGraph();
    public OurGraph Graph
    {
      get => graph;

      ///Setter is only for serialization purpose. Do not call it from anywhere else.
      set => Set(ref graph, value);
    }


    private RelayCommand _ApplyDiffThresholdCommand;
    public RelayCommand ApplyDiffThresholdCommand
    {
      get
      {
        if (_ApplyDiffThresholdCommand == null)
        {
          _ApplyDiffThresholdCommand = new RelayCommand(() =>
          {
            foreach (var Edge in graph.Edges)
              Edge.Tag.DiffThreshold = graph.DiffThreshold;
          },
          () => true);
        }

        return _ApplyDiffThresholdCommand;
      }
    }


    private RelayCommand _AddDocsCommand;
    public RelayCommand AddDocsCommand
    {
      get
      {
        if (_AddDocsCommand == null)
        {
          _AddDocsCommand = new RelayCommand(() =>
          {
            var Docs = ViewModelLocator.DialogService.ShowOpenMulti("Word documents (*.doc, *.docx, *.docm)|*.doc;*.docx;*.docm");

            if (Docs != null)
            {
              foreach (var Doc in Docs)
              {
                AddDocInternal(Doc);
              }

              this.IsDirty = true;
            }
          },
          () => !_IsProcessing);
        }

        return _AddDocsCommand;
      }
    }

    private void AddDocInternal(string Doc)
    {
      System.IO.FileInfo FileInfo = new System.IO.FileInfo(Doc);

      var NewDoc = new XMLDoc()
      {
        LastModified = FileInfo.LastWriteTimeUtc,
        Size = FileInfo.Length,
        SourcePath = Doc
      };

      this._AllXMLDocs.Add(NewDoc);

      graph.AddVertex(NewDoc);

      //Populate QAs for the newly added document. This call is asynchronous.
      NewDoc.UpdateQAs();
    }

    private RelayCommand _RemoveSelectedDocCommand;
    public RelayCommand RemoveSelectedDocCommand
    {
      get
      {
        if (_RemoveSelectedDocCommand == null)
        {
          _RemoveSelectedDocCommand = new RelayCommand(() =>
          {
            graph.RemoveVertex(_SelectedDoc);
            this._AllXMLDocs.Remove(_SelectedDoc);

            this.IsDirty = true;
          },
          () => _SelectedDoc != null && !_IsProcessing);
        }

        return _RemoveSelectedDocCommand;
      }
    }

    private RelayCommand _SaveCommand;
    public RelayCommand SaveCommand
    {
      get
      {
        if (_SaveCommand == null)
        {
          _SaveCommand = new RelayCommand(() =>
          {
            string TempSavePath = null;

            if (_SavePath == null)
            {
              TempSavePath = ViewModelLocator.DialogService.ShowSave(MainVM.FILTER_XML_FILES);
            }
            else
              TempSavePath = _SavePath;

            if (TempSavePath != null)
            {
              this.Name = System.IO.Path.GetFileNameWithoutExtension(_SavePath);

              this.IsDirty = false;
              this._SavePath = TempSavePath;
              string ProjectXML = this.ToXML();
              File.WriteAllText(TempSavePath, ProjectXML);
             
              SaveCommand.RaiseCanExecuteChanged();

              RaisePropertyChanged(nameof(Name));
            }
          },
          () => this.IsDirty && !_IsProcessing);
        }

        return _SaveCommand;
      }
    }


    private RelayCommand _UpdateQAsCommand;
    public RelayCommand UpdateQAsCommand
    {
      get
      {
        if (_UpdateQAsCommand == null)
        {
          _UpdateQAsCommand = new RelayCommand(() =>
          {
            if (SelectedDoc != null)
            {
              var DocPath = SelectedDoc.SourcePath;

              //remove this doc from the graph and add again to clear old processed results
              graph.RemoveVertex(SelectedDoc);
              this._AllXMLDocs.Remove(SelectedDoc);

              //now add it again as a new doc
              AddDocInternal(DocPath);

              //and refresh results
              RaisePropertyChanged(nameof(Graph));

              IsDirty = true;
            }
          },
          () => !_IsProcessing);
        }

        return _UpdateQAsCommand;
      }
    }

    private RelayCommand _CheckSyncWithSourceCommand;
    public RelayCommand CheckSyncWithSourceCommand
    {
      get
      {
        if (_CheckSyncWithSourceCommand == null)
        {
          _CheckSyncWithSourceCommand = new RelayCommand(() =>
          {
            foreach (var Doc in this.AllXMLDocs)
              Doc.RaisePropertyChanged(nameof(XMLDoc.IsSyncWithSource));
          },
          () => true);
        }

        return _CheckSyncWithSourceCommand;
      }
    }

    private RelayCommand _ProcessCommand;
    public RelayCommand ProcessCommand
    {
      get
      {
        if (_ProcessCommand == null)
        {
          _ProcessCommand = new RelayCommand(() =>
          {
            IsProcessing = true;

            ViewModelLocator.DocComparer.QACompared += (sender, args) =>
            {
              args.QA1.Doc.ProcessingProgress = args.PercentProgress;
              args.QA2.Doc.ProcessingProgress = args.PercentProgress;
            };

            ViewModelLocator.DocComparer.DocCompareStarted += (d1, d2) =>
            {
              d1.ProcessingProgress = 0;
              d2.ProcessingProgress = 0;
            };

            ViewModelLocator.DocComparer.DocCompareCompleted += (d1, d2) =>
            {
              d1.ProcessingProgress = 100;
              d2.ProcessingProgress = 100;
            };

            List<Task> Tasks = new List<Task>();
            foreach (var V1 in graph.Vertices)
            {
              foreach (var V2 in graph.Vertices)
              {
                if (V1.QAs != null && V2.QAs != null && !graph.ContainsEdge(V1, V2))
                {
                  var Edge = new OurEdge(V1, V2, null);

                  if (graph.AddEdge(Edge))
                  {
                    var Task = ViewModelLocator.DocComparer.Compare(V1, V2, ViewModelLocator.QAComparer, true, token);
                    Task.ContinueWith((t1) =>
                    {
                      Edge.Tag = t1.Result;
                      IsDirty = true;
                    });

                    Tasks.Add(Task);
                  }
                }
              }
            }

            Task.WhenAll(Tasks.ToArray()).ContinueWith(t =>
            {
#if(DEBUG)
              if (UnitTestDetector.IsInUnitTest)
                IsProcessing = false;
              else
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => IsProcessing = false);
#else
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => IsProcessing = false);
#endif

              RaisePropertyChanged(nameof(Graph));
              ApplyDiffThresholdCommand.Execute(null);
            });
          },
          () => !_IsProcessing);
        }

        return _ProcessCommand;
      }
    }

    private RelayCommand _AbortProcessCommand;
    public RelayCommand AbortProcessCommand
    {
      get
      {
        if (_AbortProcessCommand == null)
        {
          _AbortProcessCommand = new RelayCommand(() =>
          {
            _TokenSource.Cancel();
          },
          () => _IsProcessing);
        }

        return _AbortProcessCommand;
      }
    }


    private RelayCommand _ExportCommand;
    public RelayCommand ExportCommand
    {
      get
      {
        if (_ExportCommand == null)
        {
          _ExportCommand = new RelayCommand(() =>
          {
            if(this.AllXMLDocs.Any(d => !d.IsSyncWithSource))
            {
              ViewModelLocator.DialogService.ShowMessage("Some of the documents in this project are not synchronized with source files. Refresh the project before using Export command.", true);
              return;
            }

            var ExportSavePath = ViewModelLocator.DialogService.ShowSave(MainVM.FILTER_XML_FILES);

            if (ExportSavePath != null)
            {
              Project P = Project.FromXML(this.ToXML());

              P.Name = Path.GetFileNameWithoutExtension(ExportSavePath);
              P.SavePath = ExportSavePath;

              string DestFolder = Path.GetDirectoryName(ExportSavePath);
              foreach (var Doc in P.AllXMLDocs)
              {
                var DestFile = GetAvailableFileName(Doc.SourcePath, DestFolder);
                File.Copy(Doc.SourcePath, DestFile);
                Doc.SourcePath = DestFile;
              }

              P.IsDirty = false;
              File.WriteAllText(ExportSavePath, P.ToXML());
              ViewModelLocator.DialogService.ShowMessage($"Project has been exported successfully to '{ExportSavePath}'.", false);
            }
          },
          () => !_IsProcessing);
        }

        return _ExportCommand;
      }
    }

    /// <summary>
    /// Returns an file name that is not currently in use in the specified directory by appending an integer to the file name. 
    /// </summary>
    /// <returns></returns>
    private string GetAvailableFileName(string sourcePath, string destFolder)
    {
      string DocFileName = Path.GetFileName(sourcePath);
      string DocFileNameNoExt = Path.GetFileNameWithoutExtension(sourcePath);
      string DocExt = Path.GetExtension(sourcePath);
      var DestFile = Path.Combine(destFolder, DocFileName);
      int i = 1;

      while (File.Exists(DestFile))
        DestFile = Path.Combine(destFolder, DocFileNameNoExt + ' ' + (i++).ToString() + DocExt);

      return DestFile;
    }


    private RelayCommand<DFResultRow> _OpenDiffCommand;
    public RelayCommand<DFResultRow> OpenDiffCommand
    {
      get
      {
        if (_OpenDiffCommand == null)
        {
          _OpenDiffCommand = new RelayCommand<DFResultRow>((row) =>
          {
            ViewModelLocator.DialogService.OpenDiffWindow(row.Q1.Question, row.Q2.Question, row.Q1.Choices, row.Q2.Choices);
          },
          (row) => true);
        }

        return _OpenDiffCommand;
      }
    }
    
    private bool _IsProcessing = false;

    [XmlIgnore]
    public bool IsProcessing
    {
      get { return _IsProcessing; }
      private set
      {
        Set(ref _IsProcessing, value);

        SaveCommand.RaiseCanExecuteChanged();
        ProcessCommand.RaiseCanExecuteChanged();
        AbortProcessCommand.RaiseCanExecuteChanged();
        AddDocsCommand.RaiseCanExecuteChanged();
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
        UpdateQAsCommand.RaiseCanExecuteChanged();
        ExportCommand.RaiseCanExecuteChanged();
      }
    }

    public static Project FromXML(string xml)
    {
      return xml.DeserializeDC<Project>();
    }

    public string ToXML()
    {
      return this.SerializeDC();
    }

    public void Dispose()
    {
      Dispose(true);
    }

    // The bulk of the clean-up code is implemented in Dispose(bool)
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // free managed resources
        if (_TokenSource != null)
          _TokenSource.Dispose();
      }
    }
  }
}