using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using System.IO;

namespace DuplicateFinderMulti.Test
{
  [TestClass]
  public class UnitTest1
  {
    public UnitTest1()
    {
      SimpleIoc.Default.Register<IWordService, TestWordService>();
    }

    [TestMethod]
    public void TestQAExtractionFile1()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 1 for DF.docx");
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName);
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
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName);
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
      var Paras = ViewModelLocator.WordService.GetDocumentParagraphs(FileName);
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
  }
}
