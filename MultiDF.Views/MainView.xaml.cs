using MultiDF.VM;
using GalaSoft.MvvmLight.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MultiDF.Views
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

    private void OpenMRU_Click(object sender, RoutedEventArgs e)
    {
      if (sender is Button MRUButton)
        MRUButton.ContextMenu.IsOpen = true;
    }
  }
}