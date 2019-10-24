using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class XMLDoc : ObservableObject, IDisposable
  {
    //This class allows QA Update process to be cancelled at any stage. The following token objects provide task cancellation mechanism.
    private readonly CancellationTokenSource _TokenSource;
    private CancellationToken _Token;

    public XMLDoc()
    {
      _TokenSource = new CancellationTokenSource();
      _Token = _TokenSource.Token;
    }

    public string Name => string.IsNullOrEmpty(_SourcePath) ? "" : System.IO.Path.GetFileName(_SourcePath);

    private string _SourcePath;
    public string SourcePath
    {
      get => _SourcePath;
      set
      {
        Set(ref _SourcePath, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }

    private long _Size;
    public long Size
    {
      get => _Size;
      set
      {
        Set(ref _Size, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }


    private double _ProcessingProgress;

    //[IgnoreDataMember]
    public double ProcessingProgress
    {
      get => _ProcessingProgress;
      set => Set(ref _ProcessingProgress, value);
    }


    private DateTime _LastModified;
    public DateTime LastModified
    {
      get => _LastModified;
      set
      {
        Set(ref _LastModified, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }

    private List<QA> _QAs;
    public List<QA> QAs
    {
      get => _QAs;
      set => Set(ref _QAs, value);
    }

    /// <summary>
    /// Compares the size and last modified date properties with source file to see if there is a change.
    /// </summary>
    /// <returns></returns>
    public bool IsSyncWithSource
    {
      get
      {
        if (!string.IsNullOrEmpty(SourcePath) && System.IO.File.Exists(SourcePath))
        {
          RaisePropertyChanged(nameof(SyncInfo));

          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);
          return fileInfo.LastWriteTime == LastModified && fileInfo.Length == Size;
        }
        else
          return false;
      }
    }

    public FileAttributesComparison SyncInfo
    {
      get
      {
        if (!string.IsNullOrEmpty(SourcePath) && System.IO.File.Exists(SourcePath))
        {
          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);

          var Info = new FileAttributesComparison
          {
            LastModified1 = fileInfo.LastWriteTime,
            LastModified2 = LastModified,

            Size1 = fileInfo.Length,
            Size2 = Size
          };

          return Info;
        }
        else
          return null;
      }
    }

    private RelayCommand _OpenSourceCommand;
    public RelayCommand OpenSourceCommand
    {
      get
      {
        if (_OpenSourceCommand == null)
        {
          _OpenSourceCommand = new RelayCommand(() =>
          {
            if (!string.IsNullOrEmpty(_SourcePath) && System.IO.File.Exists(_SourcePath))
              ViewModelLocator.WordService.OpenDocument(_SourcePath);
            else
              ViewModelLocator.DialogService.ShowMessage("Source document does not exist.", true);
          },
          () => !string.IsNullOrEmpty(_SourcePath) && System.IO.File.Exists(_SourcePath));
        }

        return _OpenSourceCommand;
      }
    }

    public Task UpdateQAs()
    {
      if (!string.IsNullOrEmpty(_SourcePath) && System.IO.File.Exists(_SourcePath))
      {
        return Task.Run(() =>
        {
          var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(_SourcePath, _Token, (i, Total) => ProcessingProgress = (i / (float)Total) * 100);

          if (Paragraphs != null)
          {
            try
            {
              QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paragraphs, _Token);
            }
            catch (Exception ex)
            {
              if(ex.Data.Contains("Paragraph"))
              {
                var Res = ViewModelLocator.DialogService.AskBooleanQuestion(ex.Message + Environment.NewLine + Environment.NewLine + "Do you want to open source document to fix this problem?");
                if(Res)
                {
                  ViewModelLocator.WordService.OpenDocument(_SourcePath, (int)ex.Data["Paragraph"], (int)ex.Data["Paragraph"]);
                }
              }
            }

            if (QAs != null)
            {
              foreach (var QA in QAs)
                QA.Doc = this;
            }

            RaisePropertyChanged(nameof(QAs));
          }
        });
      }
      else
        return null;
    }

    public void CancelUpdateQAs()
    {
      _TokenSource.Cancel();
    }

    public void Dispose()
    {
      Dispose(true);
    }

    // The bulk of the clean-up code is implemented in Dispose(bool)
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // free managed resources
        if (_TokenSource != null)
          _TokenSource.Dispose();
      }
    }
  }
}