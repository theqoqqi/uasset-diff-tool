using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAssetDiffTool.Diffs.Json;

public class JsonDiffWriter(string path, Formatting formatting = Formatting.None) {

    public readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            ContractResolver = new DiffContractResolver(),
            Converters = {
                    new ChangeableDictionaryConverter(),
                    new StringEnumConverter(),
            },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = formatting,
    };

    public void WriteJson(Diff diff) {
        File.WriteAllText(path, Serialize(diff));
    }

    public void WriteJson<T>(Dictionary<string, T> diffs) where T : Diff {
        File.WriteAllText(path, Serialize(diffs));
    }

    public string Serialize(Diff diff) {
        return JsonConvert.SerializeObject(diff, Settings);
    }

    public string Serialize<T>(Dictionary<string, T> diffs) where T : Diff {
        return JsonConvert.SerializeObject(diffs, Settings);
    }
}
