using System.Windows;

namespace DuplicateFinderMulti.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    public DiffWindow()
    {
      InitializeComponent();    
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }
  }
}