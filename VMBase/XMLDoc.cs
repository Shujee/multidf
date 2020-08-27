using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;

namespace VMBase
{
  public class XMLDoc : ObservableObject, IDisposable
  {
    //This class allows QA Update process to be cancelled at any stage. The following token objects provide task cancellation mechanism.
    public readonly CancellationTokenSource _TokenSource;
    public CancellationToken _Token;

    public XMLDoc()
    {
      _TokenSource = new CancellationTokenSource();
      _Token = _TokenSource.Token;
    }

    public string Name => string.IsNullOrEmpty(_SourcePath) ? "" : Path.GetFileName(_SourcePath);

    protected string _SourcePath;
    public string SourcePath
    {
      get => _SourcePath;
      set
      {
        Set(ref _SourcePath, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }

    protected long _Size;
    public long Size
    {
      get => _Size;
      set
      {
        Set(ref _Size, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }

    protected double _ProcessingProgress;

    public double ProcessingProgress
    {
      get => _ProcessingProgress;
      set => Set(ref _ProcessingProgress, value);
    }


    protected DateTime _LastModified;
    public DateTime LastModified
    {
      get => _LastModified;
      set
      {
        Set(ref _LastModified, value);
        RaisePropertyChanged(nameof(IsSyncWithSource));
      }
    }

    protected List<QA> _QAs;
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

          //FAT file system does not store timezone info, so fileInfo.LastWriteTime is interpretted as UTC time and shifted by timezone
          //offset of the machine. On the other hand, our XML stores LastModified property with full timezone info and can be interpretted
          //correctly on any machine. This makes it impossible to compare file's time to our property value. 
          //Here we try to do a "fuzzy" comparison between these two timestamps by seeing if it difference is less than 12 hours and
          //is an integer multiple of 30 minutes (consecutive timezones are always 30-minutes away for each other)

          var TimeStampDiff = Math.Abs(Math.Round(LastModified.Subtract(fileInfo.LastWriteTime).TotalHours, 3));

          return (
                  fileInfo.LastWriteTimeUtc == LastModified.ToUniversalTime() ||
                  (TimeStampDiff <= 12 && TimeStampDiff % 0.5 == 0)
                 )
                  && 
                  fileInfo.Length == Size;
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
            LastModified1 = fileInfo.LastWriteTimeUtc,
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
    protected static readonly Type[] AllObjectTypes = {
      typeof(QA),
      typeof(List<QA>),
      typeof(XMLDoc),
    };

    protected static readonly DataContractSerializer DSSerializer = new DataContractSerializer(typeof(XMLDoc), AllObjectTypes, 0x7F_FFFF, false, true, null); //max graph size is 8,388,607‬ items

    /// <summary>
    /// This setting is required to prevent exceptions that are thrown by the serialization when it encounters control characters used by
    /// Microsoft Word to denote newlines and non-breaking newlines.
    /// </summary>
    protected static readonly XmlWriterSettings xmlWriterSettingsForWordDocs = new XmlWriterSettings()
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
        //fix for xml files created with previous version of MultiDF
        xml = xml.Replace("http://schemas.datacontract.org/2004/07/MultiDF.VM", "http://schemas.datacontract.org/2004/07/VMBase");
        xml = xml.Replace("http://schemas.datacontract.org/2004/07/MultiDF.VM", "http://schemas.datacontract.org/2004/07/VMBase");

        using (StringReader s = new StringReader(xml))
        {
          using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
          {
            return (XMLDoc)DSSerializer.ReadObject(reader, true);
          }
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

    public void CancelUpdateQAs()
    {
      _TokenSource.Cancel();
    }
  }
}