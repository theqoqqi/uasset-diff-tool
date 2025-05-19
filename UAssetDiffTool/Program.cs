using System.CommandLine;
using UAssetAPI;
using UAssetDiffTool.Cli;
using UAssetDiffTool.Diffs;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool;

internal static class Program {

    private static readonly UAssetDiffCommand Command = new UAssetDiffCommand();

    private static async Task<int> Main(string[] args) {
        Command.SetHandler(Run);

        return await Command.InvokeAsync(args);
    }

    private static void Run(UAssetDiffCommandContext context) {
        var assetsA = context.AssetsA;
        var assetsB = context.AssetsB;
        var diffPrinter = context.DiffPrinter;
        var renamedFiles = context.RenamedFiles;
        var filterByDeps = context.FilterByDeps;
        var blueprintsOnly = context.BlueprintsOnly;

        if (context.IsSingleFileDiff) {
            var assetDiff = CompareSingleAsset(context.AssetA!, context.AssetB!);
            
            diffPrinter.PrintDiff(assetDiff);
            return;
        }
        
        var assetDiffs = CompareAssets(assetsA!, assetsB!, renamedFiles, blueprintsOnly);
        var filteredAssetDiffs = FilterAssetDiffs(assetDiffs, filterByDeps);

        diffPrinter.PrintDiffs(filteredAssetDiffs.Values);
    }

    private static Dictionary<string, AssetDiff> FilterAssetDiffs(
            Dictionary<string, AssetDiff> assetDiffs,
            IReadOnlySet<string>? allowedPaths
    ) {
        if (allowedPaths is null) {
            return assetDiffs;
        }

        return assetDiffs
                .Where(pair => allowedPaths.Contains(pair.Key))
                .ToDictionary();
    }

    private static AssetDiff CompareSingleAsset(UAsset assetA, UAsset assetB) {
        var context = DiffContext.From(assetA, assetB);
        var nameA = Path.GetFileNameWithoutExtension(assetA.FilePath);
        var nameB = Path.GetFileNameWithoutExtension(assetB.FilePath);
        var assetName = nameA == nameB ? nameA : $"{nameA} => {nameB}";

        return AssetDiff.Create(context, assetName, assetA, assetB);
    }

    private static Dictionary<string, AssetDiff> CompareAssets(
            Dictionary<string, UAsset> assetsA,
            Dictionary<string, UAsset> assetsB,
            Dictionary<string, string> renameMap,
            bool blueprintsOnly
    ) {
        return assetsA.Keys.Union(assetsB.Keys)
                .OrderBy(k => k)
                .Select(shortPath => GetAssetDiff(shortPath, assetsA, assetsB, renameMap, blueprintsOnly))
                .OfType<AssetDiff>()
                .ToDictionary(diff => diff.Name);
    }

    private static AssetDiff? GetAssetDiff(
            string shortPath,
            Dictionary<string, UAsset> assetsA,
            Dictionary<string, UAsset> assetsB,
            Dictionary<string, string> renameMap,
            bool blueprintsOnly
    ) {
        if (renameMap.ContainsValue(shortPath)) {
            return null;
        }

        var shortPathA = shortPath;
        var shortPathB = renameMap.GetValueOrDefault(shortPath, shortPath);
        var assetA = assetsA.GetValueOrDefault(shortPathA);
        var assetB = assetsB.GetValueOrDefault(shortPathB);
        var context = DiffContext.From(assetA, assetB, shortPathA, shortPathB);

        if (blueprintsOnly && !(HasBlueprints(assetA) || HasBlueprints(assetB))) {
            return null;
        }

        return AssetDiff.Create(context, shortPath, assetA, assetB);
    }
}
