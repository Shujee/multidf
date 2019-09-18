using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DuplicateFinderMulti.VM
{
  /// <summary>
  /// This static class adds extension method to 
  /// </summary>
  public static class StaticExtensions
  {
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