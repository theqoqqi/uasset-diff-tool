
using UAssetAPI;
using static UassetComparisonTool.UassetUtils;

namespace UassetComparisonTool.Diffs;

public class AssetDiff(DiffType diffType, string name) : Diff(diffType, name) {

    public Dictionary<string, FunctionDiff> Functions { get; private init; } = new();

    public Dictionary<string, PropertyDiff> Properties { get; private init; } = new();

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
        if (assetA is null) {
            return new AssetDiff(DiffType.Added, assetName);
        }

        if (assetB is null) {
            return new AssetDiff(DiffType.Removed, assetName);
        }

        if (!HasBlueprints(assetA) || !HasBlueprints(assetB)) {
            return new AssetDiff(DiffType.Unchanged, assetName);
        }

        var diff = new AssetDiff(DiffType.Unchanged, assetName) {
                Properties = PropertyDiff.Create(context, assetA, assetB),
                Functions = FunctionDiff.Create(context, assetA, assetB)
        };
        
        diff.ResolveDiffType();        
        
        return diff;
    }
}
