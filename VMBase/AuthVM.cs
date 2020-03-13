using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Common;
using Settings = VMBase.Properties.Settings;
using System.Threading.Tasks;

namespace VMBase
{
  /// <summary>
  /// Handles client authentication (login/logout etc.)
  /// </summary>
  public class AuthVM : ViewModelBase
  {
    private System.Timers.Timer tmrServerStatus = new System.Timers.Timer(5000);

    public AuthVM()
    {
      Email = Properties.Settings.Default.Email;
      Password = Properties.Settings.Default.Password;

      tmrServerStatus.AutoReset = true;
      tmrServerStatus.Elapsed += TmrServerStatus_Elapsed;
      tmrServerStatus.Enabled = true;

      ViewModelLocatorBase.Register.PropertyChanged += Register_PropertyChanged;
    }

    private void Register_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if(e.PropertyName == nameof(RegisterVM.IsRegistered))
      {
        LoginCommand.RaiseCanExecuteChanged();
      }
    }

    private void TmrServerStatus_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      Task.Run(() =>
      {
        return ViewModelLocatorBase.DataService.IsAlive();
      }).ContinueWith(t =>
      {
        IsConnected = t.Result;
        GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => RaisePropertyChanged(nameof(IsConnected)));
      });
    }

    protected bool _IsLoggedIn;
    public bool IsLoggedIn
    {
      get { return _IsLoggedIn; }
      private set
      {
        Set(ref _IsLoggedIn, value);
        LoginCommand.RaiseCanExecuteChanged();
        LogoutCommand.RaiseCanExecuteChanged();
      }
    }

    private User _User;
    public User User
    {
      get => _User;
      private set => Set(ref _User, value);
    }
    public bool IsConnected { get; private set; } = false;

    private bool _IsCommunicating;
    public bool IsCommunicating
    {
      get { return _IsCommunicating; }
      set
      {
        Set(ref _IsCommunicating, value);
      }
    }

    private RelayCommand _LoginCommand;
    public RelayCommand LoginCommand
    {
      get
      {
        if (_LoginCommand == null)
        {
          _LoginCommand = new RelayCommand(() =>
          {
            //Remember Me setting is not saved in AppSettings. We toggle it ON if e-mail or password setting is not blank.
            if (!string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(Password))
              RememberMe = true;

            if (ViewModelLocatorBase.DialogService.ShowLogin())
            {
              IsCommunicating = true;

              ViewModelLocatorBase.DataService.Login(_Email, _Password).ContinueWith(t =>
              {
                GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                  if (t.Result == null)
                    ViewModelLocatorBase.DialogService.ShowMessage("Internet connection error or specified credentials are not correct.", true);
                  else
                  {
                    User = t.Result;
                    IsLoggedIn = true;
                  }

                  IsCommunicating = false;
                });

                if (!RememberMe)
                {
                  Email = "";
                  Password = "";
                }

                Settings.Default.Email = Email;
                Settings.Default.Password = Password;

                Settings.Default.Save();
              });
            }
          },
          () => ViewModelLocatorBase.Register.IsRegistered && !IsLoggedIn);
        }

        return _LoginCommand;
      }
    }

    private RelayCommand _LogoutCommand;
    public RelayCommand LogoutCommand
    {
      get
      {
        if (_LogoutCommand == null)
        {
          _LogoutCommand = new RelayCommand(() =>
          {
            IsCommunicating = true;
            ViewModelLocatorBase.DataService.Logout().ContinueWith(t =>
            {
              GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
              {
                IsCommunicating = false;
                IsLoggedIn = false;
              });
            });
          },
          () => _IsLoggedIn
          );
        }

        return _LogoutCommand;
      }
    }

    private string _Email;
    public string Email
    {
      get { return _Email; }
      set
      {
        Set(ref _Email, value);
        LoginCommand.RaiseCanExecuteChanged();
      }
    }

    private string _Password;
    public string Password
    {
      get { return _Password; }
      set
      {
        Set(ref _Password, value);
        LoginCommand.RaiseCanExecuteChanged();
      }
    }


    private bool _RememberMe;
    public bool RememberMe
    {
      get => _RememberMe;
      set => Set(ref _RememberMe, value);
    }
  }
}
