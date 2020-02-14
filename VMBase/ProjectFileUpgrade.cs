using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace VMBase
{
  /// <summary>
  /// Upgrades the structure of Project XML files generated with previous versions of MultiDF to make them compatible with the current version.
  /// </summary>
  public class ProjectFileUpgrade
  {
    /// <summary>
    /// Upgrades the structure of supplied XML to make it compatible with the current version of MultiDF.
    /// </summary>
    /// <param name="xml"></param>
    public static string Upgrade(string xml)
    {
      bool Changed = false;

      //replace old namespace with current one
      if (xml.Contains("http://schemas.datacontract.org/2004/07/DuplicateFinderMulti.VM"))
      {
        xml = xml.Replace("http://schemas.datacontract.org/2004/07/DuplicateFinderMulti.VM", "http://schemas.datacontract.org/2004/07/MultiDF.VM");
        Changed = true;
      }

      var X = ReadXDocumentWithInvalidCharacters(new MemoryStream(Encoding.Unicode.GetBytes(xml)));

      var VersionNode = X.Root.Element("Version");
      string Version = null;

      if (VersionNode == null)
      {
        XNamespace ns_MultiDF = "http://schemas.datacontract.org/2004/07/MultiDF.VM";
        XNamespace ns_VMBase = "http://schemas.datacontract.org/2004/07/VMBase";
        XNamespace ns_i = "http://www.w3.org/2001/XMLSchema-instance";

        var att_vmbase = new XAttribute(XNamespace.Xmlns + "vmbase", ns_VMBase);

        var AllXMLDocs = X.Root.Element(ns_MultiDF + "AllXMLDocs");

        if (AllXMLDocs != null)
        {
          if (AllXMLDocs.Attribute(XNamespace.Xmlns + "vmbase") == null)
            X.Root.Element(ns_MultiDF + "AllXMLDocs").Add(att_vmbase);

          foreach (var node in AllXMLDocs.Descendants())
          {
            if (node.NodeType == XmlNodeType.Element && node.Name.LocalName != "string")
              node.Name = ns_VMBase + node.Name.LocalName;
          }
        }

        var GraphDocs = X.Root.Element(ns_MultiDF + "Graph").Element("Docs");

        if (GraphDocs.Attribute(XNamespace.Xmlns + "d2p1") != null)
          GraphDocs.Attribute(XNamespace.Xmlns + "d2p1").Remove();

        GraphDocs.Add(att_vmbase);

        if (GraphDocs.Attribute(ns_i + "type") != null)
          GraphDocs.Attribute(ns_i + "type").Remove();

        GraphDocs.Add(new XAttribute(ns_i + "type", "vmbase:ArrayOfXMLDoc"));

        foreach (var doc in GraphDocs.Elements())
        {
          doc.Name = ns_VMBase + doc.Name.LocalName;
        }

        Version = "1.4.0.0";
        Changed = true;
      }
      else
        Version = VersionNode.Value;

      var Ver = new System.Version(Version);
      
      if (Ver < new System.Version("1.4.0.0"))
      {
        //Make version-specific transformations
        Changed = true;
      }

      if (Changed)
      {
        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { CheckCharacters = false };

        StringBuilder xml2 = new StringBuilder();
        using (XmlWriter xmlWriter = XmlWriter.Create(xml2, xmlWriterSettings))
        {
          X.WriteTo(xmlWriter);
        }

        return xml2.ToString();
      }
      else
        return xml;
    }

    private static XDocument ReadXDocumentWithInvalidCharacters(Stream xml)
    {
      XDocument xDocument = null;

      XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };

      using (XmlReader xmlReader = XmlReader.Create(xml, xmlReaderSettings))
      {
        // Load our XDocument
        xmlReader.MoveToContent();
        xDocument = XDocument.Load(xmlReader);
      }

      return xDocument;
    }
  }
}
