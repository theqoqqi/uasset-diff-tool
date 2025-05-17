using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UassetComparisonTool.Diffs;

namespace UassetComparisonTool;

internal static class Program {

    private static void Main(string[] args) {
        if (args.Length != 2) {
            Console.WriteLine("Usage: UassetComparisonTool <fileOrDirectoryA> <fileOrDirectoryB>");
            return;
        }

        var pathA = args[0];
        var pathB = args[1];
        var writer = Console.Out;

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            CompareDirectories(writer, pathA, pathB);
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            CompareFiles(writer, pathA, pathB);
            return;
        }

        Console.WriteLine("Both arguments must be either existing files or existing directories.");
    }

    private static void CompareFiles(TextWriter writer, string fileA, string fileB) {
        var diffPrinter = new DiffPrinter(writer);
        var assetA = new UAsset(fileA, EngineVersion.VER_UE4_27);
        var assetB = new UAsset(fileB, EngineVersion.VER_UE4_27);
        var context = DiffContext.From(assetA, assetB);
        var nameA = Path.GetFileNameWithoutExtension(fileA);
        var nameB = Path.GetFileNameWithoutExtension(fileB);
        var assetName = nameA == nameB ? nameA : $"{nameA} => {nameB}";
        var assetDiff = AssetDiff.Create(context, assetName, assetA, assetB);

        diffPrinter.PrintDiff(assetDiff);
    }

    private static void CompareDirectories(TextWriter writer, string dirA, string dirB) {
        var diffPrinter = new DiffPrinter(writer);
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);

        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        foreach (var relPath in allKeys) {
            var assetA = GetUAsset(relPath, filesA);
            var assetB = GetUAsset(relPath, filesB);
            var context = DiffContext.From(assetA, assetB);
            var assetDiff = AssetDiff.Create(context, relPath, assetA, assetB);

            diffPrinter.PrintDiff(assetDiff);
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
