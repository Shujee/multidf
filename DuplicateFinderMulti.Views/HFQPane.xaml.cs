using DuplicateFinderMulti.VM;
using System;
using System.ComponentModel;
using System.Linq;
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
    public event Action<QA> QASelected;

    //My VM object
    private HFQVM MyVM => (HFQVM)this.DataContext;
    private ICollectionView QAs => (this.Resources["QACVS"] as CollectionViewSource).View;

    public HFQPane()
    {
      InitializeComponent();

      MyVM.NewResultRowAdded += MyVM_NewResultRowAdded;
    }

    private void MyVM_NewResultRowAdded(HFQResultRowVM obj)
    {
      SearchBox.Text = "";
      SearchBox.Focus();
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

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      SearchBox.Text = "";

      //Refresh filtering
      QAs.Refresh();
    }

    private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
    {
      e.Accepted = (e.Item as QA).Question.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.Item as QA).Choices.Any(c => c.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (sender is ListBox lb)
      {
          QASelected?.Invoke(lb.SelectedItem as QA);
      }
    }
  }
}
