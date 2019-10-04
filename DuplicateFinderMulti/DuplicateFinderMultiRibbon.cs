using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System.Linq;

namespace DuplicateFinderMulti
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Scope = "module", Justification = "Built-in arguments 'sender' and 'e' are not needed in the event handlers below")]
  public partial class DuplicateFinderMultiRibbon
  {
    private void ShowHidePaneButton_Click(object sender, RibbonControlEventArgs e)
    {
      if (Globals.ThisAddIn.IsPaneVisible)
        Globals.ThisAddIn.RemoveAllTaskPanes();
      else
        Globals.ThisAddIn.AddAllTaskPanes();
    }

    private void RegisterButton_Click(object sender, RibbonControlEventArgs e)
    {
      VM.ViewModelLocator.DialogService.OpenRegisterWindow();

      foreach (Document Doc in Globals.ThisAddIn.Application.Documents)
      {
        var DupTaskPanes = Globals.ThisAddIn.CustomTaskPanes.Where(tp => tp.Title.StartsWith("DuplicateFinderMulti"));

        foreach (var TP in DupTaskPanes)
        {
          if (TP.Control is DuplicateFinderMultiPaneUC UC)
          {
            if (UC.EH.Child is Views.MainView MV)
            {
              ((VM.MainVM)MV.DataContext).SelectedProject.ProcessCommand.RaiseCanExecuteChanged();
            }
          }
        }
      }
    }

    private void AboutButton_Click(object sender, RibbonControlEventArgs e)
    {
      VM.ViewModelLocator.DialogService.OpenAboutWindow();
    }
  }
}
