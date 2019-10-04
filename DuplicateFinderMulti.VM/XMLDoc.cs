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
          RaisePropertyChanged(nameof(SyncErrors));

          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);
          return fileInfo.LastWriteTimeUtc == LastModified && fileInfo.Length == Size;
        }
        else
          return false;
      }
    }

    public FileAttributesComparison SyncErrors
    {
      get
      {
        if (!string.IsNullOrEmpty(SourcePath) && System.IO.File.Exists(SourcePath))
        {
          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);

          var Errors = new FileAttributesComparison();

          if (fileInfo.LastWriteTimeUtc != LastModified)
          {
            Errors.LastModified1 = fileInfo.LastWriteTimeUtc;
            Errors.LastModified2 = LastModified;
          }

          if (fileInfo.Length != Size)
          {
            Errors.Size1 = fileInfo.Length;
            Errors.Size2 = Size;
          }

          return Errors;
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


    

    public void UpdateQAs()
    {
      if (!string.IsNullOrEmpty(_SourcePath) && System.IO.File.Exists(_SourcePath))
      {
        Task.Run(() =>
        {
          var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(_SourcePath, _Token, (i, Total) => ProcessingProgress = (i / (float)Total) * 100);

          if (Paragraphs != null)
          {
            QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paragraphs, _Token);

            if (QAs != null)
            {
              foreach (var QA in QAs)
                QA.Doc = this;
            }

            RaisePropertyChanged(nameof(QAs));
          }
        });
      }
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