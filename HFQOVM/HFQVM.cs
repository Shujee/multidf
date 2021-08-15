using Common;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
      public string ExamName { get; set; }
      public AccessibleMasterFile SelectedAccess { get; set; }
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
    private readonly string ResultCacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HFQApp", "result_cache.xml");

#if (DEBUG)
    private const int MIN_SNAPSHOT_TIME = 1; //min. time to wait before taking next camera snapshot
    private const int MAX_SNAPSHOT_TIME = 2; //max. time to wait before taking next camera snapshot
#else
    private const int MIN_SNAPSHOT_TIME = 5; //min. time to wait before taking next camera snapshot
    private const int MAX_SNAPSHOT_TIME = 10; //max. time to wait before taking next camera snapshot
#endif

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
          ViewModelLocator.Logger.Info($"User '{ViewModelLocator.Auth.User.name}' logged in successfully.");

          GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
          {
            OpenExamCommand.RaiseCanExecuteChanged();
            UploadResultCommand.RaiseCanExecuteChanged();
          });

          if (ViewModelLocator.Auth.IsLoggedIn)
          {
            if (File.Exists(ResultCacheFilePath))
            {
              ViewModelLocator.Logger.Info("Previous cache file exists. Loading data from cache file.");

              var Cache = ReadFromCache();
              Result = Cache.Result;
              XPSPath = Cache.XPSPath;
              XMLDoc = Cache.XMLDoc;
              _DownloadId = Cache.DownloadId;
              ExamName = Cache.ExamName;
              _QueuedSnapshots = Cache.QueuedSnapshots;
              SelectedAccess = Cache.SelectedAccess;

              ViewModelLocator.Logger.Info($"There are {_QueuedSnapshots.Count} snapshots in the uplaod queue.");

              RaisePropertyChanged(nameof(Result));
            }

            UpdateProgress(true, "Ready", 0);
          }
          else
          {
            SelectedAccess = null;

            Result.Clear();
            RaisePropertyChanged(nameof(Result));

            if (File.Exists(ResultCacheFilePath))
              File.Delete(ResultCacheFilePath);

            _DownloadId = null;
            ExamName = "No Exam";
            SelectedResultIndex = 0;
            SearchText = "";
            _QueuedSnapshots.Clear();

            XPSPath = null;
            XMLDoc = null;
          }
        }
        else if (e.PropertyName == nameof(ViewModelLocator.Auth.User))
        {
          UploadResultCommand.RaiseCanExecuteChanged();
        }
      };
    }

    private async void _CameraTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
#if (!DEBUG)
      if (ViewModelLocator.HardwareHelper.HasMultipleScreens())
      {
        _CameraTimer.Stop();
        _CameraTimer.Elapsed -= _CameraTimer_Elapsed;
        AutoSave();

        ViewModelLocator.Logger.Error("Multiple monitors or projector detected. Exiting.");
        ViewModelLocator.DialogService.ShowMessage("Multiple monitors or projector detected. The application will close now. Please fix the problem and restart the application to continue from where you left.", true);
        ViewModelLocator.ApplicationService.Shutdown();
        return;
      }
