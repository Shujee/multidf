using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// This static class adds extension method to 
  /// </summary>
  public static class StaticExtensions
  {
    private static Type[] AllObjectTypes = {
      typeof(XMLDoc),
      typeof(QA),
    };

    private static DataContractSerializer DSSerializer = new DataContractSerializer(typeof(Project), AllObjectTypes, 0x7fff, false, true, null);

    /// <summary>
    /// Exports the contents of this list to XML.
    /// </summary>
    /// <param name="selectedOnly">If True, only selected objects are exported, otherwise the entire list is exported.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string SerializeDC(this object drawing)
    {
      XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces();
      Namespaces.Add(string.Empty, string.Empty);

      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      using (StringWriter writer = new StringWriter(sb))
      {
        using (XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
        {
          DSSerializer.WriteObject(xmlWriter, drawing);
          xmlWriter.Close();
        }

        writer.Close();
      }

      return sb.ToString();
    }

    public static T DeserializeDC<T>(this string xml)
    {
      try
      {
        var X = XElement.Parse(xml);

        using (var XReader = X.CreateReader())
        {
          return (T)DSSerializer.ReadObject(XReader);
        }
      }
      catch
      {
        return default;
      }
    }

    public static string Serialize(this object value)
    {
      if (value == null)
        return string.Empty;
      else
      {
        var xmlserializer = new XmlSerializer(value.GetType());
        var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { NewLineChars = "\a\r\n", CheckCharacters = false, Encoding = System.Text.Encoding.UTF8, NewLineHandling = NewLineHandling.Entitize }))
        {
          xmlserializer.Serialize(writer, value);
          return stringWriter.ToString();
        }
      }
    }

    public static T Deserialize<T>(this string xml)
    {
      using (StringReader s = new StringReader(xml))
      {
        using (XmlReader reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        { 
          return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
        }
      }
    }
  }
}