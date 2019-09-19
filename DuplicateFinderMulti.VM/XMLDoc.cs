using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  public class XMLDoc : ObservableObject
  {
    private string _Name;
    public string Name
    {
      get => _Name;
      set => Set(ref _Name, value);
    }
    
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
          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);
          return fileInfo.LastWriteTimeUtc == LastModified && fileInfo.Length == Size;
        }
        else
          return false;
      }
    }

    public string SyncErrors
    {
      get
      {
        if (!string.IsNullOrEmpty(SourcePath) && System.IO.File.Exists(SourcePath))
        {
          System.IO.FileInfo fileInfo = new System.IO.FileInfo(SourcePath);

          string Errors = "";

          if(fileInfo.LastWriteTimeUtc != LastModified)
          {
            Errors += "Last Modified Date (local): " + LastModified.ToString();
            Errors += Environment.NewLine + "Last Modified Date (source): " + fileInfo.LastWriteTimeUtc.ToString();
          }

          if (fileInfo.Length == Size)
          {
            Errors += Environment.NewLine + "File Size (local): " + Size.ToString();
            Errors += Environment.NewLine + "File Size (source): " + fileInfo.Length.ToString();
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
          () => !string.IsNullOrEmpty( _SourcePath) && System.IO.File.Exists(_SourcePath));
        }

        return _OpenSourceCommand;
      }
    }

    public void UpdateQAs()
    {
      if (!string.IsNullOrEmpty(_SourcePath) && System.IO.File.Exists(_SourcePath))
      {
        CancellationToken c = new CancellationToken();
        var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(_SourcePath);
        QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paragraphs, c);
        RaisePropertyChanged(nameof(Paragraphs));
      }
    }
  }
}