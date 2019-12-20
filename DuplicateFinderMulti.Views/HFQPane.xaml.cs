using DuplicateFinderMulti.VM;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DuplicateFinderMulti.Views
{
  /// <summary>
  /// Interaction logic for DocumentView.xaml
  /// </summary>
  public partial class HFQPane : UserControl
  {
    public event Action<QA> QADoubleClicked;

    //My VM object
    private MainVM MyVM => ((MainVM)this.DataContext);
    private ICollectionView QAs => (this.Resources["QACVS"] as CollectionViewSource).View;

    public HFQPane()
    {
      InitializeComponent();
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        //Refresh filtering
        QAs.Refresh();
      }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
      //Refresh filtering
      QAs.Refresh();
    }

    private void FilesListItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.Source is ListBoxItem lbi)
      {
        if(lbi.DataContext is QA qa)
        {
          QADoubleClicked?.Invoke(qa);
        }
      }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      SearchBox.Text = "";

      //Refresh filtering
      QAs.Refresh();
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.Source is ListBoxItem lbi)
      {
      }
    }

    private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
    {
      e.Accepted = (e.Item as QA).Question.Contains(SearchBox.Text);
    }
  }
}
