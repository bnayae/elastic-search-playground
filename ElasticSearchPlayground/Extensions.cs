using System;
using System.Collections.Generic;
using System.IO;
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
        public static string GetString(this JsonDocument jdoc, string key)
        {
            return jdoc.RootElement.GetProperty(key).GetString();
        }
        public static bool GetBool(this JsonDocument jdoc, string key)
        {
            return jdoc.RootElement.GetProperty(key).GetBoolean();
        }
        public static string GetSourceString(this JsonDocument jdoc, string key)
        {
            return jdoc.RootElement
                            .GetProperty("_source")
                            .GetProperty(key)
                            .GetString();
        }
        public static HttpContent AsJsonContent(this string data)
        {
            return new StringContent(data, Encoding.UTF8, "application/json");
        }
    }
}
