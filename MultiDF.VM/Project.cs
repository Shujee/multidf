﻿using Common;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VMBase;
using System.Xml.Linq;
using System.Text;

namespace MultiDF.VM
{
  /// <summary>
  /// Public property of this type are serialized as XML attribute instead of XML child element.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class AttributeProperty<T>
  {
    [XmlAttribute]
    public T Value { get; set; }
  }

  /// <summary>
  /// Represents a MultiDF project. A project is a set of input files and ad hoc/complete results.
  /// </summary>
  [KnownType(typeof(OurEdge))]
  [KnownType(typeof(OurGraph))]
  public class Project : GalaSoft.MvvmLight.ObservableObject, IDisposable
  {
    public AttributeProperty<string> Version
    {
      get => new AttributeProperty<string>() { Value = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() };
      set { } //Serializer writes a property to the output only when it has a setter
    }

    //Token will be used to cancel running comparisons.
    private readonly CancellationTokenSource _TokenSource = new CancellationTokenSource();
    private CancellationToken token;

    //these variables keep track of progress during ProcessCommand
    double TotalComparisons;
    int CompletedComparisons = 0;
    double Factor; //avoid computing constant factor over and over in the loop. just a small micro-optimization that I couldn't resist :)


    public Project()
    {
      token = _TokenSource.Token;

      ViewModelLocator.DocComparer.QACompared += DocComparer_QACompared;
      ViewModelLocator.DocComparer.QASkipped += DocComparer_QASkipped;
      ViewModelLocator.DocComparer.DocCompareCompleted += DocComparer_DocCompareCompleted;

      AllXMLDocs = new ObservableCollection<XMLDoc>();
      AllXMLDocs.CollectionChanged += (sender, e) =>
      {
        GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
          UploadExamCommand.RaiseCanExecuteChanged();
          MergeAsDOCXCommand.RaiseCanExecuteChanged();
          MergeAsPDFCommand.RaiseCanExecuteChanged();
        });
      };
    }

    private void DocComparer_QACompared(object sender, QAComparedArgs e)
    {
      Interlocked.Increment(ref CompletedComparisons);
      e.QA1.Doc.ProcessingProgress = e.PercentProgress;
      e.QA2.Doc.ProcessingProgress = e.PercentProgress;

      ViewModelLocator.Main.UpdateProgress(false, "QA compared", Factor * CompletedComparisons);
    }

    private void DocComparer_QASkipped()
    {
      Interlocked.Increment(ref CompletedComparisons);
    }

    private void DocComparer_DocCompareCompleted(XMLDoc doc1, XMLDoc doc2)
    {
      doc1.ProcessingProgress = 100;
      doc2.ProcessingProgress = 100;
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

    public ObservableCollection<XMLDoc> AllXMLDocs { get; set; }

    private XMLDoc _SelectedDoc;
    public XMLDoc SelectedDoc
    {
      get => _SelectedDoc;
      set
      {
        Set(ref _SelectedDoc, value);
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
        UpdateQAsCommand.RaiseCanExecuteChanged();
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


    private RelayCommand _OpenResultsWindowCommand;
    public RelayCommand OpenResultsWindowCommand
    {
      get
      {
        if (_OpenResultsWindowCommand == null)
        {
          _OpenResultsWindowCommand = new RelayCommand(() =>
          {
            ViewModelLocator.DialogServiceMultiDF.OpenResultsWindow();
          },
          () => true);
        }

        return _OpenResultsWindowCommand;
      }
    }


    private RelayCommand _ExportResultsCommand;
    public RelayCommand ExportResultsCommand
    {
      get
      {
        if (_ExportResultsCommand == null)
        {
          _ExportResultsCommand = new RelayCommand(() =>
          {

            var ExportFilePath = ViewModelLocator.DialogService.ShowSave("HTML Files (*.html)|*.html", "", "Export Results to HTML", this.Name + ".html");

            if (ExportFilePath != null)
            {
              string TablesHtml = "";

              foreach (var Res in graph.Results)
              {
                TablesHtml += Res.ToHtml();
              }

              var ExportHtml = VM.Properties.Resources.ResultsExportTemplate.Replace("{TABLES}", TablesHtml);

              File.WriteAllText(ExportFilePath, ExportHtml);

              ViewModelLocator.DialogService.ShowMessage($"Results exported to '{ExportFilePath}'.", false);
            }

          },
          () => !_IsProcessing && graph.Edges.Any());
        }

        return _ExportResultsCommand;
      }
    }


    private RelayCommand _MergeAsPDFCommand;
    public RelayCommand MergeAsPDFCommand
    {
      get
      {
        if (_MergeAsPDFCommand == null)
        {
          _MergeAsPDFCommand = new RelayCommand(() =>
          {
            //Merge the project source files into a single document and save it to user-specified path
            MergeInternal("PDF Files (*.pdf)|*.pdf", (mergedDocPath, savePath) => ViewModelLocator.WordService.ExportDocumentToFixedFormat(ExportFixedFormat.PDF, mergedDocPath, savePath, true));
          },
          () => this.AllXMLDocs.Count > 0);
        }

        return _MergeAsPDFCommand;
      }
    }
    
    private RelayCommand _MergeAsDOCXCommand;
    public RelayCommand MergeAsDOCXCommand
    {
      get
      {
        if (_MergeAsDOCXCommand == null)
        {
          _MergeAsDOCXCommand = new RelayCommand(() =>
          {
            //Merge the project source files into a single document and save it to user-specified path
            MergeInternal("Word Documents (*.docx)|*.docx", (mergedDocPath, savePath) => File.Copy(mergedDocPath, savePath));
          },
          () => this.AllXMLDocs.Count > 0);
        }

        return _MergeAsDOCXCommand;
      }
    }

    /// <summary>
    /// Merges all source documents of the current project into a single new document, fixes QA numbering and then invokes caller specified function
    /// to save/export this new document.
    /// </summary>
    /// <param name="dialogFilter"></param>
    /// <param name="callback"></param>
    private void MergeInternal(string dialogFilter, Action<string, string> callback)
    {
      //Make sure full analysis has been completed before merging
      if (graph.Vertices.Any(v1 => graph.Vertices.Any(v2 => !graph.ContainsEdge(v1, v2))))
      {
        ViewModelLocator.DialogService.ShowMessage("One or more documents in the project have not been analyzed for duplicates yet. Use Process command to complete analysis.", true);
        return;
      }

      var OutputPath = ViewModelLocator.DialogService.ShowSave(dialogFilter);

      if (OutputPath != null)
      {
        var TempDocxPath = StaticExtensions.GetTempFileName(".docx");
        ViewModelLocator.WordService.CreateMergedDocument(this.AllXMLDocs.Select(d => d.SourcePath).ToArray(), TempDocxPath, false);
        var WPs = ViewModelLocator.WordService.GetDocumentParagraphs(TempDocxPath, token, UpdateQAsProgressHandler, false);
        var DelimiterParas = ViewModelLocator.QAExtractionStrategy.ExtractDelimiterParagraphs(WPs, token, true);
        ViewModelLocator.WordService.FixAllQANumbers(TempDocxPath, DelimiterParas);

        callback(TempDocxPath, OutputPath);

        if (ViewModelLocator.DialogService.AskBooleanQuestion($"Successfully merged all documents into '{OutputPath}'. Do you want to open it now?"))
        {
          System.Diagnostics.Process.Start(OutputPath);
        }
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
            var VM = ViewModelLocator.UploadExam;
            VM.RefreshExamsCommand.Execute(null);
            VM.FileName = this.AllXMLDocs.FirstOrDefault()?.Name;
            VM.QACount = this.AllXMLDocs.Sum(doc => doc.QAs.Count);

            var Res = ViewModelLocator.DialogServiceMultiDF.ShowUploadExamDialog(VM);          

            if (Res && ((VM.CreateNew && !string.IsNullOrEmpty(VM.NewExamName)) || (!VM.CreateNew && VM.SelectedExam != null)))
            {
              Task.Run(() =>
              {
                //Merge all XMLDocs into a single temporary Word document
                var TempDocxPath = StaticExtensions.GetTempFileName(".docx");
                ViewModelLocator.WordService.CreateMergedDocument(this.AllXMLDocs.Select(d => d.SourcePath).ToArray(), TempDocxPath, false);

                ViewModelLocator.Main.UpdateProgress(false, "Extracting paragraphs from merged document", 0);
                var WPs = ViewModelLocator.WordService.GetDocumentParagraphs(TempDocxPath, token, UpdateQAsProgressHandler, false);

               ViewModelLocator.Main.UpdateProgress(false, "Marking delimiters", 0);
                var DelimiterParas = ViewModelLocator.QAExtractionStrategy.ExtractDelimiterParagraphs(WPs, token, true);
                ViewModelLocator.Main.UpdateProgress(false, null, 1);

                ViewModelLocator.Main.UpdateProgress(false, "Re-numbering merged QAs", 0);
                ViewModelLocator.WordService.FixAllQANumbers(TempDocxPath, DelimiterParas);
                ViewModelLocator.Main.UpdateProgress(false, null, 1);

                return TempDocxPath;

              }).ContinueWith(t =>
              {
                if (t.IsCompleted && !t.IsFaulted)
                {
                  var TempDocxPath = t.Result;

                  //Create the XPS file
                  ViewModelLocator.Main.UpdateProgress(false, "Creating XPS", 0);
                  var XPSFile = StaticExtensions.GetTempFileName(".xps");
                  ViewModelLocator.WordService.ExportDocumentToFixedFormat(ExportFixedFormat.XPS, TempDocxPath, XPSFile, true);
                  ViewModelLocator.Main.UpdateProgress(false, null, 1);

                  //Encrypt the XPS file
                  ViewModelLocator.Main.UpdateProgress(false, "Creating XML", 0);
                  var XPSFileEncrypted = Encryption.Encrypt(System.IO.File.ReadAllBytes(XPSFile));
                  File.WriteAllBytes(XPSFile, XPSFileEncrypted);
                  ViewModelLocator.Main.UpdateProgress(false, null, 1);

                  //Create the XML file
                  var XMLDoc = new XMLDoc() { SourcePath = TempDocxPath };
                  UpdateQAs(XMLDoc).ContinueWith(t2 =>
                  {
                    if (t2.IsCompleted && !t2.IsFaulted)
                    {
                      if (XMLDoc.QAs != null)
                      {
                        var WrongStartPageEndPage = XMLDoc.QAs.FirstOrDefault(qa => qa.StartPage <= 0 || qa.EndPage <= 0);
                        if (WrongStartPageEndPage != null)
                        {
                          ViewModelLocator.Logger.Error($"Some of the QAs do not have correct values for StartPage and/or EndPage. Q. No. {WrongStartPageEndPage.Index} has StartPage = {WrongStartPageEndPage.StartPage}, EndPage = {WrongStartPageEndPage.EndPage}.");
                          ViewModelLocator.DialogService.ShowMessage($"Some of the QAs do not have correct values for StartPage and/or EndPage. This exam cannot be uploaded. Q. No. {WrongStartPageEndPage.Index} has StartPage = {WrongStartPageEndPage.StartPage}, EndPage = {WrongStartPageEndPage.EndPage}.", true);
                          return;
                        }

                        var XMLFile = StaticExtensions.GetTempFileName(".xml");
                        File.WriteAllText(XMLFile, XMLDoc.ToXML());

                        //Encrypt the XML file
                        var XMLFileEncrypted = Encryption.Encrypt(File.ReadAllBytes(XMLFile));
                        File.WriteAllBytes(XMLFile, XMLFileEncrypted);

                        try
                        {
                          ViewModelLocator.Auth.IsCommunicating = true;

                          if (VM.CreateNew)
                          {
                            ViewModelLocator.DataService.UploadExam(XPSFile, XMLFile, VM.NewExamNumber, VM.NewExamName, VM.NewExamDescription, XMLDoc.QAs.Count, Newtonsoft.Json.JsonConvert.SerializeObject(XMLDoc.QAs), VM.FileName);
                            ViewModelLocator.DialogService.ShowMessage("Master file was uploaded successfully.", false);
                          }
                          else
                          {
                            ViewModelLocator.DataService.UpdateExamFiles(XPSFile, XMLFile, VM.SelectedExam.id, XMLDoc.QAs.Count, Newtonsoft.Json.JsonConvert.SerializeObject(XMLDoc.QAs),VM.Remarks, VM.FileName);
                            ViewModelLocator.DialogService.ShowMessage("Master file was updated successfully.", false);
                          }
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
                      ViewModelLocator.DialogService.ShowMessage(t2.Exception.Message, true);
                  });
                }
              });
            }
          },
          () => ViewModelLocator.Auth.IsLoggedIn && 
                (ViewModelLocator.Auth.User.type == UserType.Admin || 
                ViewModelLocator.Auth.User.type == UserType.Uploader) 
                && this.AllXMLDocs.Count > 0);
        }

        return _UploadExamCommand;
      }
    }

    private void UpdateQAsProgressHandler(int i, int Total)
    {
      ViewModelLocator.Main.UpdateProgress(true, null, ((float)i / Total));

      GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
      {
        ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
        ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
      });
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
              IsExtractingQA = true;
              bool WrongFilesFlag = false;

              List<Task> Tasks = new List<Task>();
              foreach (var Doc in Docs)
              {
                var Ext = System.IO.Path.GetExtension(Doc);

                //Only .doc and .docx files are allowed to be added.
                if (Ext == ".doc" || Ext == ".docx")
                {
                  var Task = AddDocInternal(Doc);
                  Tasks.Add(Task);
                }
                else
                  WrongFilesFlag = true;
              }

              if (WrongFilesFlag)
                ViewModelLocator.DialogService.ShowMessage("One or more of the file(s) you selected are not Word documents (DOC or DOCX extension). These files have been skipped.", false);

              Task.WhenAll(Tasks.ToArray()).ContinueWith(t =>
              {
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                  IsExtractingQA = false;
                  this.IsDirty = true;
                });
              });
            }
          },
          () => !_IsProcessing);
        }

        return _AddDocsCommand;
      }
    }

    private Task AddDocInternal(string Doc, int? insertAt = null)
    {
      FileInfo FileInfo = new FileInfo(Doc);

      var NewDoc = new XMLDoc()
      {
        LastModified = FileInfo.LastWriteTimeUtc,
        Size = FileInfo.Length,
        SourcePath = Doc
      };

      if (insertAt != null)
        this.AllXMLDocs.Insert(insertAt.Value, NewDoc);
      else
        this.AllXMLDocs.Add(NewDoc);

      graph.AddVertex(NewDoc);

      //Populate QAs for the newly added document. This call is asynchronous.
      return UpdateQAs(NewDoc);
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
            this.AllXMLDocs.Remove(_SelectedDoc);

            this.IsDirty = true;
          },
          () => _SelectedDoc != null && !_IsProcessing && !_IsExtractingQA);
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
              try
              {
                this.Name = System.IO.Path.GetFileNameWithoutExtension(_SavePath);

                this.IsDirty = false;
                this._SavePath = TempSavePath;
                this.Name = Path.GetFileNameWithoutExtension(_SavePath);
                string ProjectXML = this.ToXML();
                File.WriteAllText(TempSavePath, ProjectXML);

                ViewModelLocator.Main.UpdateMRU(TempSavePath);

                SaveCommand.RaiseCanExecuteChanged();

                ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.MRU));

                RaisePropertyChanged(nameof(Name));
              }
              catch (Exception ee)
              {
                ViewModelLocator.DialogService.ShowMessage("The following error occurred while trying to save project: " + ee.Message, true);
              }
            }
          },
          () => this.IsDirty && !_IsProcessing && !_IsExtractingQA);
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

              if (!File.Exists(DocPath))
              {
                ViewModelLocator.DialogService.ShowMessage($"Source document '{DocPath}' does not exist. Cannot update QAs.", true);
                return;
              }

              //remove this doc from the graph and add again to clear old processed results
              graph.RemoveVertex(SelectedDoc);
              var MyIndex = this.AllXMLDocs.IndexOf(SelectedDoc);
              this.AllXMLDocs.Remove(SelectedDoc);

              //now add it again as a new doc
              AddDocInternal(DocPath, MyIndex);

              //and refresh results
              RaisePropertyChanged(nameof(Graph));

              IsDirty = true;
            }
          },
          () => !_IsProcessing && !_IsExtractingQA && _SelectedDoc != null);
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

            if (this.AllXMLDocs.All(d => d.IsSyncWithSource))
              ViewModelLocator.DialogService.ShowMessage("All documents are in sync with their sources.", false);
          },
          () => !IsExtractingQA);
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
            if (graph.Vertices.Any(v => !v.IsSyncWithSource))
            {
              if (!ViewModelLocator.DialogService.AskBooleanQuestion("One or more documents in this project are not synchronized with their source files. These documents will not be analyzed. Do you want to continue?"))
                return;
            }

            IsProcessing = true;

            //progress increment
            TotalComparisons = graph.Vertices.Sum(v1 => graph.Vertices.Sum(v2 => v2.QAs.Count) * v1.QAs.Count);
            CompletedComparisons = 0;
            Factor = 1 / TotalComparisons; //avoid computing constant factor over and over in the loop. just a small micro-optimization that I couldn't resist :)

            ViewModelLocator.Main.UpdateProgress(true, "Comparing QAs", 0);

            foreach (var V1 in graph.Vertices)
            {
              if (V1.IsSyncWithSource)
              {
                foreach (var V2 in graph.Vertices)
                {
                  if (V2.IsSyncWithSource)
                  {
                    if (V1.QAs != null && V2.QAs != null && !graph.ContainsEdge(V1, V2))
                    {
                      var Edge = new OurEdge(V1, V2, null);

                      if (graph.AddEdge(Edge))
                      {
                          var t = ViewModelLocator.DocComparer.Compare(V1, V2, ViewModelLocator.QAComparer, true, token);
                          Edge.Tag = t;
                          IsDirty = true;

                          GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                          {
                            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
                            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
                          });
                      }
                    }
                  }
                }
              }
            }

