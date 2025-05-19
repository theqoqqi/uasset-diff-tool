using Newtonsoft.Json;
using UAssetAPI;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool.Diffs;

public class AssetDiff(DiffType diffType, string name) : Diff(diffType, name) {

    [JsonProperty]
    public ValueChange<string> Path { get; private set; } = ValueChange<string>.Default();

    [JsonProperty]
    public Dictionary<string, FunctionDiff> Functions { get; private set; } = new();

    [JsonProperty]
    public Dictionary<string, PropertyDiff> Properties { get; private set; } = new();

    public IEnumerable<FunctionDiff> ChangedFunctions =>
            Functions.Values.Where(d => d.DiffType != DiffType.Unchanged);

    public IEnumerable<PropertyDiff> ChangedProperties =>
            Properties.Values.Where(d => d.DiffType != DiffType.Unchanged);

    protected override IList<IChangeable> CollectChildren() {
        var children = new List<IChangeable>();

        children.AddRange(Functions.Values);
        children.AddRange(Properties.Values);

        return children;
    }

    public static AssetDiff Create(DiffContext context, string assetName, UAsset? assetA, UAsset? assetB) {
        var diffType = GetInitialDiffType(assetA, assetB);
        var diff = new AssetDiff(diffType, assetName);

        if (CanBeTreatedAsBlueprint(assetA) && CanBeTreatedAsBlueprint(assetB)) {
            diff.Path = ValueChange<string>.Create(context.shortPathA, context.shortPathB);
            diff.Properties = PropertyDiff.Create(context, assetA, assetB);
            diff.Functions = FunctionDiff.Create(context, assetA, assetB);
            diff.ResolveDiffType();
        }

        return diff;
    }

    private static bool CanBeTreatedAsBlueprint(UAsset? asset) {
        return asset is null || HasBlueprints(asset);
    }
}
