using System.Collections;
using Newtonsoft.Json;

namespace UAssetDiffTool.Diffs.Json;

public class ChangeableDictionaryConverter : JsonConverter {

    public override bool CanRead => false;

    public override bool CanConvert(Type objectType) {
        if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            return false;

        var keyType = objectType.GetGenericArguments()[0];
        var valueType = objectType.GetGenericArguments()[1];

        return keyType == typeof(string) && typeof(IChangeable).IsAssignableFrom(valueType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        if (value is null) {
            return;
        }

        writer.WriteStartObject();

        var enumerable = (IEnumerable) value;

        foreach (var item in enumerable) {
            var keyProp = item.GetType().GetProperty("Key");
            var valueProp = item.GetType().GetProperty("Value");

            var key = (string?) keyProp?.GetValue(item);
            var val = valueProp?.GetValue(item);
            var isChanged = val is not IChangeable {
                    DiffType: DiffType.Unchanged
            };

            if (key is not null && isChanged) {
                writer.WritePropertyName(key);
                serializer.Serialize(writer, val);
            }
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
    ) {
        throw new NotImplementedException("Deserialization is not supported.");
    }
}
