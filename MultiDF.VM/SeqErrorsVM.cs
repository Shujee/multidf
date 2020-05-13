using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiDF.VM
{
  public class SeqErrorsVM : ObservableObject
  {
    private string _DocPath;
    private List<WordParagraph> _DelimiterParagraphs;

    public SeqErrorsVM(string docPath)
    {
      _DocPath = docPath;
    }

    private ObservableCollection<SeqError> _Errors = new ObservableCollection<SeqError>();
    public ObservableCollection<SeqError> Errors 
    {
      get => _Errors;
      set => Set(ref _Errors, value);
    }

    private RelayCommand<SeqError> _GoToErrorCommand;
    public RelayCommand<SeqError> GoToErrorCommand
    {
      get
      {
        if (_GoToErrorCommand == null)
        {
          _GoToErrorCommand = new RelayCommand<SeqError>((err) =>
          {
            //This document is already open. We are only interested in moving the caret to err.Start.
            ViewModelLocator.WordService.OpenDocument(_DocPath, false, err.Start, err.Start);
          },
          (err) => true);
        }

        return _GoToErrorCommand;
      }
    }


    private RelayCommand _AnalyzeCommand;
    public RelayCommand AnalyzeCommand
    {
      get
      {
        if (_AnalyzeCommand == null)
        {
          _AnalyzeCommand = new RelayCommand(() =>
          {
            ViewModelLocator.Main.UpdateProgress(true, "Analyzing...", 0);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(_DocPath, tokenSource.Token, (i, Total) =>
            {
              ViewModelLocator.Main.UpdateProgress(false, "Extracting content...", (i / (float)Total) * 100);

              ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
              ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
            }, true);

            if (Paragraphs != null)
            {
              try
              {
                ViewModelLocator.Main.UpdateProgress(false, "Locating QA delimeters", 40);

                _DelimiterParagraphs = ViewModelLocator.QAExtractionStrategy.ExtractDelimiterParagraphs(Paragraphs, tokenSource.Token, false);

                ViewModelLocator.Main.UpdateProgress(false, "Comparing sequence numbers", 60);

                //Report how many of the QAs have wrong sequence numbers. Our definition of "wrong" is "any QA whose sequence number is not N + 1, where N is the sequence number of previous QA".
                //This dictionary will store out-of-sequence paragraphs and with their expected sequence number.
                Errors.Clear();

                //First QA should have sequence number 1
                int? PrevSeqNumber = ViewModelLocator.QAExtractionStrategy.ParseQuestionNumber(_DelimiterParagraphs[0].Text);
                if (PrevSeqNumber == null || PrevSeqNumber != 1)
                  Errors.Add(new SeqError() { Start = _DelimiterParagraphs[0].Start, Page = _DelimiterParagraphs[0].StartPage, Index = PrevSeqNumber ?? 0, ExpectedIndex = 1 });

                for (int i = 1; i < _DelimiterParagraphs.Count; i++)
                {
                  var Para = _DelimiterParagraphs[i];

                  int? SeqNumber = ViewModelLocator.QAExtractionStrategy.ParseQuestionNumber(Para.Text);

                  if (SeqNumber == null || SeqNumber != PrevSeqNumber.Value + 1)
                  {
                    Errors.Add(new SeqError() { Start = Para.Start, Page = Para.StartPage, Index = SeqNumber ?? 0, ExpectedIndex = PrevSeqNumber.Value + 1 });
                    PrevSeqNumber = PrevSeqNumber.Value + 1;
                  }
                  else
                    PrevSeqNumber = SeqNumber;
                }

                ViewModelLocator.Main.UpdateProgress(false, "Done", 100);

                if (Errors.Count == 0)
                  ViewModelLocator.DialogService.ShowMessage("This document does not appear to have any sequencing problems.", false);
              }
              catch (Exception ex)
              {
                if (ex.Data.Contains("Paragraph"))
                {
                  var Res = ViewModelLocator.DialogService.AskBooleanQuestion(ex.Message + Environment.NewLine + Environment.NewLine + "Do you want to open source document to fix this problem?");
                }
              }
            }

            FixAutomaticCommand.RaiseCanExecuteChanged();
          },
          () => true);
        }

        return _AnalyzeCommand;
      }
    }

    private RelayCommand _FixAutomaticCommand;
    public RelayCommand FixAutomaticCommand
    {
      get
      {
        if (_FixAutomaticCommand == null)
        {
          _FixAutomaticCommand = new RelayCommand(() =>
          {
            var Fixes = ViewModelLocator.WordService.FixAllQANumbers(_DocPath, _DelimiterParagraphs);

            foreach(var Fix in Fixes)
            {
              var FixedError = Errors.FirstOrDefault(e => e.Index == Fix.OldIndex);

              if (FixedError != null)
                Errors.Remove(FixedError);
            }

            ViewModelLocator.DialogService.ShowMessage($"Operation completed. {Fixes.Count} sequence errors were corrected. These QA numbers have been removed from the list.", false);
          },
          () => _Errors != null && _Errors.Count > 0);
        }

        return _FixAutomaticCommand;
      }
    }

    //private RelayCommand _GoToNextIncorrectDelimeterCommand;
    //public RelayCommand GoToNextIncorrectDelimeterCommand
    //{
    //  get
    //  {
    //    if (_GoToNextIncorrectDelimeterCommand == null)
    //    {
    //      _GoToNextIncorrectDelimeterCommand = new RelayCommand(() =>
    //      {
    //        if (ViewModelLocator.WordService.ActiveDocumentPath == null)
    //          ViewModelLocator.DialogService.ShowMessage("There is no active document.", true);
    //        else
    //        {
    //          if (NextIncorrectDelimiter_DocPath != ViewModelLocator.WordService.ActiveDocumentPath)
    //          {
    //            CancellationTokenSource tokenSource = new CancellationTokenSource();

    //            if (ViewModelLocator.DialogService.AskBooleanQuestion("MultiDF will now extract paragraphs data from the active document. This is a one-time process and can take some time for large documents. Subsequent calls to this command will use extracted data and will not perform extraction again. Continue?"))
    //            {
    //              NextIncorrectDelimiter_DocPath = ViewModelLocator.WordService.ActiveDocumentPath;
    //              NextIncorrectDelimiter_AllParagraphs = ViewModelLocator.WordService.GetDocumentParagraphs(ViewModelLocator.WordService.ActiveDocumentPath, tokenSource.Token, (i, Total) =>
    //              {
    //                ViewModelLocator.Main.UpdateProgress(false, "Extracting content...", (i / (float)Total) * 100);

    //                ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
    //                ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
    //              }, true);
    //            }
    //            else
    //              return;
    //          }

    //          GoToNextIncorrectDelimiter();
    //        }
    //      },
    //      () => ViewModelLocator.WordService.ActiveDocumentPath != null);
    //    }

    //    return _GoToNextIncorrectDelimeterCommand;
    //  }
    //}

    //private void GoToNextIncorrectDelimiter()
    //{
    //  if (NextIncorrectDelimiter_AllParagraphs != null)
    //  {
    //    //Get paragraph number from current cursor location
    //    var CurParaNumber = ViewModelLocator.WordService.CurrentParagraphNumber;

    //    if (CurParaNumber != null)
    //    {
    //      WordParagraph NextErr = null;

    //      try
    //      {
    //        NextErr = ViewModelLocator.QAExtractionStrategy.ExtractNearestIncorrectDelimiterParagraphs(NextIncorrectDelimiter_AllParagraphs, CurParaNumber.Value);
    //      }
    //      catch (Exception ee)
    //      {
    //        ViewModelLocator.DialogService.ShowMessage(ee.Message, false);
    //        NextIncorrectDelimiter_AllParagraphs = null;
    //        NextIncorrectDelimiter_DocPath = null;
    //        return;
    //      }

    //      if (NextErr != null)
    //        ViewModelLocator.WordService.OpenDocument(NextIncorrectDelimiter_DocPath, false, NextErr.Start, NextErr.Start);
    //      else
    //        ViewModelLocator.DialogService.ShowMessage("Cannot find any sequence number problems below cursor position.", false);
    //    }
    //    else
    //      ViewModelLocator.DialogService.ShowMessage("Cannot determine current paragraph number.", true);
    //  }
    //  else
    //    ViewModelLocator.DialogService.ShowMessage("No paragraphs were extracted from the active document.", true);
    //}
  }

  public class SeqError : ObservableObject
  {
    private int _Page;
    public int Page
    {
      get => _Page;
      set => Set(ref _Page, value);
    }


    private int _Index;
    public int Index
    {
      get => _Index;
      set => Set(ref _Index, value);
    }


    private int _ExpectedIndex;
    public int ExpectedIndex
    {
      get => _ExpectedIndex;
      set => Set(ref _ExpectedIndex, value);
    }

    private int _Start;
    public int Start
    {
      get => _Start;
      set => Set(ref _Start, value);
    }
  }
}
