using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

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

    public string Name => string.IsNullOrEmpty(_SourcePath) ? "" : Path.GetFileName(_SourcePath);

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
        if (!string.IsNullOrEmpty(_SourcePath) && File.Exists(_SourcePath))
        {
          RaisePropertyChanged(nameof(SyncInfo));

          FileInfo fileInfo = new FileInfo(_SourcePath);
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
        if (!string.IsNullOrEmpty(_SourcePath) && File.Exists(_SourcePath))
        {
          FileInfo fileInfo = new FileInfo(_SourcePath);

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
            if (!string.IsNullOrEmpty(_SourcePath) && File.Exists(_SourcePath))
              ViewModelLocator.WordService.OpenDocument(_SourcePath, null, null);
            else
              ViewModelLocator.DialogService.ShowMessage("Source document does not exist.", true);
          },
          () => !string.IsNullOrEmpty(_SourcePath) && File.Exists(_SourcePath));
        }

        return _OpenSourceCommand;
      }
    }

    public Task UpdateQAs()
    {
      if (!string.IsNullOrEmpty(_SourcePath) && File.Exists(_SourcePath))
      {
        return Task.Run(() =>
        {
          ViewModelLocator.Main.UpdateProgress(true, "Importing...", 0);

          var Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(_SourcePath, _Token, UpdateQAsProgressHandler);

          if (Paragraphs != null)
          {
            try
            {
              QAs = ViewModelLocator.QAExtractionStrategy.ExtractQAs(Paragraphs, _Token);
            }
            catch (Exception ex)
            {
              if (ex.Data.Contains("Paragraph"))
              {
                var Res = ViewModelLocator.DialogService.AskBooleanQuestion(ex.Message + Environment.NewLine + Environment.NewLine + "Do you want to open source document to fix this problem?");
                if (Res)
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

    private void UpdateQAsProgressHandler(int i, int Total)
    {
        ProcessingProgress = (i / (float)Total) * 100;
        ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.ElapsedTime));
        ViewModelLocator.Main.RaisePropertyChanged(nameof(MainVM.EstimatedRemainingTime));
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

    #region "Serialize/De-serialize"
    private static readonly Type[] AllObjectTypes = {
      typeof(QA),
    };

    private static readonly DataContractSerializer DSSerializer = new DataContractSerializer(typeof(XMLDoc), AllObjectTypes, 0x7F_FFFF, false, true, null); //max graph size is 8,388,607‬ items

    /// <summary>
    /// This setting is required to prevent exceptions that are thrown by the serialization when it encounters control characters used by
    /// Microsoft Word to denote newlines and non-breaking newlines.
    /// </summary>
    private static readonly XmlWriterSettings xmlWriterSettingsForWordDocs = new XmlWriterSettings()
    {
      NewLineChars = "\a\r\n",
      CheckCharacters = false,
      Encoding = Encoding.UTF8,
      NewLineHandling = NewLineHandling.Entitize,
    };

    public static XMLDoc FromXML(string xml)
    {
      try
      {
        StringReader s = new StringReader(xml);

        using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        {
          return (XMLDoc)DSSerializer.ReadObject(reader, true);
        }
      }
      catch
      {
        return null;
      }
    }

    public string ToXML()
    {
      StringBuilder sb = new StringBuilder();

      StringWriter writer = new StringWriter(sb);
      using (XmlWriter xmlWriter = XmlWriter.Create(writer, xmlWriterSettingsForWordDocs))
      {
        DSSerializer.WriteStartObject(xmlWriter, this);
        DSSerializer.WriteObjectContent(xmlWriter, this);
        DSSerializer.WriteEndObject(xmlWriter);
      }

      return sb.ToString();
    }
    #endregion
  }
}