using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;

namespace MultiDF.VM
{
  public class DiffVM : ViewModelBase
  {
    public DiffVM()
    {      
      if(IsInDesignMode)
      {
        //PerformDiffCommand.Execute((Properties.Resources.Q10, Properties.Resources.Q34));
      }
    }

    public List<DiffPiece> Q1 { get; private set; }
    public List<DiffPiece> Q2 { get; private set; }

    public List<DiffPiece> A1 { get; private set; }
    public List<DiffPiece> A2 { get; private set; }

    private RelayCommand<(string, string, List<string>, List<string>)> _PerformDiffCommand;
    public RelayCommand<(string, string, List<string>, List<string>)> PerformDiffCommand
    {
      get
      {
        if (_PerformDiffCommand == null)
        {
          _PerformDiffCommand = new RelayCommand<(string q1, string q2, List<string> a1, List<string> a2)>((args) =>
          {
            var diff = new SideBySideDiffBuilder(new Differ());
            var Result = diff.BuildDiffModel(args.q1, args.q2, false);

            Q1 = Result.OldText.Lines;
            RaisePropertyChanged(nameof(Q1));

            Q2 = Result.NewText.Lines;
            RaisePropertyChanged(nameof(Q2));

            var diff2 = new SideBySideDiffBuilder(new Differ());

            var Acopy = new List<string>(args.a1);
            var Bcopy = new List<string>(args.a2);

            MinDistanceSort.Sort(Acopy, Bcopy, true, Fastenshtein.Levenshtein.Distance);
            
            var Result2 = diff2.BuildDiffModel(string.Join(Environment.NewLine, Acopy), string.Join(Environment.NewLine, Bcopy), false);

            A1 = Result2.OldText.Lines;
            RaisePropertyChanged(nameof(A1));

            A2 = Result2.NewText.Lines;
            RaisePropertyChanged(nameof(A2));


          },
          (args) => true);
        }

        return _PerformDiffCommand;
      }
    }
  }
}