using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  public class UploadExamVM : ViewModelBase
  {
    public UploadExamVM()
    {
      RefreshExamsCommand.Execute(null);
    }

    private string _NewExamName;
    public string NewExamName
    {
      get { return _NewExamName; }
      set { Set(ref _NewExamName, value); }
    }

    public Dictionary<string, string> Exams { get; private set; }

    public KeyValuePair<string, string> SelectedExam { get; set; }

    private bool _CreateNew;
    public bool CreateNew
    {
      get => _CreateNew;
      set => Set(ref _CreateNew, value);
    }


    private RelayCommand _RefreshExamsCommand;
    public RelayCommand RefreshExamsCommand
    {
      get
      {
        if (_RefreshExamsCommand == null)
        {
          _RefreshExamsCommand = new RelayCommand(() =>
          {
            ViewModelLocator.DataService.GetExamsUL().ContinueWith(t =>
            {
              if (t.IsCompleted && !t.IsFaulted)
              {
                Exams = t.Result;
                RaisePropertyChanged(nameof(Exams));
              }
              else
              {
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                  ViewModelLocator.DialogService.ShowMessage(t.Exception.Message, true);
                });
              }
            });
          },
          () => ViewModelLocator.Auth.IsLoggedIn);
        }

        return _RefreshExamsCommand;
      }
    }
  }
}