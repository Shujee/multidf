using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  public class DiffVM : ViewModelBase
  {
    public DiffVM()
    {      
      if(IsInDesignMode)
      {
        PerformDiffCommand.Execute((Properties.Resources.Q10, Properties.Resources.Q34));
      }
    }

    public List<DiffPiece> Para1 { get; private set; }
    public List<DiffPiece> Para2 { get; private set; }

    private RelayCommand<(string, string)> _PerformDiffCommand;
    public RelayCommand<(string, string)> PerformDiffCommand
    {
      get
      {
        if (_PerformDiffCommand == null)
        {
          _PerformDiffCommand = new RelayCommand<(string text1, string text2)>((args) =>
          {
            var diff = new SideBySideDiffBuilder(new Differ());
            var Result = diff.BuildDiffModel(args.text1, args.text2, false);

            Para1 = Result.OldText.Lines;
            RaisePropertyChanged(nameof(Para1));

            Para2 = Result.NewText.Lines;
            RaisePropertyChanged(nameof(Para2));
          },
          (args) => true);
        }

        return _PerformDiffCommand;
      }
    }
  }
}