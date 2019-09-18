using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// Represents a DuplicateFinderMulti project. A project is a set of input files and ad hoc/complete results.
  /// </summary>
  public class Project : GalaSoft.MvvmLight.ObservableObject
  {
    private string _SavePath;
    public string SavePath
    {
      get => _SavePath;
      set => Set(ref _SavePath, value);
    }

    private string _Name;
    public string Name
    {
      get => _Name;
      set => Set(ref _Name, value);
    }
    
    private bool _IsDirty;
    public bool IsDirty
    {
      get => _IsDirty;
      set
      {
        Set(ref _IsDirty, value);
        SaveCommand.RaiseCanExecuteChanged();
      }
    }

    private ObservableCollection<XMLDoc> _XMLDocs = new ObservableCollection<XMLDoc>();
    public ObservableCollection<XMLDoc> XMLDocs
    {
      get { return _XMLDocs; }
      set => Set(ref _XMLDocs, value);
    }

    private XMLDoc _SelectedDoc;
    public XMLDoc SelectedDoc
    {
      get => _SelectedDoc;
      set
      {
        Set(ref _SelectedDoc, value);
        RemoveSelectedDocCommand.RaiseCanExecuteChanged();
      }
    }

    private RelayCommand _AddDocsCommand;
    public RelayCommand AddDocsCommand
    {
      get
      {
        if (_AddDocsCommand == null)
        {
          _AddDocsCommand = new RelayCommand(() =>
          {
            var Docs = ViewModelLocator.DialogService.ShowOpenMulti("Word documents (*.doc, *.docx, *.docm)|*.doc;*.docx;*.docm");

            if (Docs != null)
            {
              foreach (var Doc in Docs)
              {
                System.IO.FileInfo FileInfo = new System.IO.FileInfo(Doc);

                this._XMLDocs.Add(new XMLDoc()
                {
                  Name = System.IO.Path.GetFileNameWithoutExtension( Doc),
                  Paragraphs = ViewModelLocator.WordService.GetDocumentParagraphs(Doc),
                  LastModified = FileInfo.LastWriteTimeUtc,
                  Size = FileInfo.Length,
                  SourcePath = Doc
                });
              }

              this.IsDirty = true;
            }
          },
          () => true);
        }

        return _AddDocsCommand;
      }
    }


    private RelayCommand _RemoveSelectedDocCommand;
    public RelayCommand RemoveSelectedDocCommand
    {
      get
      {
        if (_RemoveSelectedDocCommand == null)
        {
          _RemoveSelectedDocCommand = new RelayCommand(() =>
          {
            this._XMLDocs.Remove(_SelectedDoc);
            this.IsDirty = true;
          },
          () => _SelectedDoc != null);
        }

        return _RemoveSelectedDocCommand;
      }
    }

    private RelayCommand _SaveCommand;
    public RelayCommand SaveCommand
    {
      get
      {
        if (_SaveCommand == null)
        {
          _SaveCommand = new RelayCommand(() =>
          {
            if (_SavePath == null)
            {
              _SavePath = ViewModelLocator.DialogService.ShowSave(MainVM.FILTER_XML_FILES);
            }

            if (_SavePath != null)
            {
              string ProjectXML = this.ToXML();
              System.IO.File.WriteAllText(_SavePath, ProjectXML);
              this.IsDirty = false;
              SaveCommand.RaiseCanExecuteChanged();
            }
          },
          () => this.IsDirty);
        }

        return _SaveCommand;
      }
    }


    public static Project FromXML(string xml)
    {
      return xml.Deserialize<Project>();
    }

    public string ToXML()
    {
      return this.Serialize();
    }
  }
}
