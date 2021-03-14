using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiDF.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickGraph;
using VMBase;

namespace MultiDF.Test
{
  [TestClass]
  public class QuickGraphTests : TestBase
  {
    /// <summary>
    /// Checks whether removing a vertex drops its edges too.
    /// </summary>
    [TestMethod]
    public void VertexRemoveTest()
    {
      UndirectedGraph<string, TaggedUndirectedEdge<string, int>> graph = new UndirectedGraph<string, TaggedUndirectedEdge<string, int>>();

      graph.AddVertex("A");
      graph.AddVertex("B");
      graph.AddVertex("C");

      var Edge = new TaggedUndirectedEdge<string, int>("A", "B", 10);
      var Edge2 = new TaggedUndirectedEdge<string, int>("B", "C", 20);
      var Edge3 = new TaggedUndirectedEdge<string, int>("C", "A", 30);

      graph.AddEdge(Edge);
      graph.AddEdge(Edge2);
      graph.AddEdge(Edge3);

      graph.RemoveVertex("A");

      Assert.AreEqual(graph.Edges.Count(), 1);
    }

    /// <summary>
    /// Ideally, ContainsEdge(vetex1, vertex2) function of an undirected graph should return not care about the order of vertices and should return
    /// true if there is an edge between the two vertices. This is not the case with QuickGraph.UndirectedGraph. This simple test proves the point.
    /// </summary>
    [TestMethod]
    public void UndirectednessTest()
    {
      UndirectedGraph<string, TaggedUndirectedEdge<string, int>> graph = new UndirectedGraph<string, TaggedUndirectedEdge<string, int>>();

      graph.AddVertex("A");
      graph.AddVertex("B");

      var Edge = new TaggedUndirectedEdge<string, int>("A", "B", 10);
      graph.AddEdge(Edge);

      Assert.IsTrue(graph.ContainsEdge("A", "B"));
      Assert.IsTrue(graph.ContainsEdge("B", "A"));
    }

    [TestMethod]
    public void GraphSerializeTest()
    {
      var QAComparer = new VM.DefaultQAComparer();
      var P = CreateRandomProject();

      UndirectedGraph<XMLDoc, OurEdge> graph = new UndirectedGraph<XMLDoc, OurEdge>();
      graph.AddVertexRange(P.AllXMLDocs);

      foreach (var V1 in graph.Vertices)
      {
        foreach (var V2 in graph.Vertices)
        {
          if (V1 != V2 && !graph.ContainsEdge(V1, V2))
          {
            var Edge = new OurEdge(V1, V2, null);

            if (graph.AddEdge(Edge))
            {
              var Task = ViewModelLocator.DocComparer.Compare(V1, V2, QAComparer, true, token);
              
                Edge.Tag = Task;
                System.Diagnostics.Debug.WriteLine($"{V1.Name} ({V1.QAs.Count}) - {V2.Name} ({V2.QAs.Count}): Result = {Task.Items.Count}");
            }
          }
        }
      }
    }
  }
}