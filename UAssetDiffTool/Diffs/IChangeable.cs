using Newtonsoft.Json;

namespace UAssetDiffTool.Diffs;

[JsonObject(MemberSerialization.OptIn)]
public interface IChangeable {

    [JsonProperty]
    DiffType DiffType { get; }
}
