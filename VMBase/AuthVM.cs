using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MultiDFCommon;
using Settings = VMBase.Properties.Settings;

namespace VMBase
{
  /// <summary>
  /// Handles client authentication (login/logout etc.)
  /// </summary>
  public class AuthVM : ViewModelBase
  {
    public AuthVM()
    {
      Email = Properties.Settings.Default.Email;
      Password = Properties.Settings.Default.Password;
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

    private UserType _UserType;
    public UserType UserType
    {
      get => _UserType;
      private set => Set(ref _UserType, value);
    }

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
                  if (t.Result == UserType.None)
                    ViewModelLocatorBase.DialogService.ShowMessage("Internet connection error or specified credentials are not correct.", true);
                  else
                  {
                    UserType = t.Result;
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
          () => !IsLoggedIn);
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
