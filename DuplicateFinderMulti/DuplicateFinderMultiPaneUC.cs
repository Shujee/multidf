using DuplicateFinderMulti.Views;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DuplicateFinderMulti
{
  public partial class DuplicateFinderMultiPaneUC : UserControl
  {
    [DllImport("user32", ExactSpelling = false, CharSet = CharSet.Auto)]
    internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    private const string wordClassName = "OpusApp";
    internal MainView MV;
    
    public DuplicateFinderMultiPaneUC()
    {
      InitializeComponent();

      MV = new MainView();
      EH.Child = MV;
    }
  }
}
