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

      var Doc =  DC.Q1.Doc.SourcePath;
      var Start = DC.Q1.Start;

      ViewModelLocator.WordService.OpenDocument(Doc, Start);
    }

    private void Hyperlink_RequestNavigate2(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      var DC = (DFResultRow)(((Hyperlink)sender).DataContext);

      var Doc = DC.Q2.Doc.SourcePath;
      var Start = DC.Q2.Start;

      ViewModelLocator.WordService.OpenDocument(Doc, Start);
    }
  }
}