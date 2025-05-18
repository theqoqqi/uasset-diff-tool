using UAssetAPI;

namespace UAssetDiffTool.Diffs;

public record DiffContext(UAsset? AssetA, UAsset? AssetB, string shortPathA, string shortPathB) {

    public static DiffContext From(UAsset? assetA, UAsset? assetB) {
        return new DiffContext(assetA, assetB, "", "");
    }
    
    public static DiffContext From(UAsset? assetA, UAsset? assetB, string shortPathA, string shortPathB) {
        return new DiffContext(assetA, assetB, shortPathA, shortPathB);
    }
}
