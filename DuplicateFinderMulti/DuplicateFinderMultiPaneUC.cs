using DuplicateFinderMulti.Views;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DuplicateFinderMulti
{
  public partial class DuplicateFinderMultiPaneUC : UserControl
  {
    internal MainView MV;
    
    public DuplicateFinderMultiPaneUC()
    {
      InitializeComponent();

      MV = new MainView();
      EH.Child = MV;
    }
  }
}
