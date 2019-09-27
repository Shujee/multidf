using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using System.IO;
using System.Threading;
using System.Linq;

namespace DuplicateFinderMulti.Test
{
  [TestClass]
  public class UnitTest1
  {
    CancellationTokenSource _TokenSource = new CancellationTokenSource();
    CancellationToken token;

    public UnitTest1()
    {
      SimpleIoc.Default.Register<IWordService, TestWordService>();

      token = _TokenSource.Token;
    }

    [TestMethod]
    public void TestQAExtractionFile1()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 1 for DF.docx");
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token);
      var QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paras, new System.Threading.CancellationToken());
      Assert.IsTrue(QAs.Count == 18);
      Assert.IsTrue(QAs[8].Choices.Count == 4);
      Assert.IsTrue(QAs[8].Answer == "C");
    }

    [TestMethod]
    public void TestQAExtractionFile2()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 2 for DF.docx");
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token);
      var QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paras, new System.Threading.CancellationToken());

      Assert.IsTrue(QAs.Count == 10);
      Assert.IsTrue(QAs[4].Choices.Count == 4);
      Assert.IsTrue(QAs[9].Choices.Count == 0);
      Assert.IsTrue(QAs[9].Answer == "1, 2");
    }

    [TestMethod]
    public void TestQAExtractionFile3()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 3 for DF.docx");
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token);
      var QAs = ViewModelLocator.QAExtractionStrategy.Extract(Paras, new System.Threading.CancellationToken());

      Assert.IsTrue(QAs.Count == 10);
      Assert.IsTrue(QAs[5].Choices.Count == 3);
      Assert.IsTrue(QAs[9].Answer == "Drive Maps, Update");
    }

    [TestMethod]
    public void TestDefaultQAComparer()
    {
      var Q1 = new QA()
      {
        Question = "A very simple question",
        Choices = new System.Collections.Generic.List<string>()
                        {
                          "Choice 1",
                          "Choice 2",
                          "Choice 3",
                          "Choice 4",
                          "Choice 5",
                        }
      };

      var Q2 = new QA()
      {
        Question = "A very simple question with minor change",
        Choices = new System.Collections.Generic.List<string>()
                        {
                          "Choice 2",
                          "Choice 5",
                          "Choice 41",
                          "Choice 3",
                          "Choice 1",
                        }
      };

      var Dist = ViewModelLocator.QAComparer.Distance(Q1, Q2, true);
      Assert.IsTrue(Dist > 0);
    }

    [TestMethod]
    public void QAEqualityTest()
    {
      var Q1 = new QA()
      {
        Doc = "abc.docx",
        Index = 33,
        Question = "A very simple question",
        Choices = new System.Collections.Generic.List<string>()
                        {
                          "Choice 1",
                          "Choice 2",
                          "Choice 3",
                          "Choice 4",
                          "Choice 5",
                        }
      };

      var Q2 = new QA()
      {
        Doc = "abc.docx",
        Index = 33,
        Question = "Question changed",
        Choices = new System.Collections.Generic.List<string>()
                        {
                          "Choice 6",
                          "Choice 7",
                        }
      };

      var Q3 = new QA()
      {
        Doc = "abc.docx",
        Index = 34,
        Question = "Question changed",
        Choices = new System.Collections.Generic.List<string>()
                        {
                          "Choice 6",
                          "Choice 7",
                        }
      };

      Assert.IsTrue(Q1.Equals(Q2));
      Assert.IsFalse(Q2.Equals(Q3));
    }

    [TestMethod]
    public void ProjectSerializationTest()
    {
      Project P = new Project()
      {
        IsDirty = true,
        Name = "Project 1",
        XMLDocs = new System.Collections.ObjectModel.ObservableCollection<XMLDoc>(
              Enumerable.Range(1, Faker.RandomNumber.Next(10, 60)).Select(i => CreateRandomXMLDoc()))
      };

      var MyXML = P.SerializeDC();
      var P2 = MyXML.DeserializeDC<Project>();

      Assert.AreEqual(P.Name, P2.Name);
      Assert.AreEqual(P.XMLDocs.Count, P2.XMLDocs.Count);
      Assert.AreEqual(P.XMLDocs[0].QAs.Count, P2.XMLDocs[0].QAs.Count);
    }

    private XMLDoc CreateRandomXMLDoc()
    {
      var Doc = new XMLDoc
      {
        Name = Faker.Name.FullName(Faker.NameFormats.Standard),
        LastModified = DateTime.Now.AddDays(Faker.RandomNumber.Next(-100, 100)),
        SourcePath = @"F:\Some\Path",
        Size = Faker.RandomNumber.Next(10000, 1000000)
      };

      Doc.QAs = Enumerable.Range(1, Faker.RandomNumber.Next(10, 60)).Select(i => CreateRandomQA(Doc, i)).ToList();

      return Doc;
    }

    private QA CreateRandomQA(XMLDoc parent, int index)
    {
      var QA = new QA
      {
        Answer = Faker.Lorem.GetFirstWord(),
        Choices = Faker.Lorem.Paragraphs(Faker.RandomNumber.Next(3, 6)).ToList(),
        Doc = parent.SourcePath,
        Start = Faker.RandomNumber.Next(1, 1000),
        Index = index,
        Question = Faker.Lorem.Sentence(50)
      };

      QA.End = QA.Start + Faker.RandomNumber.Next(100, 500);

      return QA;
    }
  }
}
