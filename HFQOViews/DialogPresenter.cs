﻿namespace HFQOViews
{
  public class DialogPresenter : ViewsBase.DialogPresenter, HFQOVM.IDialogService
  {
    public DialogPresenter(string title) : base(title)
    {

    }

    public bool ShowExamsListDialog()
    {
      var w = new ExamsListDialog();
      MakeChild(w); //Show this dialog as child of Microsoft Word window.
      var Result = w.ShowDialog().Value;
      return Result;
    }
  }
}