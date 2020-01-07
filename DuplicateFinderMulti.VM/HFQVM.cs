using DuplicateFinderMultiCommon;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HFQOModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DuplicateFinderMulti.VM
{
  public class HFQVM : ViewModelBase
  {
    public event Action<HFQResultRowVM> NewResultRowAdded;

    public HFQVM()
    {
      Result = new ObservableCollection<HFQResultRowVM>();

      ViewModelLocator.Auth.PropertyChanged += (sender, e) =>
      {
        if (e.PropertyName == nameof(ViewModelLocator.Auth.IsLoggedIn))
        {
          OpenExamCommand.RaiseCanExecuteChanged();

          if(ViewModelLocator.Auth.IsLoggedIn)
          {
            ViewModelLocator.Main.UpdateProgress(true, "Fetching master files list", 0);
            ViewModelLocator.DataService.GetExamsDL().ContinueWith(t2 => {
              MyExams = t2.Result;
              RaisePropertyChanged(nameof(MyExams));
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => ViewModelLocator.Main.UpdateProgress(true, "Ready", 0));
            });
          }
          else
          {
            XMLDoc = null;
            XPSPath = null;
          }
        }
      };
    }

    /// <summary>
    /// Important: ID field contains Access Id, not Exam Id.
    /// </summary>
    public Dictionary<string, string> MyExams { get; private set; }

    private string _SelectedAccess;
    public string SelectedAccess
    {
      get => _SelectedAccess;
      set => Set(ref _SelectedAccess, value);
    }


    private int? _ExamID;
    public int? ExamID
    {
      get => _ExamID;
      set
      {
        Set(ref _ExamID, value);
        UploadResultCommand.RaiseCanExecuteChanged();
      }
    }

    private string _XPSPath;
    public string XPSPath
    {
      get => _XPSPath;
      set => Set(ref _XPSPath, value);
    }
    
    private XMLDoc _XML;
    public XMLDoc XMLDoc
    {
      get => _XML;
      set => Set(ref _XML, value);
    }

    private string _SearchText;
    public string SearchText
    {
      get => _SearchText;
      set => Set(ref _SearchText, value);
    }

    public ObservableCollection<HFQResultRowVM> Result { get; private set; }

    private int _SelectedResultIndex;
    public int SelectedResultIndex
    {
      get => _SelectedResultIndex;
      set
      {
        Set(ref _SelectedResultIndex, value);
        GoToPreviousCommand.RaiseCanExecuteChanged();
        GoToNextCommand.RaiseCanExecuteChanged();
      }
    }

    private RelayCommand _OpenExamCommand;
    public RelayCommand OpenExamCommand
    {
      get
      {
        if (_OpenExamCommand == null)
        {
          _OpenExamCommand = new RelayCommand(() =>
          {
            if(Result.Count > 0)
            {
              var SaveChanges = ViewModelLocator.DialogService.AskTernaryQuestion("Upload current result before opening new master file?");

              if (SaveChanges == null)
                return;
              else if(SaveChanges.Value)
              {
                //upload results
              }
            }

            var Res = ViewModelLocator.DialogService.ShowExamsListDialog();

            if (Res)
            {
              if (_SelectedAccess != null)
              {
                try
                {
                  ViewModelLocator.Auth.IsCommunicating = true;
                  var MF = ViewModelLocator.DataService.DownloadExam(int.Parse(_SelectedAccess), Environment.MachineName);

                  if (MF != null)
                  {
                    ExamID = MF.exam_id;

                    var XPSBytes = Encryption.Decrypt(Convert.FromBase64String(MF.xps));
                    var XPSFilePath = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllBytes(XPSFilePath, XPSBytes);
                    XPSPath = XPSFilePath;

                    var XMLString = Encoding.UTF8.GetString(Encryption.Decrypt(Convert.FromBase64String(MF.xml)));
                    XMLDoc = XMLDoc.FromXML(XMLString);

                    Result.Clear();
                    RaisePropertyChanged(nameof(Result));

                    var Row = new HFQResultRowVM() { Q = 1 };
                    Result.Add(Row);
                    NewResultRowAdded?.Invoke(Row);

                    SelectedResultIndex = 0;
                  }
                }
                finally
                {
                  ViewModelLocator.Auth.IsCommunicating = false;
                }
              }
              else
                ViewModelLocator.DialogService.ShowMessage("You did not select a master file to download.", false);
            }
          },
          () => ViewModelLocator.Auth.IsLoggedIn);
        }

        return _OpenExamCommand;
      }
    }


    private RelayCommand<QA> _MarkCommand;
    public RelayCommand<QA> MarkCommand
    {
      get
      {
        if (_MarkCommand == null)
        {
          _MarkCommand = new RelayCommand<QA>((qa) =>
          {
            if(SelectedResultIndex >= 0)
            {
              var R = Result[SelectedResultIndex];

              if (R.A1 == null)
                R.A1 = qa.Index;
              else if (R.A2 == null)
                R.A2 = qa.Index;
              else if (R.A3 == null)
                R.A3 = qa.Index;
              else
                ViewModelLocator.DialogService.ShowMessage("This question has already been assign 3 matches.", false);
            }
          },
          (qa) => true);
        }

        return _MarkCommand;
      }
    }

    private RelayCommand _GoToNextCommand;
    public RelayCommand GoToNextCommand
    {
      get
      {
        if (_GoToNextCommand == null)
        {
          _GoToNextCommand = new RelayCommand(() =>
          {
            //if current record is the last record, then GoToNext command will create a  new record
            if (SelectedResultIndex == Result.Count - 1)
            {
              var Row = new HFQResultRowVM() { Q = Result[SelectedResultIndex].Q + 1 };
              Result.Add(Row);
              NewResultRowAdded?.Invoke(Row);
            }

            SelectedResultIndex++;
          },
          () => Result!=null && SelectedResultIndex >= 0);
        }

        return _GoToNextCommand;
      }
    }

    private RelayCommand _GoToPreviousCommand;
    public RelayCommand GoToPreviousCommand
    {
      get
      {
        if (_GoToPreviousCommand == null)
        {
          _GoToPreviousCommand = new RelayCommand(() =>
          {
            SelectedResultIndex--;
          },
          () => Result != null && SelectedResultIndex > 0);
        }

        return _GoToPreviousCommand;
      }
    }


    private RelayCommand _UploadResultCommand;
    public RelayCommand UploadResultCommand
    {
      get
      {
        if (_UploadResultCommand == null)
        {
          _UploadResultCommand = new RelayCommand(() =>
          {
            if(ViewModelLocator.DialogService.AskBooleanQuestion("Are you sure you want to upload result to the server?"))
            {
              ViewModelLocator.DataService.UploadResult(ExamID.Value, Environment.MachineName, Result.Select(r => r.ToHFQResultRow()));
            }
          },
          () => ExamID != null);
        }

        return _UploadResultCommand;
      }
    }

  }
}
