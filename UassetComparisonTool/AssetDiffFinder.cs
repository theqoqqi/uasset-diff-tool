using UAssetAPI;
using static UassetComparisonTool.UassetUtils;

namespace UassetComparisonTool;

public class AssetDiffFinder {
    
    public void FindDiffs(string assetName, UAsset? assetA, UAsset? assetB) {
        if (assetA is null) {
            Console.WriteLine($"Has been added: {assetName}");
            return;
        }

        if (assetB is null) {
            Console.WriteLine($"Has been removed: {assetName}");
            return;
        }

        if (!HasBlueprints(assetA) || !HasBlueprints(assetB)) {
            return;
        }
        
        var diffFinder = new BlueprintDiffFinder();
        
        Console.WriteLine($"\n=== Asset: {assetName} ===");
        diffFinder.FindDiff(assetA, assetB);
    }
}
