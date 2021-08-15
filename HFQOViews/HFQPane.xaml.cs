using HFQOVM;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VMBase;

namespace HFQOViews
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
      //Robert has asked not to refresh QAs
      //QAs.Refresh();

      //if(lstQAs.Items.Count > 0)
      //  lstQAs.ScrollIntoView(lstQAs.Items[0]);

      SearchBox.Focus();
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        if (QAs != null)
        {
          //Refresh filtering
          QAs.Refresh();
        }
        else
          ViewModelLocator.DialogService.ShowMessage("List of QAs is empty. There is nothing to search.", false);
      }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
      //Refresh filtering
      if(QAs != null)
        QAs.Refresh();
      else
        ViewModelLocator.DialogService.ShowMessage("List of QAs is empty. There is nothing to search.", false);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      SearchBox.Text = "";

      //Refresh filtering
      if (QAs != null)
        QAs.Refresh();
    }

    private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
    {
      e.Accepted = (e.Item as QA).Question.IndexOf(SearchBox.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.Item as QA).Choices.Any(c => c.IndexOf(SearchBox.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (sender is ListBoxItem lbi)
      {
        QASelected?.Invoke(lbi.DataContext as QA);
      }
    }
  }
}