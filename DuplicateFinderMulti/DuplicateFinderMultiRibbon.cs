using DuplicateFinderMulti.VM;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Tools.Ribbon;
using System.Linq;

namespace DuplicateFinderMulti
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Scope = "module", Justification = "Built-in arguments 'sender' and 'e' are not needed in the event handlers below")]
  public partial class DuplicateFinderMultiRibbon
  {
    private void DuplicateFinderMultiRibbon_Load(object sender, RibbonUIEventArgs e)
    {
      ViewModelLocator.Auth.PropertyChanged += Main_PropertyChanged;

      var ExpiryDate = VM.ViewModelLocator.Register.ExpiryDate;

      if (ExpiryDate == null)
      {
        btnShowHidePane.Visible = false;
        btnRegister.Visible = true;
      }
      else
      {
        var IsExpired = (ExpiryDate.Value.Subtract(System.DateTime.Now).TotalDays < 7);
        btnShowHidePane.Visible = !IsExpired;
      }
    }

    private void Main_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ViewModelLocator.Auth.IsLoggedIn) || e.PropertyName == nameof(ViewModelLocator.Auth.IsCommunicating))
      {
        btnLogin.Enabled = !(ViewModelLocator.Auth.IsLoggedIn || ViewModelLocator.Auth.IsCommunicating);
        btnLogout.Enabled = ViewModelLocator.Auth.IsLoggedIn;

        btnUploadExam.Enabled = ViewModelLocator.Auth.IsLoggedIn;
        btnUploadActive.Enabled = ViewModelLocator.Auth.IsLoggedIn;
        btnUploadActive.Enabled = ViewModelLocator.Auth.IsLoggedIn;
      }
    }

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

      btnShowHidePane.Visible = VM.ViewModelLocator.Register.IsRegistered;

      Globals.ThisAddIn.AddAllTaskPanes();

      foreach (Document Doc in Globals.ThisAddIn.Application.Documents)
      {
        var DupTaskPanes = Globals.ThisAddIn.CustomTaskPanes.Where(tp => tp.Title.StartsWith("Multi-DF"));

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

    private void btnLogin_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Auth.LoginCommand.Execute(null);
    }

    private void btnLogout_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Auth.LogoutCommand.Execute(null);
    }

    private void btnUploadExam_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Main.UploadExamCommand.Execute(null);
    }

    private void btnUploadActive_Click(object sender, RibbonControlEventArgs e)
    {
      ViewModelLocator.Main.UploadActiveExamCommand.Execute(null);
    }
  }
}
