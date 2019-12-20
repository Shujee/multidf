using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using QuickGraph;
using System.Collections.Generic;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// This static class adds extension method to 
  /// </summary>
  public static class StaticExtensions
  {
    /// <summary>
    /// This setting is required to prevent exceptions that are thrown by the serialization when it encounters control characters used by
    /// Microsoft Word to denote newlines and non-breaking newlines.
    /// </summary>
    private static readonly XmlWriterSettings xmlWriterSettingsForWordDocs = new XmlWriterSettings()
    {
      NewLineChars = "\a\r\n",
      CheckCharacters = false,
      Encoding = System.Text.Encoding.UTF8,
      NewLineHandling = NewLineHandling.Entitize,
    };

    private static readonly Type[] AllObjectTypes = {
      typeof(QA),
      typeof(XMLDoc),
      typeof(DFResultRow),
      typeof(DFResult),
    };

    private static readonly DataContractSerializer DSSerializer = new DataContractSerializer(typeof(Project), AllObjectTypes, 0x7F_FFFF, false, true, null); //max graph size is 8,388,607‬ items

    /// <summary>
    /// Exports the contents of this list to XML.
    /// </summary>
    /// <param name="selectedOnly">If True, only selected objects are exported, otherwise the entire list is exported.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string SerializeDC(this object obj)
    {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      StringWriter writer = new StringWriter(sb);
      using (XmlWriter xmlWriter = XmlWriter.Create(writer, xmlWriterSettingsForWordDocs))
      {
        DSSerializer.WriteStartObject(xmlWriter, obj);
        DSSerializer.WriteObjectContent(xmlWriter, obj);
        DSSerializer.WriteEndObject(xmlWriter);
      }

      return sb.ToString();
    }

    public static T DeserializeDC<T>(this string xml)
    {
      try
      {
        StringReader s = new StringReader(xml);

        using (var reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
        {
          return (T)DSSerializer.ReadObject(reader, true);
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
        using (var writer = XmlWriter.Create(stringWriter, xmlWriterSettingsForWordDocs))
        {
          xmlserializer.Serialize(writer, value);
          return stringWriter.ToString();
        }
      }
    }

    public static T Deserialize<T>(this string xml)
    {
      StringReader s = new StringReader(xml);

      using (XmlReader reader = XmlReader.Create(s, new XmlReaderSettings() { CheckCharacters = false }))
      {
        return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
      }
    }

    /// <summary>
    /// Returns an available temporary file path with the specified extension
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string GetTempFileName(string extension)
    {
      var TempFile = Path.GetTempFileName();
      var Orig = TempFile;
      TempFile = Path.ChangeExtension(TempFile, extension);

      while (File.Exists(TempFile))
        TempFile = Path.ChangeExtension(Path.GetTempFileName(), extension);

      File.Delete(Orig);
      return TempFile;
    }
  }
}