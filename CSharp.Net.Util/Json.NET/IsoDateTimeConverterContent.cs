using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util.NewtJson
{
    public class IsoDateTimeConverterContent : IsoDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                DateTime dateTime = (DateTime)value;
                if (dateTime == default(DateTime)
                    || dateTime == DateTime.MinValue
                    || dateTime.ToString("yyyy-MM-dd") == "1970-01-01"
                    || dateTime.ToString("yyyy-MM-dd") == "1900-01-01")
                {
                    //writer.WriteValue("");
                    return;
                }
            }
            base.WriteJson(writer, value, serializer);
        }
    }
}