#endif

      //if an exam is currently open and it is time to take next snapshot, we'll take a snapshot every once in a while.
      if (_DownloadId.HasValue && !string.IsNullOrEmpty(this.XPSPath) && DateTime.Now >= _NextSnapshotTimestamp)
      {
        try
        {
         var snapshot = await ViewModelLocator.CameraService.TakeCameraSnapshot();

          if (snapshot != null)
          {
            //Check if it's time for us to take next snapshot
            //Compute a random time for next snapshot (between MIN and MAX const values defined at the top of this class)
            _NextSnapshotTimestamp = DateTime.Now.AddMinutes(MIN_SNAPSHOT_TIME + (new Random()).Next(0, MAX_SNAPSHOT_TIME - MIN_SNAPSHOT_TIME));

            SnapshotCaptured?.Invoke();

            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
              DateTime Timestamp = DateTime.Now;
              string FileName = Timestamp.ToString("yyyyMMddHHmmss") + ".jpg";
              using (var fs = isoStore.CreateFile(FileName))
              {
                snapshot.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);

                var FullFilePath = fs.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fs).ToString();
                _QueuedSnapshots.Add(FullFilePath, Timestamp);

                ViewModelLocator.Logger.Info($"Snapshot added to queue. Queue count: {_QueuedSnapshots.Count}");

                AutoSave();
              }
            }

            if (!_IsUploadingSnapshots && _QueuedSnapshots.Count > 0)
              await UploadQueuedSnapshots();
          }
          else
          {
            throw new Exception("CameraService.TakeCameraSnapshot() returned null.");
          }
        }
        catch (Exception e2)
        {
          _CameraTimer.Stop();
          _CameraTimer.Elapsed -= _CameraTimer_Elapsed;

          AutoSave();

          ViewModelLocator.Logger.Error("Camera snapshot exception: " + e2.Message);
          ViewModelLocator.Logger.Error("Exiting.");

          ViewModelLocator.DialogService.ShowMessage("Your camera device is not accessible. The application will close now. Make sure your camera is turned on and restart the application to continue from where you left.", true);
          
          ViewModelLocator.ApplicationService.Shutdown();
        }
      }
    }

    private bool _IsUploadingSnapshots = false;
    private async Task UploadQueuedSnapshots()
    {
      _IsUploadingSnapshots = true;

      Dictionary<string, DateTime> FailedToUploadSnapshots = new Dictionary<string, DateTime>();

      try
      {
        if (_QueuedSnapshots.Count > 0)
        {
          ViewModelLocator.Logger.Info($"Starting snapshot upload. Queue count: " + _QueuedSnapshots.Count);

          while (_QueuedSnapshots.Count > 0)
          {
            var snap = _QueuedSnapshots.First();
            var ImageFileName = snap.Key;

            _QueuedSnapshots.Remove(ImageFileName);

            if (File.Exists(ImageFileName))
            {
              ViewModelLocator.Logger.Info($"Uploading snapshot {ImageFileName}");

              bool result = false;
              try
              {
               result = await ViewModelLocator.DataService.UploadSnapshot(_DownloadId.Value, snap.Value.ToUniversalTime(), ImageFileName);
              }
              catch (Exception ee)
              {
                ViewModelLocator.Logger.Error(ee, $"Snapshot upload failed. Download Id: {_DownloadId}, Image File: {ImageFileName}");
              }

              if (result)
              {
                try
                {
                  ViewModelLocator.Logger.Info("Upload success. Deleting local snapshot file.");

                  //and remove the image file from the disk
                  File.Delete(ImageFileName);

                  AutoSave();
                }
                catch (Exception ee)
                {
                  ViewModelLocator.Logger.Warn(ee, "Could not delete snapshot from local cache.");
                }
              }
              else
              {
                ViewModelLocator.Logger.Error($"Snapshot upload failed. Download Id: {_DownloadId}, Image File: {ImageFileName}");

                //if a snapshot fails, add it to failed uploads list. We'll add failed snapshots back to upload queue below.
                FailedToUploadSnapshots.Add(ImageFileName, snap.Value);
                AutoSave();
              }
            }
            else
            {
              //Image file no longer exists on the disk. We'll ignore it and proceed to update our local cache. This will remove the image file entry from cache.
              ViewModelLocator.Logger.Error($"Snapshot file does not exist on disk. Download Id: {_DownloadId}, Image File: {ImageFileName}");
              AutoSave();
            }
          }
        }
      }
      finally
      {
        _IsUploadingSnapshots = false;

        if (FailedToUploadSnapshots.Count > 0)
        {
          ViewModelLocator.Logger.Error($"{FailedToUploadSnapshots.Count} snapshot failed to upload. Adding them back to the queue.");

          foreach (var failed in FailedToUploadSnapshots)
            _QueuedSnapshots.Add(failed.Key, failed.Value);

          AutoSave();
        }
      }
    }

    private HFQVMCache ReadFromCache()
    {
      var xml = File.ReadAllText(ResultCacheFilePath);
      using (StringReader s = new StringReader(xml))
      {
        using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        {
          return (HFQVMCache)HFQSerializer.ReadObject(reader, true);
        }
      }
    }

    /// <summary>
    /// New calls to this function will wait for the previous ones to complete.
    /// </summary>
    private bool autoSaving = false;
    private bool AutoSave()
    {
      if (autoSaving)
      {
        ViewModelLocator.Logger.Info("AutoSave is already running.");
        return false;
      }

      autoSaving = true;

      var TempResult = new HFQVMCache()
      {
        Result = this.Result,
        XMLDoc = XMLDoc,
        XPSPath = XPSPath,
        QueuedSnapshots = _QueuedSnapshots,
        DownloadId = _DownloadId,
        ExamName = _ExamName,
        SelectedAccess = SelectedAccess,
      };

      try
      {
        using (FileStream fs = new FileStream(ResultCacheFilePath, FileMode.Create))
        {
          using (XmlWriter xmlWriter = XmlWriter.Create(fs, xmlWriterSettingsForWordDocs))
          {
            HFQSerializer.WriteStartObject(xmlWriter, TempResult);
            HFQSerializer.WriteObjectContent(xmlWriter, TempResult);
            HFQSerializer.WriteEndObject(xmlWriter);
          }
        }

        return true;
      }
      catch (Exception ee)
      {
        ViewModelLocator.Logger.Error("AutoSave failed. Error message: " + ee.Message);
        return false;
      }
      finally
      {
        autoSaving = false;
      }
    }

    private AccessibleMasterFile _SelectedAccess;
    public AccessibleMasterFile SelectedAccess
    {
      get => _SelectedAccess;
      set
      {
        Set(ref _SelectedAccess, value);
        UploadResultCommand.RaiseCanExecuteChanged();
      }
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

    private string _ExamName;
    public string ExamName
    {
      get => _ExamName;
      private set => Set(ref _ExamName, value);
    }

    public bool IsWatching => !string.IsNullOrEmpty(this.XPSPath);

    private AsyncCommand _OpenExamCommand;
    public AsyncCommand OpenExamCommand
    {
      get
      {
        if (_OpenExamCommand == null)
        {
          _OpenExamCommand = new AsyncCommand(async () =>
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

            try
            {
              var Exams = await ViewModelLocator.DataService.GetExamsDL();

              IsBusy = false;

              ViewModelLocator.Logger.Info($"Opening new exam");
              ViewModelLocator.Logger.Info($"Showing Exams List dialog");

              var Res = await ViewModelLocator.DialogServiceHFQ.ShowExamsListDialog();

              if (Res != null)
              {
                SelectedAccess = Res;

                try
                {
                  IsBusy = true;

                  ViewModelLocator.Auth.IsCommunicating = true;

                  ViewModelLocator.Logger.Info($"Downloading Exam for Access ID: '{_SelectedAccess.access_id}");

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
                    ViewModelLocator.Logger.Info($"Creating local XPS file");
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

                    ViewModelLocator.Logger.Info($"Creating local XML file");
                    var XMLString = Encoding.UTF8.GetString(Encryption.Decrypt(Convert.FromBase64String(MF.xml)));
                    XMLDoc = XMLDoc.FromXML(XMLString);

                    _DownloadId = MF.download_id;
                    ExamName = MF.number + " - " + MF.name;

                    ViewModelLocator.Logger.Info($"Clearing snapshots queue and results");
                    _QueuedSnapshots.Clear();
                    Result.Clear();
                    RaisePropertyChanged(nameof(Result));

                    var Row = new HFQResultRowVM() { Q = 1 };
                    Result.Add(Row);
                    NewResultRowAdded?.Invoke(Row);

                    ViewModelLocator.Logger.Info($"Starting camera timer");
                    _CameraTimer.Stop();
                    _CameraTimer.Start();

                    SelectedResultIndex = 0;

                    ViewModelLocator.Logger.Info($"Updating results cache");
                    AutoSave();
                  }
                }
                catch (Exception ee)
                {
                  ViewModelLocator.DialogService.ShowMessage(ee.Message, true);
                  ViewModelLocator.Logger.Error("OpenExam command failed. Error message is: " + ee.Message);
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
            catch (Exception ex)
            {
              if (ex is AggregateException aggex)
                ViewModelLocator.DialogService.ShowMessage("Could not fetch exams list from the server. Please try again later. The error message is: " + aggex.InnerExceptions[0].Message, true);
              else
                ViewModelLocator.DialogService.ShowMessage("Could not fetch exams list from the server. Please try again later. The error message is: " + ex.Message, true);
            }
            finally
            {
              IsBusy = false;
            }
          },

          () => ViewModelLocator.Auth.IsLoggedIn && (ViewModelLocator.Auth.User.type == UserType.Admin || ViewModelLocator.Auth.User.type == UserType.Downloader));
        }

        return _OpenExamCommand;
      }
    }


    private RelayCommand _OpenLogFileCommand;
    public RelayCommand OpenLogFileCommand
    {
      get
      {
        if (_OpenLogFileCommand == null)
        {
          _OpenLogFileCommand = new RelayCommand(() =>
          {
            Process.Start(new ProcessStartInfo()
            {
              FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"HFQApp\"),
              UseShellExecute = true
            });
          },
          () => true);
        }

        return _OpenLogFileCommand;
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
                  AutoSave();
                }
                else if (R.A2 == null)
                {
                  R.A2 = qa.Index;
                  AutoSave();
                }
                else if (R.A3 == null)
                {
                  R.A3 = qa.Index;
                  AutoSave();
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
              var Row = new HFQResultRowVM() { Q = (Result.LastOrDefault()?.Q ?? 0) + 1 };
              Result.Add(Row);
              NewResultRowAdded?.Invoke(Row);
              SelectedResultIndex = Result.Count - 1;

              AutoSave();
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
            if (Result == null || Result.Count == 0 || Result.All(r => r.A1 == null && r.A2 == null && r.A3 == null))
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

              ViewModelLocator.Logger.Info($"Uploading result for exam '{_ExamName}'");

              try
              {
                if (Result.Last().A1 == null)
                  Result.RemoveAt(Result.Count - 1);

                ViewModelLocator.DataService.UploadResult(_SelectedAccess.exam_id, Environment.MachineName, Result.Select(r => r.ToHFQResultRow())).ContinueWith(t =>
                {
                  IsBusy = false;

                  if (!t.IsFaulted)
                  {
                    ViewModelLocator.Logger.Info($"Successfully uploaded result");

                    GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                      SelectedAccess = null;

                      ViewModelLocator.Logger.Info($"Clearing local results cache and snapshots queue.");

                      _QueuedSnapshots.Clear();
                      Result.Clear();
                      RaisePropertyChanged(nameof(Result));

                      if (File.Exists(ResultCacheFilePath))
                        File.Delete(ResultCacheFilePath);

                      SelectedResultIndex = 0;
                      SearchText = "";
                      _DownloadId = null;
                      _ExamName = "No Exam";

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
          () => ViewModelLocator.Auth.IsLoggedIn && (ViewModelLocator.Auth.User?.type == UserType.Admin || ViewModelLocator.Auth.User?.type == UserType.Downloader) && _SelectedAccess != null && Result.Count > 0);
        }

        return _UploadResultCommand;
      }
    }
  }
}
