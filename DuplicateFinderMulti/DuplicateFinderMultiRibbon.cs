using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System.Linq;

namespace DuplicateFinderMulti
{
  public partial class DuplicateFinderMultiRibbon
  {
    private void DuplicateFinderMultiRibbon_Load(object sender, RibbonUIEventArgs e)
    {

    }

    private void btnShowHidePane_Click(object sender, RibbonControlEventArgs e)
    {
      if (Globals.ThisAddIn.IsPaneVisible)
        Globals.ThisAddIn.RemoveAllTaskPanes();
      else
        Globals.ThisAddIn.AddAllTaskPanes();
    }

    private void btnRegister_Click(object sender, RibbonControlEventArgs e)
    {
      VM.ViewModelLocator.DialogService.OpenRegisterWindow();

      foreach (Document Doc in Globals.ThisAddIn.Application.Documents)
      {
        var DupTaskPanes = Globals.ThisAddIn.CustomTaskPanes.Where(tp => tp.Title.StartsWith("DuplicateFinderMulti"));

        foreach (var TP in DupTaskPanes)
        {
          var UC = TP.Control as DuplicateFinderMultiPaneUC;

          if (UC != null)
          {
            var MV = UC.EH.Child as Views.MainView;

            if (MV != null)
            {
              ((VM.MainVM)MV.DataContext).StartCommand.RaiseCanExecuteChanged();
            }
          }
        }
      }
    }

    private void btnAbout_Click(object sender, RibbonControlEventArgs e)
    {
      VM.ViewModelLocator.DialogService.OpenAboutWindow();
    }
  }
}
