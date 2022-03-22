using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prinubes.Identity.Datamodels.Platform
{

    public class CustomJSONDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                return DateTime.SpecifyKind(
                    DateTime.Parse(jsonDoc.RootElement.GetRawText().Trim('"').Trim('\'')),
                    DateTimeKind.Utc
                );
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
