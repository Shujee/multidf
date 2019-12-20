using DuplicateFinderMultiCommon;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class HFQVM : ViewModelBase
  {
    public HFQVM()
    {
      ViewModelLocator.Auth.PropertyChanged += (sender, e) =>
      {
        if (e.PropertyName == nameof(ViewModelLocator.Auth.IsLoggedIn))
        {
          OpenExamCommand.RaiseCanExecuteChanged();

          if(ViewModelLocator.Auth.IsLoggedIn)
          {
            ViewModelLocator.Main.UpdateProgress(true, "Fetching master files list", 0);
            ViewModelLocator.DataService.GetExams().ContinueWith(t2 => {
              _MyExams = t2.Result;
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => ViewModelLocator.Main.UpdateProgress(true, "Ready", 0));
            });
          }
          else
          {
            XML = null;
            XPSPath = null;
          }
        }
      };
    }

    private Dictionary<string, string> _MyExams;
    /// <summary>
    /// Important: ID field contains Access Id, not Exam Id.
    /// </summary>
    public Dictionary<string, string> MyExams => _MyExams;

    private string _SelectedAccess;
    public string SelectedAccess
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
    public XMLDoc XML
    {
      get => _XML;
      set => Set(ref _XML, value);
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
            RaisePropertyChanged(nameof(MyExams));

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
                    var XPSBytes = Encryption.Decrypt(Convert.FromBase64String(MF.xps));
                    var XPSFilePath = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllBytes(XPSFilePath, XPSBytes);
                    XPSPath = XPSFilePath;

                    var XMLString = System.Text.Encoding.UTF8.GetString(Encryption.Decrypt(Convert.FromBase64String(MF.xml)));
                    XML = XMLString.Deserialize<XMLDoc>();
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

  }
}
