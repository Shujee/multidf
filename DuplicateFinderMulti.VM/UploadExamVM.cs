using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HFQOModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class UploadExamVM : ViewModelBase
  {
    private string _NewExamNumber;
    public string NewExamNumber
    {
      get { return _NewExamNumber; }
      set
      {
        Set(ref _NewExamNumber, value);

        if (string.IsNullOrEmpty(_NewExamNumber))
        {
          ExamNumberExists = false;
          RaisePropertyChanged(nameof(ExamNumberExists));
        }
        else
        {
          ViewModelLocator.DataService.ExamNumberExists(_NewExamNumber).ContinueWith(t =>
          {
            ExamNumberExists = t.Result;
            GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => RaisePropertyChanged(nameof(ExamNumberExists)));
          });
        }
      }
    }

    private string _NewExamName;
    public string NewExamName
    {
      get { return _NewExamName; }
      set { Set(ref _NewExamName, value); }
    }

    private string _NewExamDescription;
    public string NewExamDescription
    {
      get { return _NewExamDescription; }
      set { Set(ref _NewExamDescription, value); }
    }

    private string _Remarks = "UPDATED";
    public string Remarks
    {
      get => _Remarks;
      set => Set(ref _Remarks, value);
    }

    public string FileName { get; set; }
    public int QACount { get; set; }

    public MasterFile[] Exams { get; private set; }

    public MasterFile SelectedExam { get; set; }

    public bool ExamNumberExists { get; private set; }

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


    public Task<bool> CheckExamNumberExists()
    {
      if (string.IsNullOrEmpty(_NewExamNumber))
        return Task.FromResult(false);
      else
        return ViewModelLocator.DataService.ExamNumberExists(_NewExamNumber);
    }
  }
}