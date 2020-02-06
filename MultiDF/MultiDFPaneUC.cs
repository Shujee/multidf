using MultiDF.Views;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiDF
{
  public partial class MultiDFPaneUC : UserControl
  {
    internal MainView MV;
    
    public MultiDFPaneUC()
    {
      InitializeComponent();

      MV = new MainView();
      EH.Child = MV;
    }
  }
}
