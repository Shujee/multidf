using System.Threading.Tasks;
using System.Windows;

namespace HFQOViews
{
  public class DialogPresenter : ViewsBase.DialogPresenter, HFQOVM.IDialogService
  {
    public DialogPresenter(Window window, string title) : base(window, title)
    {

    }

    public async Task<Common.AccessibleMasterFile> ShowExamsListDialog()
    {
      var w = await ExamsListDialog.CreateAsync();
      MakeChild(w); //Show this dialog as child of Microsoft Word window.
      var Result = w.ShowDialog().Value;

      if (Result)
        return w.SelectedAccess;
      else
        return null;
    }
  }
}