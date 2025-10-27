using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public sealed class FlexibleDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number) return reader.GetDouble();
            if (reader.TokenType == JsonTokenType.String &&
                double.TryParse(reader.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            throw new JsonException("Expected number for double.");
        }
        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value);
    }
}
