using MultiDF.VM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MultiDF.Views
{
  /// <summary>
  /// Interaction logic for ResultContent.xaml
  /// </summary>
  public partial class ResultContent : UserControl
  {


    public bool AllowOneExpansionOnly
    {
      get { return (bool)GetValue(AllowOneExpansionOnlyProperty); }
      set { SetValue(AllowOneExpansionOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for AllowOneExpansionOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AllowOneExpansionOnlyProperty =
        DependencyProperty.Register("AllowOneExpansionOnly", typeof(bool), typeof(ResultContent), new PropertyMetadata(true));

    
    public ResultContent()
    {
      InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      var DC = (DFResultRow)(((Hyperlink)sender).DataContext);
      var Q = DC.Q1;

      ViewModelLocator.WordService.OpenDocument(Q.Doc.SourcePath, Q.Start, Q.End);
    }

    private void Hyperlink_RequestNavigate2(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      var DC = (DFResultRow)(((Hyperlink)sender).DataContext);
      var Q = DC.Q2;

      ViewModelLocator.WordService.OpenDocument(Q.Doc.SourcePath, Q.Start, Q.End);
    }

    private void TreeViewItem_Expanded(object sender, System.Windows.RoutedEventArgs e)
    {
      if (AllowOneExpansionOnly && e.Source is TreeViewItem tvi)
      {
        foreach (var Node in TV.Items)
        {
          if (Node != tvi.DataContext)
          {
            var C = TV.ItemContainerGenerator.ContainerFromItem(Node) as TreeViewItem;

            if(C != null)
              C.IsExpanded = false;
          }
        }
      }
    }
  }
}
