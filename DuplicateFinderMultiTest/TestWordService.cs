using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuplicateFinderMulti.VM;
using Microsoft.Office.Interop.Word;

namespace DuplicateFinderMulti.Test
{
  class TestWordService : VM.IWordService
  {
    Application App;

    public TestWordService()
    {
      App = new Application();
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath)
    {
      if (!string.IsNullOrEmpty(docPath) && System.IO.File.Exists(docPath))
      {
        List<WordParagraph> Result = new List<WordParagraph>();

        var Doc = App.Documents.Open(docPath, ReadOnly: true, AddToRecentFiles: false, Visible: false);

        foreach (Paragraph p in Doc.Paragraphs)
        {
          var PType = ParagraphType.Text;

          if (p.Range.Tables.Count > 0)
          {
            if (p.Range.Tables[1].ApplyStyleHeadingRows && p.Range.Rows.First.Index == 1)
              PType = ParagraphType.TableHeader;
            else
              PType = ParagraphType.TableRow;
          }
          else if (p.Range.ListFormat.ListType == WdListType.wdListSimpleNumbering)
            PType = ParagraphType.NumberedList;


          Result.Add(new WordParagraph(p.Range.Text, p.Range.Start, p.Range.End, PType));
        }

        Doc.Close(SaveChanges: false);

        return Result;
      }
      else
        return null;
    }

    public void OpenDocument(string docPath)
    {
      
    }
  }
}
