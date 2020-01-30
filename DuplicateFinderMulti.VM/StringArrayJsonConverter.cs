using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderMulti.VM
{
  internal class StringArrayJsonConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(List<string>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      JToken token = JToken.Load(reader);

      if (token.Type == JTokenType.String)
        return  ((string)existingValue).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

      return token.ToObject(objectType);
    }

    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var lst = value as List<string>;
      writer.WriteValue(lst != null && lst.Count > 0 ? string.Join(Environment.NewLine, lst) : "");;
    }
  }
}