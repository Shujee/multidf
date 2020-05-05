using Microsoft.Office.Interop.Word;
using MultiDF.Views;
using System.Windows.Forms;

namespace MultiDF
{
  public partial class SeqErrorsPaneUC : UserControl
  {
    internal SequenceErrorsView SEV;
    
    public SeqErrorsPaneUC(Document doc)
    {
      InitializeComponent();

      SEV = new SequenceErrorsView();
      SEV.DataContext = new VM.SeqErrorsVM(doc.FullName);
      EH.Child = SEV;
    }
  }
}
