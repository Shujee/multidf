using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using System.Windows.Controls;

namespace DuplicateFinderMulti.Views
{
  /// <summary>
  /// Interaction logic for MainView.xaml
  /// </summary>
  public partial class MainView : UserControl
  {
    public MainView()
    {
      InitializeComponent();

      if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
        SimpleIoc.Default.Register<IWordService, DummyWordService>();
    }
  }
}