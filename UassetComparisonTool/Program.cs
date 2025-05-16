using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UassetComparisonTool;

internal static class Program {

    private static void Main(string[] args) {
        if (args.Length != 2) {
            Console.WriteLine("Usage: UassetComparisonTool <fileOrDirectoryA> <fileOrDirectoryB>");
            return;
        }

        var pathA = args[0];
        var pathB = args[1];

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            CompareDirectories(pathA, pathB);
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            CompareFiles(pathA, pathB);
            return;
        }

        Console.WriteLine("Both arguments must be either existing files or existing directories.");
    }

    private static void CompareFiles(string fileA, string fileB) {
        var assetA = new UAsset(fileA, EngineVersion.VER_UE4_27);
        var assetB = new UAsset(fileB, EngineVersion.VER_UE4_27);
        var assetDiffFinder = new AssetDiffFinder();

        assetDiffFinder.FindDiffs("", assetA, assetB);
    }

    private static void CompareDirectories(string dirA, string dirB) {
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);

        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        foreach (var relPath in allKeys) {
            var assetA = GetUAsset(relPath, filesA);
            var assetB = GetUAsset(relPath, filesB);
            var assetDiffFinder = new AssetDiffFinder();

            assetDiffFinder.FindDiffs(relPath, assetA, assetB);
        }
    }
    
    private static UAsset? GetUAsset(string relPath, Dictionary<string, string> files) {
        return files.TryGetValue(relPath, out var fullPath)
                ? new UAsset(fullPath, EngineVersion.VER_UE4_27)
                : null;
    }

    private static Dictionary<string, string> GetUassetPaths(string directory) {
        return Directory.GetFiles(directory, "*.uasset", SearchOption.AllDirectories)
                .ToDictionary(fullPath => Path.GetRelativePath(directory, fullPath));
    }
}