#if (DEBUG)
            if (UnitTestDetector.IsInUnitTest)
              IsProcessing = false;
            else
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => IsProcessing = false);
#else
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => IsProcessing = false);
#endif

            ViewModelLocator.Main.UpdateProgress(false, "Analysis completed", 1);

            RaisePropertyChanged(nameof(Graph));
            ApplyDiffThresholdCommand.Execute(null);
          },

          () => !_IsProcessing && !_IsExtractingQA);
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
            if (this.AllXMLDocs.Any(d => !d.IsSyncWithSource))
            {
              ViewModelLocator.DialogService.ShowMessage("Some of the documents in this project are not synchronized with source files. Refresh the project before using Export command.", true);
              return;
            }

            var ExportSavePath = ViewModelLocator.DialogService.ShowSave(MainVM.FILTER_XML_FILES);

            if (ExportSavePath != null)
            {
              //Create a clone of this project
              Project P = Project.FromXML(this.ToXML());

              P.Name = Path.GetFileNameWithoutExtension(ExportSavePath);
              P.SavePath = ExportSavePath;

              string DestFolder = Path.GetDirectoryName(ExportSavePath);
              foreach (var Doc in P.AllXMLDocs)
              {
                var DestFile = GetAvailableFileName(Doc.SourcePath, DestFolder);
                File.Copy(Doc.SourcePath, DestFile);
                Doc.SourcePath = Path.GetFileName(DestFile);
                Doc.LastModified = new FileInfo(DestFile).LastWriteTimeUtc;
              }

              P.IsDirty = false;
              File.WriteAllText(ExportSavePath, P.ToXML());
              ViewModelLocator.DialogService.ShowMessage($"Project has been exported successfully to '{ExportSavePath}'.", false);
            }
          },
          () => !_IsProcessing && !_IsExtractingQA);
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
            ViewModelLocator.DialogServiceMultiDF.OpenDiffWindow(row.Q1.Question, row.Q2.Question, row.Q1.Choices, row.Q2.Choices);
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
        ExportResultsCommand.RaiseCanExecuteChanged();
      }
    }

    private bool _IsExtractingQA = false;

    [XmlIgnore]
    public bool IsExtractingQA
    {
      get { return _IsExtractingQA; }
      private set
      {
        Set(ref _IsExtractingQA, value);

        CheckSyncWithSourceCommand.RaiseCanExecuteChanged();
        SaveCommand.RaiseCanExecuteChanged();
        ProcessCommand.RaiseCanExecuteChanged();
        AbortProcessCommand.RaiseCanExecuteChanged();
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
        UpdateQAsCommand.RaiseCanExecuteChanged();
        ExportCommand.RaiseCanExecuteChanged();
      }
    }

    #region "Serialize/De-serialize"
    private static readonly Type[] AllObjectTypes = {
      typeof(QA),
      typeof(XMLDoc),
      typeof(XMLDoc[]),
      typeof(DFResultRow),
      typeof(OurGraph),
      typeof(OurEdge),
      typeof(DFResult),
      typeof(DFResult[]),
    };

    private static readonly DataContractSerializer DSSerializer = new DataContractSerializer(typeof(Project), AllObjectTypes, 0x7F_FFFF, false, true, null); //max graph size is 8,388,607‬ items

    /// <summary>
    /// This setting is required to prevent exceptions that are thrown by the serialization when it encounters control characters used by
    /// Microsoft Word to denote newlines and non-breaking newlines.
    /// </summary>
    private static readonly XmlWriterSettings xmlWriterSettingsForWordDocs = new XmlWriterSettings()
    {
      NewLineChars = "\a\r\n",
      CheckCharacters = false,
      Encoding = Encoding.UTF8,
      NewLineHandling = NewLineHandling.Entitize,
    };

    public static Project FromXML(string xml)
    {
      try
      {
        //Make sure our input xml is up-to-date (file format has evolved over time, so we need to upgrade projects created using previous versions)
        xml = ProjectFileUpgrade.Upgrade(xml);

        StringReader s = new StringReader(xml.ToString());

        using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        {
          //var MyReader = new XmlReader(reader);

          return (Project)DSSerializer.ReadObject(reader, true);
        }
      }
      catch
      {
        return null;
      }
    }

    public string ToXML()
    {
      StringBuilder sb = new StringBuilder();

      StringWriter writer = new StringWriter(sb);
      using (XmlWriter xmlWriter = XmlWriter.Create(writer, xmlWriterSettingsForWordDocs))
      {
        DSSerializer.WriteStartObject(xmlWriter, this);
        DSSerializer.WriteObjectContent(xmlWriter, this);
        DSSerializer.WriteEndObject(xmlWriter);
      }

      return sb.ToString();
    }
    #endregion

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

        ViewModelLocator.DocComparer.QACompared -= DocComparer_QACompared;
        ViewModelLocator.DocComparer.QASkipped -= DocComparer_QASkipped;
        ViewModelLocator.DocComparer.DocCompareCompleted -= DocComparer_DocCompareCompleted;
      }
    }

    private RelayCommand _OpenSourceCommand;
    public RelayCommand OpenSourceCommand
    {
      get
      {
        if (_OpenSourceCommand == null)
        {
          _OpenSourceCommand = new RelayCommand(() =>
          {
            if (!string.IsNullOrEmpty(SelectedDoc.SourcePath) && File.Exists(SelectedDoc.SourcePath))
              ViewModelLocator.WordService.OpenDocument(SelectedDoc.SourcePath, false, null, null);
            else
              ViewModelLocator.DialogService.ShowMessage("Source document does not exist.", true);
          },
          () => SelectedDoc != null && !string.IsNullOrEmpty(SelectedDoc.SourcePath) && File.Exists(SelectedDoc.SourcePath));
        }

        return _OpenSourceCommand;
      }
    }

    private Task UpdateQAs(XMLDoc xmldoc)
    {
      if (!string.IsNullOrEmpty(xmldoc.SourcePath) && File.Exists(xmldoc.SourcePath))
      {
        return Task.Run(() =>
        {
          ViewModelLocator.Main.UpdateProgress(true, "Importing...", 0);

          var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(xmldoc.SourcePath, xmldoc._Token, (i, Total) =>
          {
            xmldoc.ProcessingProgress = (i / (float)Total) * 100;

            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
            ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
          });

          if (Paragraphs != null)
          {
            try
            {
              xmldoc.QAs = ViewModelLocator.QAExtractionStrategy.ExtractQAs(Paragraphs, xmldoc._Token);
            }
            catch (Exception ex)
            {
              if (ex.Data.Contains("Paragraph"))
              {
                var Res = ViewModelLocator.DialogService.AskBooleanQuestion(ex.Message + Environment.NewLine + Environment.NewLine + "Do you want to open source document to fix this problem?");
                if (Res)
                {
                  ViewModelLocator.WordService.OpenDocument(xmldoc.SourcePath, false, (int)ex.Data["Paragraph"], (int)ex.Data["Paragraph"]);
                }
              }
            }

            if (xmldoc.QAs != null)
            {
              foreach (var QA in xmldoc.QAs)
                QA.Doc = xmldoc;
            }

            xmldoc.RaisePropertyChanged(nameof(XMLDoc.QAs));
          }
        });
      }
      else
        return null;
    }
  }
}