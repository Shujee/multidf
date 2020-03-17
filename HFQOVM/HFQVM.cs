using Common;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VMBase;

namespace HFQOVM
{
  public class HFQVM : VMBase.MainBase
  {
    public class HFQVMCache
    {
      public ObservableCollection<HFQResultRowVM> Result { get; set; }
      public XMLDoc XMLDoc { get; set; }
      public string XPSPath { get; set; }
      public Dictionary<string, DateTime> QueuedSnapshots { get; set; }
      public int? DownloadId { get; set; }
    }

    public event Action<HFQResultRowVM> NewResultRowAdded;
    public event Action<string> ExamUploaded; //front-end will listen to this event to remove left-over XPS file
    public event Action SnapshotCaptured; //front-end will listen to this event and make camera shutter sound

    private readonly Type[] AllObjectTypes = {
      typeof(HFQResultRow),
      typeof(HFQResultRowVM),
      typeof(XMLDoc),
      typeof(ObservableCollection<HFQResultRowVM>),
      typeof(Dictionary<string, DateTime>)
    };

    private System.Timers.Timer _CameraTimer = new System.Timers.Timer(15 * 1000); //takes camera snapshots every once in a while
    private Dictionary<string, DateTime> _QueuedSnapshots = new Dictionary<string, DateTime>();
    private readonly DataContractSerializer HFQSerializer;
    private int? _DownloadId = null;
    private DateTime _NextSnapshotTimestamp = DateTime.Now;

    private const int MIN_SNAPSHOT_TIME = 5; //min. time to wait before taking next camera snapshot
    private const int MAX_SNAPSHOT_TIME = 10; //max. time to wait before taking next camera snapshot

    /// <summary>
    /// This setting is required to prevent exceptions that are thrown by the serialization when it encounters control characters used by
    /// Microsoft Word to denote newlines and non-breaking newlines.
    /// </summary>
    private readonly XmlWriterSettings xmlWriterSettingsForWordDocs = new XmlWriterSettings()
    {
      NewLineChars = "\a\r\n",
      CheckCharacters = false,
      Encoding = Encoding.UTF8,
      NewLineHandling = NewLineHandling.Entitize,
    };

    public HFQVM()
    {
      HFQSerializer = new DataContractSerializer(typeof(HFQVMCache), AllObjectTypes, 0x7F_FFFF, false, true, null); //max graph size is 8,388,607‬ items
      Result = new ObservableCollection<HFQResultRowVM>();

      _CameraTimer.AutoReset = true;
      _CameraTimer.Enabled = true;
      _CameraTimer.Elapsed += _CameraTimer_Elapsed;

      ViewModelLocator.Auth.PropertyChanged += (sender, e) =>
      {
        if (e.PropertyName == nameof(ViewModelLocator.Auth.IsLoggedIn))
        {
          OpenExamCommand.RaiseCanExecuteChanged();

          if (ViewModelLocator.Auth.IsLoggedIn)
          {
            if (File.Exists("result_cache.xml"))
            {
              var Cache = ReadFromCache();
              Result = Cache.Result;
              XPSPath = Cache.XPSPath;
              XMLDoc = Cache.XMLDoc;
              _DownloadId = Cache.DownloadId;
              _QueuedSnapshots = Cache.QueuedSnapshots;

              RaisePropertyChanged(nameof(Result));
            }

            UpdateProgress(true, "Fetching master files list", 0);
            RefreshExamsList();
          }
          else
          {
            SelectedAccess = null;

            Result.Clear();
            RaisePropertyChanged(nameof(Result));

            if (File.Exists("result_cache.xml"))
              File.Delete("result_cache.xml");

            _DownloadId = null;
            SelectedResultIndex = 0;
            SearchText = "";
            _QueuedSnapshots.Clear();

            XPSPath = null;
            XMLDoc = null;
          }
        }
      };
    }

    private void _CameraTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
#if(!DEBUG)
      if (ViewModelLocator.HardwareHelper.HasMultipleScreens())
      {
        _CameraTimer.Stop();
        _CameraTimer.Elapsed -= _CameraTimer_Elapsed;
        WriteToCache();

        ViewModelLocator.Logger.Error("Multiple monitors or projector detected. Exiting.");
        ViewModelLocator.DialogService.ShowMessage("Multiple monitors or projector detected. The application will close now. Please fix the problem and restart the application to continue from where you left.", true);
        ViewModelLocator.ApplicationService.Shutdown();
        return;
      }
#endif

