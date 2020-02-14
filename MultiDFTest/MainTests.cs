using MultiDF.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickGraph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VMBase;

namespace MultiDF.Test
{
  [TestClass]
  public class MainTests : TestBase
  {
    [TestMethod]
    public void TestQAExtractionFile1()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 1 for DF.docx");
      var Paras = VM.ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token, null);
      var QAs = VM.ViewModelLocator.QAExtractionStrategy.ExtractQAs(Paras, new System.Threading.CancellationToken());
      Assert.IsTrue(QAs.Count == 18);
      Assert.IsTrue(QAs[8].Choices.Count == 4);
      Assert.IsTrue(QAs[8].Answer == "C");
    }

    [TestMethod]
    public void TestQAExtractionFile2()
    {
      var DirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      var FileName = System.IO.Path.Combine(DirName, "File 2 for DF.docx");
      var Paras = VM.ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token, null);
      var QAs = VM.ViewModelLocator.QAExtractionStrategy.ExtractQAs(Paras, new System.Threading.CancellationToken());

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
      var Paras = VM.ViewModelLocator.WordService.GetDocumentParagraphs(FileName, token, null);
      var QAs = VM.ViewModelLocator.QAExtractionStrategy.ExtractQAs(Paras, new System.Threading.CancellationToken());

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

      var QAComparer = new VM.DefaultQAComparer();
      var Dist = QAComparer.Distance(Q1, Q2, true);
      Assert.IsTrue(Dist > 0);
    }

    [TestMethod]
    public void QAEqualityTest()
    {
      var D1 = CreateRandomXMLDoc();

      var Q1 = new QA()
      {
        Doc = D1,
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
        Doc = D1,
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
        Doc = D1,
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
      };

      var Docs = Enumerable.Range(1, Faker.RandomNumber.Next(10, 60)).Select(i => CreateRandomXMLDoc());
      foreach (var D in Docs)
      {
        P.AllXMLDocs.Add(D);
      }

      var MyXML = P.ToXML();
      var P2 = Project.FromXML(MyXML);

      Assert.AreEqual(P.Name, P2.Name);
      Assert.AreEqual(P.AllXMLDocs.Count, P2.AllXMLDocs.Count);
      Assert.AreEqual(P.AllXMLDocs[0].QAs.Count, P2.AllXMLDocs[0].QAs.Count);
    }

    [TestMethod]
    public void DFResultKeyEqualityTest()
    {
      DFResultRow k1 = new DFResultRow(1, 3, .5);
      DFResultRow k2 = new DFResultRow(1, 4, .6);
      DFResultRow k3 = new DFResultRow(3, 1, .7);

      Assert.IsFalse(k1.Equals(k2));
      Assert.IsFalse(k2.Equals(k1));

      Assert.IsTrue(k1.Equals(k3));
      Assert.IsTrue(k3.Equals(k1));
    }

    [TestMethod]
    public void DFResultDictionaryTest()
    {
      DFResultRow k1 = new DFResultRow(1, 3, 0.5);
      DFResultRow k3 = new DFResultRow(3, 1, 0.7);

      Dictionary<DFResultRow, double> Dic = new Dictionary<DFResultRow, double>
      {
        { k1, 20 }
      };
      Assert.IsTrue(Dic.ContainsKey(k3));
    }

    [TestMethod]
    public void DocComparerTest()
    {
      DefaultDocComparer Comparer = new DefaultDocComparer();
      Comparer.QACompared += (sender, args) =>
      {
        System.Diagnostics.Debug.WriteLine($"{args.QA1.Doc} ({args.QA1.Index}) - {args.QA2.Doc} ({args.QA2.Index}) => {args.Distance}");
      };

      var Doc1 = base.CreateRandomXMLDoc();
      var Doc2 = CreateRandomXMLDoc();

      //add closely matching QAs to both docs to check if the distance is computed correctly similar (distance should be zero). 
      var BlackSheep1 = new QA()
      {
        Answer = "A",
        Question = "A question",
        Choices = new List<string> { "A", "B", "C", "D", "E" },
        Doc = Doc1,
        Start = 1,
        End = 200,
        Index = 100
      };

      var BlackSheep2 = new QA()
      {
        Answer = "A",
        Question = "A question",
        Choices = new List<string> { "A", "B", "C", "D", "E" },
        Doc = Doc2,
        Start = 201,
        End = 400,
        Index = 106
      };

      Doc1.QAs.Add(BlackSheep1);
      Doc2.QAs.Add(BlackSheep2);

      var QAComparer = new VM.DefaultQAComparer();
      var ResultTask = Comparer.Compare(Doc1, Doc2, QAComparer, true, token);

      ResultTask.ContinueWith(t =>
      {
        System.Diagnostics.Debug.WriteLine($"FINAL RESULT COUNT: {t.Result.Items.Count}");
        Assert.IsTrue(t.IsCanceled);
      });

      Task.Delay(5000).Wait();

      _TokenSource.Cancel();

      ResultTask.Wait(10000);



      //Assert.AreEqual(ResultTask.Result.Count, Doc1.QAs.Count * Doc2.QAs.Count);

      //Assert.AreEqual(ResultTask.Result[new DFResultKey(BlackSheep1, BlackSheep2)], 0);
    }

    [TestMethod]
    public void QuickGraphTest()
    {
      var QAComparer = new VM.DefaultQAComparer();
      using (var P = CreateRandomProject())
      {
        UndirectedGraph<XMLDoc, OurEdge> graph = new UndirectedGraph<XMLDoc, OurEdge>();
        graph.AddVertexRange(P.AllXMLDocs);

        List<Task> Tasks = new List<Task>();
        foreach (var V1 in graph.Vertices)
        {
          foreach (var V2 in graph.Vertices)
          {
            if (V1 != V2 && !graph.ContainsEdge(V1, V2))
            {
              var Edge = new OurEdge(V1, V2, null);

              if (graph.AddEdge(Edge))
              {
                var Task = VM.ViewModelLocator.DocComparer.Compare(V1, V2, QAComparer, true, token);
                Task.ContinueWith(t =>
                  {
                    Edge.Tag = t.Result;
                    System.Diagnostics.Debug.WriteLine($"{V1.Name} ({V1.QAs.Count}) - {V2.Name} ({V2.QAs.Count}): Result = {t.Result.Items.Count}");
                  });

                Tasks.Add(Task);
              }
            }
          }
        }

        Task.WaitAll(Tasks.ToArray());
      }
    }

    [TestMethod]
    public void SerializeTest()
    {
      GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Unregister<IDialogService>();
      GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<IDialogService, DialogServiceForAutomatedTesting>();

      using (var p = CreateRandomProject())
      {
        p.SavePath = null;
        p.AddDocsCommand.Execute(null);

        while (p.AllXMLDocs.Any(d => d.QAs == null))
          System.Threading.Thread.Sleep(300);

        p.ProcessCommand.Execute(null);

        while (p.IsProcessing)
          System.Threading.Thread.Sleep(300);

        var SavePath = ViewModelLocator.DialogService.ShowSave("XML Files (*.xml)|*.xml");
        File.WriteAllText(SavePath, p.ToXML());

        var P2 = Project.FromXML(File.ReadAllText(SavePath));
        Assert.IsTrue(P2 != null && P2.AllXMLDocs.Count == p.AllXMLDocs.Count && P2.Graph.EdgeCount == p.Graph.EdgeCount && P2.Graph.VertexCount == p.Graph.VertexCount);
      }
    }
  }
}
