using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Statiq.Core
{
    /// <summary>
    /// A JSON converter that will convert arbitrary JSON values to an object while guessing
    /// the appropriate type.
    /// </summary>
    /// <remarks>
    /// See https://github.com/dotnet/runtime/issues/29960 for a discussion of why this is needed.
    /// In summary, the System.Text.Json serializer will not attempt to "guess" at the type of
    /// a value when deserializing to a <see cref="object"/>. This is a combination of the
    /// <c>ObjectToPrimitiveConverter</c> at https://github.com/dotnet/runtime/issues/29960#issuecomment-535166692
    /// and the <c>SystemObjectNewtonsoftCompatibleConverter</c> at
    /// https://github.com/dotnet/runtime/blob/7eea339df0dab9feb1a9b7bf6be66ddcb9924dc9/src/libraries/System.Text.Json/tests/Serialization/CustomConverterTests.Object.cs#L267.
    /// </remarks>
    public class ObjectToPrimitiveJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int i))
                {
                    return i;
                }

                if (reader.TryGetInt64(out long l))
                {
                    return l;
                }

                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTime(out DateTime datetime))
                {
                    // If an offset was provided, use DateTimeOffset
                    if (datetime.Kind == DateTimeKind.Local
                        && reader.TryGetDateTimeOffset(out DateTimeOffset datetimeOffset))
                    {
                        return datetimeOffset;
                    }

                    return datetime;
                }

                return reader.GetString();
            }

            // Use JsonElement as fallback (Newtonsoft uses JArray or JObject)
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.Clone();
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Should not get here");
        }
    }
}