      //if an exam is currently open, we'll take a snapshot every once in a while.
      if (!string.IsNullOrEmpty(this.XPSPath))
      {
        var SnapshotTask = ViewModelLocator.CameraService.TakeCameraSnapshot();

        SnapshotTask.ContinueWith(t =>
        {
          if (t.Result != null)
          {
            //Check if it's time for us to take next snapshot
            if (DateTime.Now >= _NextSnapshotTimestamp)
            {
              //Compute a random time for next snapshot (between MIN and MAX const values defined at the top of this class)
              _NextSnapshotTimestamp = DateTime.Now.AddMinutes(MIN_SNAPSHOT_TIME + (new Random()).Next(0, MAX_SNAPSHOT_TIME - MIN_SNAPSHOT_TIME));

              ViewModelLocator.DialogService.ShowMessage(_NextSnapshotTimestamp.ToString(), false);

              SnapshotCaptured?.Invoke();

              using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
              {
                DateTime Timestamp = DateTime.Now;
                string FileName = Timestamp.ToString("yyyyMMddHHmmss") + ".jpg";
                using (var fs = isoStore.CreateFile(FileName))
                {
                  t.Result.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);

                  var FullFilePath = fs.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fs).ToString();
                  _QueuedSnapshots.Add(FullFilePath, Timestamp);
                }
              }

              UploadQueuedSnapshots();
            }
          }
          else
          {
            _CameraTimer.Stop();
            _CameraTimer.Elapsed -= _CameraTimer_Elapsed;
            WriteToCache();

            ViewModelLocator.Logger.Error("CameraService.TakeCameraSnapshot() returned null. Exiting. Error: " + t.Exception.Message);
            ViewModelLocator.DialogService.ShowMessage("Your camera device is not accessible. The application will close now. Make sure your camera is turned on and restart the application to continue from where you left.", true);
            ViewModelLocator.ApplicationService.Shutdown();
          }
        });
      }
    }

    private bool _IsUploadingSnapshots = false;
    private void UploadQueuedSnapshots()
    {
      if (_IsUploadingSnapshots)
        return;
    
      if (_QueuedSnapshots.Count > 0)
      {
        _IsUploadingSnapshots = true;

        try
        {
          while (_QueuedSnapshots.Count > 0)
          {
            var snap = _QueuedSnapshots.First();
            _QueuedSnapshots.Remove(snap.Key);
            ViewModelLocator.DataService.UploadSnapshot(_DownloadId.Value, snap.Value.ToUniversalTime(), snap.Key).ContinueWith(t =>
            {
              if (t.Result)
              {
                try
                {
                  //and remove the image file from the disk
                  File.Delete(snap.Key);
                }
                catch (Exception ee)
                {
                  ViewModelLocator.Logger.Warn(ee, "Could not delete snapshot from local cache.");
                }
              }
              else
              {
                if (t.Exception == null)
                  ViewModelLocator.Logger.Error($"Snapshot upload failed. Download Id: {_DownloadId}, Image File: {snap.Key}");
                else
                  ViewModelLocator.Logger.Error(t.Exception, "Snapshot upload failed. Download Id: {_DownloadId}, Image File: {snap.Key}");

                //if a snapshot fails, add it back to the queue.
                _QueuedSnapshots.Add(snap.Key, snap.Value);
              }
            }).Wait();
          }
        }
        finally
        {
          _IsUploadingSnapshots = false;
        }
      }
    }

    private HFQVMCache ReadFromCache()
    {
      var xml = File.ReadAllText("result_cache.xml");
      using (StringReader s = new StringReader(xml))
      {
        using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        {
          return (HFQVMCache)HFQSerializer.ReadObject(reader, true);
        }
      }
    }

    private void WriteToCache()
    {
      var TempResult = new HFQVMCache() { Result = this.Result, XMLDoc = XMLDoc, XPSPath = XPSPath, QueuedSnapshots = _QueuedSnapshots, DownloadId = _DownloadId };

      using (FileStream fs = new FileStream("result_cache.xml", FileMode.Create))
      {
        using (XmlWriter xmlWriter = XmlWriter.Create(fs, xmlWriterSettingsForWordDocs))
        {
          HFQSerializer.WriteStartObject(xmlWriter, TempResult);
          HFQSerializer.WriteObjectContent(xmlWriter, TempResult);
          HFQSerializer.WriteEndObject(xmlWriter);
        }
      }
    }

    public void RefreshExamsList()
    {
      ViewModelLocator.DataService.GetExamsDL().ContinueWith(t2 =>
      {
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
      set
      {
        Set(ref _XPSPath, value);
        RaisePropertyChanged(nameof(IsWatching));
      }
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

    public bool IsWatching => !string.IsNullOrEmpty(this.XPSPath);

    private RelayCommand _OpenExamCommand;
    public RelayCommand OpenExamCommand
    {
      get
      {
        if (_OpenExamCommand == null)
        {
          _OpenExamCommand = new RelayCommand(() =>
          {
            if (Result != null && Result.Count > 0 && Result.Any(r => r.A1 != null || r.A2 != null || r.A3 != null))
            {
              var SaveChanges = ViewModelLocator.DialogService.AskTernaryQuestion("Upload current result before opening new master file?");

              if (SaveChanges == null)
                return;
              else if (SaveChanges.Value)
              {
                //upload current results before opening a new exam
                UploadResultCommand.Execute(null);
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

                          string XPSFilePath;

                          //Store this downloaded data into an isolated storage file
                          using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
                          {
                            var XPSFileName = DateTime.Now.ToString("yyyyMMHHmmss") + $"_{MF.id}.gxr";
                            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(XPSFileName, FileMode.CreateNew, isoStore))
                            {
                              isoStream.Write(XPSBytes, 0, XPSBytes.Length);
                              XPSFilePath = isoStream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isoStream).ToString();
                              isoStream.Close();
                            }

                            isoStore.Close();
                          }

                          XPSPath = XPSFilePath;

                          var XMLString = Encoding.UTF8.GetString(Encryption.Decrypt(Convert.FromBase64String(MF.xml)));
                          XMLDoc = XMLDoc.FromXML(XMLString);

                          _DownloadId = MF.download_id;

                          _QueuedSnapshots.Clear();
                          Result.Clear();
                          RaisePropertyChanged(nameof(Result));

                          var Row = new HFQResultRowVM() { Q = 1 };
                          Result.Add(Row);
                          NewResultRowAdded?.Invoke(Row);

                          SelectedResultIndex = 0;

                          WriteToCache();
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
            if (SelectedResultIndex >= 0)
            {
              var R = Result[SelectedResultIndex];

              if (Result.Any(r => r.A1 == qa.Index || r.A2 == qa.Index || r.A3 == qa.Index))
              {
                ViewModelLocator.DialogService.ShowMessage($"This question has already been marked previously. You cannot use it again.", false);
              }
              else
              {
                if (R.A1 == null)
                {
                  R.A1 = qa.Index;
                  WriteToCache();
                }
                else if (R.A2 == null)
                {
                  R.A2 = qa.Index;
                  WriteToCache();
                }
                else if (R.A3 == null)
                {
                  R.A3 = qa.Index;
                  WriteToCache();
                }
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

    private RelayCommand<HFQResultRowVM> _RemoveLastAnswerCommand;
    public RelayCommand<HFQResultRowVM> RemoveLastAnswerCommand
    {
      get
      {
        if (_RemoveLastAnswerCommand == null)
        {
          _RemoveLastAnswerCommand = new RelayCommand<HFQResultRowVM>((row) =>
          {
            if (row.A3 != null)
              row.A3 = null;
            else if (row.A2 != null)
              row.A2 = null;
            else if (row.A1 != null)
              row.A1 = null;
            else
              Result.Remove(row);
          },
          (row) => true);
        }

        return _RemoveLastAnswerCommand;
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
              var Row = new HFQResultRowVM() { Q = (Result.LastOrDefault()?.Q??0) + 1 };
              Result.Add(Row);
              NewResultRowAdded?.Invoke(Row);
              SelectedResultIndex = Result.Count - 1;

              WriteToCache();
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
    /// <summary>
    /// We need to delete XPS files after UploadResult, which cannot be done till the View layer releases lock on the XPS file. So the  View layer will pass us with the delegate
    /// of the delete function that we'll call after UploadResult succeeds.
    /// </summary>
    public RelayCommand UploadResultCommand
    {
      get
      {
        if (_UploadResultCommand == null)
        {
          _UploadResultCommand = new RelayCommand(() =>
          {
            if(Result ==null || Result.Count == 0 || Result.All(r => r.A1 == null && r.A2 == null && r.A3 == null))
            {
              ViewModelLocator.DialogService.ShowMessage("There is nothing to upload.", true);
              return;
            }

            if (Result.Any(r => r != Result.Last() && r.A1 == null))
            {
              ViewModelLocator.DialogService.ShowMessage("One or more entries do not have any value in A1 column. Please fix these entries before uploading result.", true);
              return;
            }

            if (ViewModelLocator.DialogService.AskBooleanQuestion("Are you sure you want to upload result to the server?"))
            {
              IsBusy = true;

              try
              {
                if (Result.Last().A1 == null)
                  Result.RemoveAt(Result.Count - 1);

                ViewModelLocator.DataService.UploadResult(_SelectedAccess.exam_id, Environment.MachineName, Result.Select(r => r.ToHFQResultRow())).ContinueWith(t =>
                {
                  IsBusy = false;

                  if (!t.IsFaulted)
                  {
                    GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                      SelectedAccess = null;

                      _QueuedSnapshots.Clear();
                      Result.Clear();
                      RaisePropertyChanged(nameof(Result));

                      if (File.Exists("result_cache.xml"))
                        File.Delete("result_cache.xml");

                      SelectedResultIndex = 0;
                      SearchText = "";
                      _DownloadId = null;

                      //front-end will listen to this event and clean up the left-over XPS file
                      ExamUploaded.Invoke(XPSPath);

                      XPSPath = null;
                      XMLDoc = null;

                      UploadResultCommand.RaiseCanExecuteChanged();

                      ViewModelLocator.DialogService.ShowMessage("Results uploaded successfully.", false);
                    });
                  }
                  else
                  {
                    ViewModelLocator.DialogService.ShowMessage("Could not upload result to the server. Please try again later. The error message is: " + t.Exception.InnerException.Message, true);
                  }
                });
              }
              catch (Exception ee)
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
