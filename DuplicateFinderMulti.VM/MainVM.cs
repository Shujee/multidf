using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class MainVM : ViewModelBase
  {
    public const string FILTER_IMAGE_FILES_ALL_FILES = "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg;*.png;*.gif|All Files (*.*)|*.*";
    public const string FILTER_XML_FILES = "XML Files (*.xml)|*.xml";

    public MainVM()
    {
      if (IsInDesignModeStatic)
      {
        _SelectedProject = new Project()
        {
          IsDirty = false,
          Name = "Project 1",
          XMLDocs = new System.Collections.ObjectModel.ObservableCollection<XMLDoc>()
        };

        _SelectedProject.XMLDocs.Add(new XMLDoc()
        {
          Name = "Sample Question for DF Multi",
          LastModified = System.DateTime.Now.AddDays(-1),
          Size = 123456,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\Sample Question for DF Multi.docx",
        });

        _SelectedProject.XMLDocs.Add(new XMLDoc()
        {
          Name = "File 1 for DF.docx",
          LastModified = System.DateTime.Now.AddDays(-8),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 1 for DF.docx",
        });

        _SelectedProject.XMLDocs.Add(new XMLDoc()
        {
          Name = "File 2 for DF.docx",
          LastModified = System.DateTime.Now.AddDays(-15),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 2 for DF.docx",
        });

        _SelectedProject.XMLDocs.Add(new XMLDoc()
        {
          Name = "File 3 for DF.docx",
          LastModified = System.DateTime.Now.AddDays(-1),
          Size = 123456,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\File 3 for DF.docx",
        });

        _SelectedProject.XMLDocs.Add(new XMLDoc()
        {
          Name = "Doc that does not exist.docx",
          LastModified = System.DateTime.Now.AddDays(-8),
          Size = 6487987,
          SourcePath = @"F:\Office\Larry Gong\DuplicateFinder\Analysis\Doc that does not exist.docx",
        });
      }

      NewCommand.Execute(null);
    }

    protected Project _SelectedProject;
    public Project SelectedProject
    {
      get => _SelectedProject;
      set
      {
        //unbind old project's event listener
        if(_SelectedProject!=null)
          _SelectedProject.PropertyChanged -= _SelectedProject_PropertyChanged;

        Set(ref _SelectedProject, value);

        //bind new project's event listener
        if (_SelectedProject != null)
          _SelectedProject.PropertyChanged -= _SelectedProject_PropertyChanged;
      }
    }

    private void _SelectedProject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(Project.IsDirty))
      {
        NewCommand.RaiseCanExecuteChanged();
        OpenCommand.RaiseCanExecuteChanged();
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
            SelectedProject = new Project() { Name = "New Project" };
            _SelectedProject.SavePath = null;
          },
          () => _SelectedProject?.IsDirty??false == false);
        }

        return _NewCommand;
      }
    }

    private RelayCommand _OpenCommand;
    public RelayCommand OpenCommand
    {
      get
      {
        if (_OpenCommand == null)
        {
          _OpenCommand = new RelayCommand(() =>
          {
            var ProjectPath = ViewModelLocator.DialogService.ShowOpen(FILTER_XML_FILES);

            if (ProjectPath != null)
            {
              var ProjectXML = System.IO.File.ReadAllText(ProjectPath);
              SelectedProject = Project.FromXML(ProjectXML);
              _SelectedProject.SavePath = ProjectPath;
            }
          },
          () => ((_SelectedProject?.IsDirty)??false) == false);
        }

        return _OpenCommand;
      }
    }

    #region "Status"
    private bool _IsProcessing = false;
    public bool IsProcessing
    {
      get { return _IsProcessing; }
    }

    private string _ProgressMessage;
    public string ProgressMessage
    {
      get { return _ProgressMessage; }
      set { Set(ref _ProgressMessage, value); }
    }

    private double _ProgressValue;
    public double ProgressValue
    {
      get { return _ProgressValue; }
      set { Set(ref _ProgressValue, value); }
    }

    void UpdateStatusLabel(int currentStep, int totalSteps, string statusText, bool newMessage)
    {
      GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
      {
        if (newMessage)
          ProgressMessage = statusText;
        else
          ProgressMessage = $"{statusText} (step {currentStep} of {totalSteps})";

        ProgressValue = (100 * ((currentStep - 1) / (float)totalSteps));
      });
    }
    #endregion

    //public int MaxDistance
    //{
    //  get { return Properties.Settings.Default.MaxDistance; }
    //  set
    //  {
    //    Properties.Settings.Default.MaxDistance = value;
    //    Properties.Settings.Default.Save();
    //    RaisePropertyChanged(nameof(MaxDistance));
    //  }
    //}

    //public int MinParaLength
    //{
    //  get { return Properties.Settings.Default.MinParaLength; }
    //  set
    //  {
    //    Properties.Settings.Default.MinParaLength = value;
    //    Properties.Settings.Default.Save();
    //    RaisePropertyChanged(nameof(MinParaLength));
    //  }
    //}

    //private List<WordParagraph[]> _Content;
    //public List<WordParagraph[]> Content
    //{
    //  get { return _Content; }
    //  set { Set(ref _Content, value); }
    //}

    private CancellationTokenSource source;
    private CancellationToken tok;
    private Stopwatch _stopwatch = new Stopwatch();

    private RelayCommand<bool> _StartCommand;

    /// <summary>
    /// Processes active Word document by splitting its text and then searching for similar chunks.
    /// If command parameter is true, then document text is split using question number. If command
    /// parameter is false, then splitting is done on new line character.
    /// </summary>
    //public RelayCommand<bool> StartCommand
    //{
    //  get
    //  {
    //    if (_StartCommand == null)
    //    {
    //      _StartCommand = new RelayCommand<bool>((fullQA) =>
    //      {
    //        source = new CancellationTokenSource();
    //        tok = source.Token;

    //        _IsProcessing = true;
    //        base.RaisePropertyChanged(nameof(VM.MainVM.IsProcessing));
    //        StartCommand.RaiseCanExecuteChanged();
    //        StopCommand.RaiseCanExecuteChanged();

    //        Task.Run((() =>
    //        {
    //          _stopwatch.Start();

    //          UpdateStatusLabel(0, 100, "Fetching paragraphs", true);

    //          List<WordParagraph> WPs;

    //          //Fetch all paragraphs from active Word document
    //          List<WordParagraph> Paras = FetchAllParagraphs();

    //          if (fullQA)
    //          {
    //            //Document being processed contains MCQs. Each MCQ starts with Question Number on a single line, followed by Question text on the next line
    //            //followed by multiple answers, each on a separate line. In FullQA mode, we need to group these lines (paragraphs) such that one question and all its
    //            //answers are in one group. Here we do that.

    //            //This RegEx will find Question Number paragraphs
    //            System.Text.RegularExpressions.Regex RE_QNumberWithHardReturn = new System.Text.RegularExpressions.Regex(@"^((\d+\.)|([Q]\d+\.?))\s*[\r\n\x0B]", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);

    //            WPs = new List<WordParagraph>();

    //            System.Text.RegularExpressions.Match M2;
    //            int Start = 0;
    //            int i = 0;

    //            //This loop will mark the start and end of each QA. We simply iterate through the paragraphs and try to locate a paragraph that
    //            //marks the beginning of a new QA. This paragraph will have question number followed by a period followed by a (hard or soft) line break.
    //            //When we find such a paragraph, we mark it as the beginning of a new QA and continue our loop. When next QA beginning is found, we 
    //            //mark the end of previous QA. The process continues till the end of list.
    //            while (i < Paras.Count - 1)
    //            {
    //              do
    //              {
    //                i++;
    //                M2 = RE_QNumberWithHardReturn.Match(Paras[i].Text);
    //              }
    //              while (!M2.Success && i < Paras.Count - 1);

    //              if (i == Paras.Count - 1)
    //              {
    //                M2 = RE_QNumberWithHardReturn.Match(Paras[i].Text);
    //                if (!M2.Success)
    //                  WPs.Add(new WordParagraph() { Start = Start, End = Paras[i].End, Text = ViewModelLocator.WordService.GetRangeText(Start, Paras[i].End) });
    //                else
    //                  WPs.Add(new WordParagraph() { Start = Start, End = Paras[i - 1].End, Text = ViewModelLocator.WordService.GetRangeText(Start, Paras[i - 1].End) });
    //              }
    //              else
    //                WPs.Add(new WordParagraph() { Start = Start, End = Paras[i - 1].End, Text = ViewModelLocator.WordService.GetRangeText(Start, Paras[i - 1].End) });

    //              if (i < Paras.Count)
    //                Start = Paras[i].Start;

    //              tok.ThrowIfCancellationRequested();
    //            }

    //            UpdateStatusLabel(20, 100, "Recognizing questions and answers", true);
    //          }
    //          else
    //            WPs = Paras;

    //          //Now call our Duplicate Finder Service to take this data and try to locate duplicate records.
    //          if (!tok.IsCancellationRequested)
    //          {
    //            Content = ViewModelLocator.DupFinderService.Find(
    //                                                    WPs,
    //                                                    MaxDistance,
    //                                                    Properties.Settings.Default.CaseSensitiveComparison,
    //                                                    (fullQA ? null : (int?)Properties.Settings.Default.MinParaLength),
    //                                                    UpdateStatusLabel,
    //                                                    tok
    //                                                  );

    //            GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
    //            {
    //              ClearContentCommand.RaiseCanExecuteChanged();
    //            });
    //          }

    //        }), tok).ContinueWith((t) =>
    //                  {
    //                    GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI((() =>
    //                    {
    //                      _IsProcessing = false;
    //                      RaisePropertyChanged(nameof(IsProcessing));
    //                      StartCommand.RaiseCanExecuteChanged();
    //                      StopCommand.RaiseCanExecuteChanged();

    //                      _stopwatch.Stop();

    //                      if (t.IsCanceled)
    //                        ViewModelLocator.DialogService.ShowMessage("Processing aborted.", false);
    //                      else
    //                        ViewModelLocator.DialogService.ShowMessage($"Processing completed. {Content.Count} duplicates were found. Completed in {_stopwatch.Elapsed.TotalSeconds.ToString("0.00")} seconds.", false);

    //                      _stopwatch.Reset();

    //                      ProgressMessage = "Ready";
    //                      ProgressValue = 0;
    //                    }));
    //                  });
    //      },
    //      ((fullQA) => !_IsProcessing && ViewModelLocator.Register.IsRegistered));
    //    }

    //    return _StartCommand;
    //  }
    //}

    //private RelayCommand _StopCommand;
    //public RelayCommand StopCommand
    //{
    //  get
    //  {
    //    if (_StopCommand == null)
    //    {
    //      _StopCommand = new RelayCommand(() =>
    //      {
    //        //If process is already running, user wants to cancel it
    //        if (_IsProcessing)
    //        {
    //          source.Cancel();
    //          _IsProcessing = false;

    //          base.RaisePropertyChanged(nameof(IsProcessing));
    //          StartCommand.RaiseCanExecuteChanged();
    //          StopCommand.RaiseCanExecuteChanged();
    //        }
    //      },
    //      () => _IsProcessing);
    //    }

    //    return _StopCommand;
    //  }
    //}

    //private List<WordParagraph> FetchAllParagraphs()
    //{
    //  //Get all paragraphs from Word
    //  var TempParas = ViewModelLocator.WordService.GetActiveDocumentParagraphs();
    //  var ParaCount = ViewModelLocator.WordService.GetActiveDocumentParagraphsCount();
    //  var Paras = new List<WordParagraph>();
    //  int i = 0;
    //  foreach (var P in TempParas)
    //  {
    //    Paras.Add(P);
    //    UpdateStatusLabel((int)((++i / (float)ParaCount) * 100), 100, "Fetching paragraphs", false);
    //    tok.ThrowIfCancellationRequested();
    //  }

    //  return Paras;
    //}

    //private RelayCommand _ClearContentCommand;
    //public RelayCommand ClearContentCommand
    //{
    //  get
    //  {
    //    if (_ClearContentCommand == null)
    //    {
    //      _ClearContentCommand = new RelayCommand(() =>
    //      {
    //        Content.Clear();
    //        Content = null;
    //        RaisePropertyChanged(nameof(Content));
    //        ClearContentCommand.RaiseCanExecuteChanged();
    //      },
    //      () => Content != null && Content.Count > 0);
    //    }

    //    return _ClearContentCommand;
    //  }
    //}

    //private RelayCommand<WordParagraph> _OpenDiffCommand;
    //public RelayCommand<WordParagraph> OpenDiffCommand
    //{
    //  get
    //  {
    //    if (_OpenDiffCommand == null)
    //    {
    //      _OpenDiffCommand = new RelayCommand<WordParagraph>((para) =>
    //      {
    //        var MyGroup = Content.First(group => group.Contains(para));

    //        if(MyGroup!=null && MyGroup.Length > 1)
    //        {
    //          ViewModelLocator.DialogService.OpenDiffWindow(MyGroup[0].Text, para.Text);
    //        }
    //      },
    //      (para) => true);
    //    }

    //    return _OpenDiffCommand;
    //  }
    //}

    //private RelayCommand<WordParagraph> _GoToParagraphCommand;
    //public RelayCommand<WordParagraph> GoToParagraphCommand
    //{
    //  get
    //  {
    //    if (_GoToParagraphCommand == null)
    //    {
    //      _GoToParagraphCommand = new RelayCommand<WordParagraph>((para) =>
    //      {
    //        ViewModelLocator.WordService.SelectRange(para.Start, para.End);
    //      },
    //      (para) => true);
    //    }

    //    return _GoToParagraphCommand;
    //  }
    //}
  }
}