using QuickGraph;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace MultiDF.VM
{
  /// <summary>
  /// This class represents an edge between two vertices of a graph. The built-in TaggedEdge class that comes with
  /// QuickGraph doesn't allow setting Source and Target properties directly. You can only send them in the constructor, therefore
  /// The class doesn't have any parameter-less constructor. Thus we cannot use it in serialization. Therefore we are making our 
  /// own version of the edge class that allows setting Source and Target properties directly and also has a parameter-less constructor.
  /// </summary>
  /// <typeparam name="TVertex"></typeparam>
  /// <typeparam name="TTag"></typeparam>
  [DebuggerDisplay("{Source}->{Target}")]
  public class OurEdge : IEdge<XMLDoc>, ITagged<DFResult>
  {
    /// <summary>
    /// Only for serialization. Do not use this overload in the code.
    /// </summary>
    public OurEdge()
    {

    }

    public OurEdge(XMLDoc source, XMLDoc target, DFResult tag)
    {
      Source = source;
      Target = target;
      Tag = tag;
    }

    public XMLDoc Source { get; set; }
    public XMLDoc Target { get; set; }
    public DFResult Tag { get; set; }

    public event EventHandler TagChanged;

    protected virtual void OnTagChanged(EventArgs e)
    {
      TagChanged?.Invoke(this, e);
    }

    public override string ToString()
    {
      return Source.ToString() + "->" + Target.ToString() + "[" + Tag.ToString() + "]";
    }
  }

  /// <summary>
  /// For custom serialization, we are inheriting the QuickGraph class
  /// </summary>
  [Serializable]
  [KnownType(typeof(XMLDoc[]))]
  [KnownType(typeof(DFResult[]))]
  public class OurGraph : UndirectedGraph<XMLDoc, OurEdge>, ISerializable
  {
    private double _DiffThreshold = .2;
    public double DiffThreshold
    {
      get => _DiffThreshold;
      set
      {
        _DiffThreshold = value;
      }
    }

    public OurGraph()
    {

    }

    protected OurGraph(SerializationInfo info, StreamingContext context)
    {
      // Perform your deserialization here...
      this.DiffThreshold = (double)info.GetValue("DiffThreshold", typeof(double));
      this.Docs = (XMLDoc[])info.GetValue("Docs", typeof(XMLDoc[]));
      this.Results = (DFResult[])info.GetValue("Results", typeof(DFResult[]));
    }

    public XMLDoc[] Docs
    {
      get => base.Vertices.ToArray();
      set => this.AddVertexRange(value);
    }

    public DFResult[] Results
    {
      get => this.Edges.Select(e => e.Tag).ToArray();
      set
      {
        foreach (var Res in value)
        {
          this.AddEdge(new OurEdge(Res.Doc1, Res.Doc2, Res));
        }
      }
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        return;

      info.AddValue("DiffThreshold", DiffThreshold);
      info.AddValue("Docs", Docs);
      info.AddValue("Results", Results);
    }
  }
}