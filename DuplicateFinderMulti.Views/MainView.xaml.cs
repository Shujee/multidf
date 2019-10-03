using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using System.Windows.Controls;
using System.Windows.Documents;

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

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      var DC = (DFResultRow)(((Hyperlink)sender).DataContext);
      var Q =  DC.Q1;

      ViewModelLocator.WordService.OpenDocument(Q.Doc.SourcePath, Q.Start, Q.End);
    }

    private void Hyperlink_RequestNavigate2(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      var DC = (DFResultRow)(((Hyperlink)sender).DataContext);
      var Q = DC.Q2;

      ViewModelLocator.WordService.OpenDocument(Q.Doc.SourcePath, Q.Start, Q.End);
    }

    private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
    {

    }
  }
}