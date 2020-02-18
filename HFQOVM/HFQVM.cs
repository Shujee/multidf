using Common;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using VMBase;

namespace HFQOVM
{
  public class HFQVM : VMBase.MainBase
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
              UpdateProgress(true, "Fetching master files list", 0);
            ViewModelLocator.DataService.GetExamsDL().ContinueWith(t2 => {
              if (!t2.IsFaulted)
              {
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                  UpdateProgress(true, "Ready", 0);
                  MyExams = t2.Result;
                  RaisePropertyChanged(nameof(MyExams));

                  if (MyExams.Length > 0)
                    SelectedAccess = MyExams[0];
                });
              }
            });
          }
          else
          {
            SelectedAccess = null;

            Result.Clear();
            RaisePropertyChanged(nameof(Result));

            SelectedResultIndex = 0;
            SearchText = "";

            XPSPath = "";
            XMLDoc = null;
          }
        }
      };
    }

    /// <summary>
    /// Important: ID field contains Access Id, not Exam Id.
    /// </summary>
    public AccessibleMasterFile[] MyExams { get; private set; }

    private AccessibleMasterFile _SelectedAccess;
    public AccessibleMasterFile SelectedAccess
    {
      get => _SelectedAccess;
      set => Set(ref _SelectedAccess, value);
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

    private bool _IsBusy;
    public bool IsBusy
    {
      get => _IsBusy;
      set => Set(ref _IsBusy, value);
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
            if(Result != null && Result.Count > 0)
            {
              var SaveChanges = ViewModelLocator.DialogService.AskTernaryQuestion("Upload current result before opening new master file?");

              if (SaveChanges == null)
                return;
              else if(SaveChanges.Value)
              {
                UploadResultCommand.Execute(null);
                //upload results
              }
            }

            IsBusy = true;

            ViewModelLocator.DataService.GetExamsDL().ContinueWith(t2 =>
            {
              IsBusy = false;

              if (t2.IsFaulted)
              {
                ViewModelLocator.DialogService.ShowMessage("Could not fetch exams list from the server. Please try again later. The error message is: " + t2.Exception.Message, true);
              }
              else
              {
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                  UpdateProgress(true, "Ready", 0);
                  MyExams = t2.Result;
                  RaisePropertyChanged(nameof(MyExams));

                  var Res = ViewModelLocator.DialogServiceHFQ.ShowExamsListDialog();

                  if (Res)
                  {
                    if (_SelectedAccess != null)
                    {
                      try
                      {
                        IsBusy = true;

                        ViewModelLocator.Auth.IsCommunicating = true;

                        MasterFile MF = null;

                        try
                        {
                          MF = ViewModelLocator.DataService.DownloadExam(_SelectedAccess.access_id, Environment.MachineName);
                        }
                        catch (Exception ee)
                        {
                          ViewModelLocator.DialogService.ShowMessage(ee);
                          return;
                        }
                        
                        if (MF != null)
                        {
                          var XPSBytes = Encryption.Decrypt(Convert.FromBase64String(MF.xps));

                          //Store this downloaded data into an isolated storage file
                          IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                          var XPSFileName = DateTime.Now.ToString("yyyyMMHHmmss") + $"_{MF.id}.xps";
                          string XPSFilePath;
                          using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(XPSFileName, FileMode.CreateNew, isoStore))
                          {
                            isoStream.Write(XPSBytes, 0, XPSBytes.Length);
                            XPSFilePath = isoStream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isoStream).ToString();
                            //isoStore.DeleteFile(_XPSPath);
                          }

                          var XMLString = Encoding.UTF8.GetString(Encryption.Decrypt(Convert.FromBase64String(MF.xml)));
                          XMLDoc = XMLDoc.FromXML(XMLString);
                          XPSPath = XPSFilePath;

                          Result.Clear();
                          RaisePropertyChanged(nameof(Result));

                          var Row = new HFQResultRowVM() { Q = 1 };
                          Result.Add(Row);
                          NewResultRowAdded?.Invoke(Row);

                          SelectedResultIndex = 0;
                        }
                      }
                      catch (Exception ee)
                      {
                        ViewModelLocator.DialogService.ShowMessage(ee.Message, true);
                      }
                      finally
                      {
                        ViewModelLocator.Auth.IsCommunicating = false;
                        IsBusy = false;
                      }
                    }
                    else
                      ViewModelLocator.DialogService.ShowMessage("You did not select a master file to download.", false);
                  }
                });
              }
            });
          },
          () => ViewModelLocator.Auth.IsLoggedIn && (ViewModelLocator.Auth.User.type == UserType.Admin || ViewModelLocator.Auth.User.type == UserType.Downloader));
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

              if (Result.Any(r => r.A1 == qa.Index || r.A2 == qa.Index || r.A3 == qa.Index))
              {
                ViewModelLocator.DialogService.ShowMessage($"This question has already been marked previously. You cannot use it again.", false);
              }
              else
              {
                if (R.A1 == null)
                  R.A1 = qa.Index;
                else if (R.A2 == null)
                  R.A2 = qa.Index;
                else if (R.A3 == null)
                  R.A3 = qa.Index;
                else
                  ViewModelLocator.DialogService.ShowMessage("This question has already been assign 3 matches.", false);
              }
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
            //if current record is the last record or there is no empty record below the current record, then GoToNext command will create a  new record
            var NearestEmpty = Result.Skip(SelectedResultIndex + 1).FirstOrDefault(r => r.A1 == null);

            if (SelectedResultIndex == Result.Count - 1 || NearestEmpty == null)
            {
              var Row = new HFQResultRowVM() { Q = Result.Last().Q + 1 };
              Result.Add(Row);
              NewResultRowAdded?.Invoke(Row);
              SelectedResultIndex = Result.Count - 1;
            }
            else
            {
              SelectedResultIndex = Result.IndexOf(NearestEmpty);
            }
          },
          () => Result != null);
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
            if(Result.Any(r => r.A1 == null))
            {
              ViewModelLocator.DialogService.ShowMessage("One or more entries do not have any value in A1 column. Please fix these entries before uploading result.", true);
              return;
            }

            if (ViewModelLocator.DialogService.AskBooleanQuestion("Are you sure you want to upload result to the server?"))
            {
              IsBusy = true;

              try
              {
                ViewModelLocator.DataService.UploadResult(_SelectedAccess.exam_id, Environment.MachineName, Result.Select(r => r.ToHFQResultRow())).ContinueWith(t =>
                {
                  IsBusy = false;

                  if (!t.IsFaulted)
                  {
                    GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                      ViewModelLocator.DialogService.ShowMessage("Results uploaded successfully.", false);
                      SelectedAccess = null;

                      Result.Clear();
                      RaisePropertyChanged(nameof(Result));

                      SelectedResultIndex = 0;
                      SearchText = "";

                      XPSPath = "";
                      XMLDoc = null;

                      UploadResultCommand.RaiseCanExecuteChanged();
                    });
                  }
                  else
                  {
                    ViewModelLocator.DialogService.ShowMessage("Could not upload result to the server. Please try again later. The error message is: " + t.Exception.InnerException.Message, true);
                  }
                });
              }
              catch(Exception ee)
              {
                ViewModelLocator.DialogService.ShowMessage(ee);
              }
            }
          },
          () => ViewModelLocator.Auth.IsLoggedIn && (ViewModelLocator.Auth.User.type == UserType.Admin || ViewModelLocator.Auth.User.type == UserType.Downloader) && _SelectedAccess != null && Result.Count > 0);
        }

        return _UploadResultCommand;
      }
    }
  }
}
