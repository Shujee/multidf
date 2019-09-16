using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Settings = DuplicateFinderMulti.VM.Properties.Settings;

namespace DuplicateFinderMulti.VM
{
  public class ConfigVM : ViewModelBase
  {
    public ConfigVM()
    {
      LoadCommand.Execute(null);
    }

    private RelayCommand _LoadCommand;
    public RelayCommand LoadCommand
    {
      get
      {
        if (_LoadCommand == null)
        {
          _LoadCommand = new RelayCommand(() =>
          {
          }, true);
        }

        return _LoadCommand;
      }
    }

    private RelayCommand _SaveCommand;
    public RelayCommand SaveCommand
    {
      get
      {
        if (_SaveCommand == null)
        {
          _SaveCommand = new RelayCommand(() =>
          {
            Settings.Default.Save();
          }, true);
        }

        return _SaveCommand;
      }
    }
  }
}