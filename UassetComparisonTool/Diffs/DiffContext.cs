using UAssetAPI;

namespace UassetComparisonTool.Diffs;

public record DiffContext(UAsset? AssetA, UAsset? AssetB) {

    public static DiffContext From(UAsset? assetA, UAsset? assetB) {
        return new DiffContext(assetA, assetB);
    }
}
