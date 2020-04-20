using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ElasticSearchPlayground
{
    internal static class Extensions
    {
        public static string ToJsonString(this JsonDocument jdoc)
        {
            using (var stream = new MemoryStream())
            {
                Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                jdoc.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static JsonElement GetElement(this JsonDocument jdoc, params string[] keys)
        {
            JsonElement jElement = jdoc.RootElement.GetProperty(keys[0]);
            return jElement.GetElement(keys.Skip(1).ToArray());
        }

        public static JsonElement GetElement(this JsonElement jElement, params string[] keys)
        {
            foreach (var key in keys)
            {
                jElement = jElement.GetProperty(key);
            }
            return jElement;
        }

        public static T Get<T>(this JsonDocument jdoc, params string[] keys)
        {
            JsonElement jElement = jdoc.RootElement.GetProperty(keys[0]);
            return jElement.Get<T>(keys.Skip(1));
        }

        public static T Get<T>(this JsonElement jElement, params string[] keys) => jElement.Get<T>((IEnumerable<string>)keys);
        
        public static T Get<T>(this JsonElement jElement, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                jElement = jElement.GetProperty(key);
            }

            dynamic result = typeof(T).Name switch
            {
                nameof(String) => jElement.GetString(),
                nameof(Boolean) => jElement.GetBoolean(),
                nameof(Int16) => jElement.GetInt16(),
                nameof(Int32) => jElement.GetInt32(),
                nameof(Int64) => jElement.GetInt64(),
                nameof(Double) => jElement.GetDouble(),
                nameof(Decimal) => jElement.GetDecimal(),
                nameof(Byte) => jElement.GetByte(),
                nameof(Guid) => jElement.GetGuid(),
                nameof(Single) => jElement.GetSingle(),
                nameof(DateTime) => jElement.GetDateTime(),
                nameof(DateTimeOffset) => jElement.GetDateTimeOffset(),
                _ => throw new NotImplementedException()
            };

            return (T)result;
        }

        public static string GetString(this JsonDocument jdoc, params string[] keys) => jdoc.Get<string>(keys);

        public static bool GetBool(this JsonDocument jdoc, params string[] keys) => jdoc.Get<bool>(keys);
        public static double GetDouble(this JsonDocument jdoc, params string[] keys) => jdoc.Get<double>(keys);

        public static string GetSourceString(this JsonDocument jdoc, params string[] keys) 
        {
            return jdoc.RootElement
                            .GetProperty("_source")
                            .Get<string>(keys);
        }
        public static HttpContent AsJsonContent(this string data)
        {
            return new StringContent(data, Encoding.UTF8, "application/json");
        }
    }
}
