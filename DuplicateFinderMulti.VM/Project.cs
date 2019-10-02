﻿using GalaSoft.MvvmLight.Command;
using QuickGraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  public class Project : GalaSoft.MvvmLight.ObservableObject
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

    private bool _IsDirty;
    public bool IsDirty
    {
      get => _IsDirty;
      set
      {
        Set(ref _IsDirty, value);
        SaveCommand.RaiseCanExecuteChanged();
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

              this.IsDirty = true;
            }
          },
          () => !_IsProcessing);
        }

        return _AddDocsCommand;
      }
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
            this._AllXMLDocs.Remove(_SelectedDoc);

            graph.RemoveVertex(_SelectedDoc);

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

              string ProjectXML = this.ToXML();
              System.IO.File.WriteAllText(TempSavePath, ProjectXML);
              this._SavePath = TempSavePath;
              this.IsDirty = false;
              SaveCommand.RaiseCanExecuteChanged();

              RaisePropertyChanged(nameof(Name));
            }
          },
          () => this.IsDirty);
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
            foreach (var Doc in this.AllXMLDocs)
              Doc.UpdateQAs();
          },
          () => true);
        }

        return _UpdateQAsCommand;
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

            ViewModelLocator.DocComparer.QACompared += (args) =>
            {
              args.QA1.Doc.ProcessingProgress = args.PercentProgress;
              args.QA2.Doc.ProcessingProgress = args.PercentProgress;
            };

            List<Task> Tasks = new List<Task>();
            foreach (var V1 in graph.Vertices)
            {
              foreach (var V2 in graph.Vertices)
              {
                if (V1 != V2 && !graph.ContainsEdge(V1, V2) && V1.QAs != null && V2.QAs != null)
                {
                  var Edge = new OurEdge(V1, V2, null);

                  if (graph.AddEdge(Edge))
                  {
                    var Task = ViewModelLocator.DocComparer.Compare(V1, V2, ViewModelLocator.QAComparer, true, token);
                    Task.ContinueWith((t1) =>
                    {
                      Edge.Tag = t1.Result;
                      RaisePropertyChanged(nameof(Graph));
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

    private bool _IsProcessing = false;

    [XmlIgnore]
    public bool IsProcessing
    {
      get { return _IsProcessing; }
      private set
      {
        Set(ref _IsProcessing, value);
        ProcessCommand.RaiseCanExecuteChanged();
        AbortProcessCommand.RaiseCanExecuteChanged();
        AddDocsCommand.RaiseCanExecuteChanged();
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
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
  }
}