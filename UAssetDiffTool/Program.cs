using System.CommandLine;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetDiffTool.Cli;
using UAssetDiffTool.Diffs;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool;

internal static class Program {

    private static readonly UAssetDiffCommand Command = new UAssetDiffCommand();

    private static async Task<int> Main(string[] args) {
        Command.SetHandler(RunComparison);

        return await Command.InvokeAsync(args);
    }

    private static void RunComparison(UAssetDiffCommand.ParsedSymbols context) {
        var pathA = context.PathA;
        var pathB = context.PathB;
        var outputPath = context.OutputPath;
        var renamedFiles = context.RenamedFiles;
        var filterByDeps = context.FilterByDeps;
        var diffTypes = context.DiffTypes;
        var blueprintsOnly = context.BlueprintsOnly;

        var renameMap = ParseRenamedFiles(renamedFiles);
        var writer = GetWriter(outputPath);
        var diffPrinter = new DiffPrinter(writer, diffTypes);

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            var assetDiffs = CompareDirectories(pathA, pathB, renameMap, blueprintsOnly);
            var allowedPaths = ParseDependencyFile(filterByDeps);
            var filteredAssetDiffs = FilterAssetDiffs(assetDiffs, allowedPaths);

            diffPrinter.PrintDiffs(filteredAssetDiffs.Values);
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            diffPrinter.PrintDiff(CompareFiles(pathA, pathB));
            return;
        }

        Console.WriteLine("Both arguments must be either existing files or existing directories.");
    }

    private static Dictionary<string, string> ParseRenamedFiles(FileInfo? renamedFiles) {
        if (renamedFiles is null) {
            return new Dictionary<string, string>();
        }

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(renamedFiles.FullName)) {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2) {
                var oldKey = GetShortAssetPath(parts[0]);
                var newKey = GetShortAssetPath(parts[1]);

                map[oldKey] = newKey;
            }
        }

        return map;
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

    private static TextWriter GetWriter(string? outputPath) {
        if (outputPath is null) {
            return Console.Out;
        }

        return new StreamWriter(outputPath) {
                AutoFlush = false
        };
    }

    private static AssetDiff CompareFiles(string fileA, string fileB) {
        var assetA = new UAsset(fileA, EngineVersion.VER_UE4_27);
        var assetB = new UAsset(fileB, EngineVersion.VER_UE4_27);
        var context = DiffContext.From(assetA, assetB);
        var nameA = Path.GetFileNameWithoutExtension(fileA);
        var nameB = Path.GetFileNameWithoutExtension(fileB);
        var assetName = nameA == nameB ? nameA : $"{nameA} => {nameB}";

        return AssetDiff.Create(context, assetName, assetA, assetB);
    }

    private static Dictionary<string, AssetDiff> CompareDirectories(
            string dirA,
            string dirB,
            Dictionary<string, string> renameMap,
            bool blueprintsOnly
    ) {
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);
        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        return allKeys
                .Select(shortPath => GetAssetDiff(shortPath, filesA, filesB, renameMap, blueprintsOnly))
                .OfType<AssetDiff>()
                .ToDictionary(diff => diff.Name);
    }

    private static AssetDiff? GetAssetDiff(
            string shortPath,
            Dictionary<string, string> filesA,
            Dictionary<string, string> filesB,
            Dictionary<string, string> renameMap,
            bool blueprintsOnly
    ) {
        if (renameMap.ContainsValue(shortPath)) {
            return null;
        }

        var shortPathA = shortPath;
        var shortPathB = renameMap.GetValueOrDefault(shortPath, shortPath);
        var assetA = GetUAsset(shortPathA, filesA);
        var assetB = GetUAsset(shortPathB, filesB);
        var context = DiffContext.From(assetA, assetB, shortPathA, shortPathB);

        if (blueprintsOnly && !(HasBlueprints(assetA) || HasBlueprints(assetB))) {
            return null;
        }

        return AssetDiff.Create(context, shortPath, assetA, assetB);
    }

    private static HashSet<string>? ParseDependencyFile(FileSystemInfo? filterByDeps) {
        if (filterByDeps is null) {
            return null;
        }
        
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(filterByDeps.FullName)) {
            var trimmed = line.Trim().Replace('/', '\\');

            if (trimmed.StartsWith("\\Game\\") && !trimmed.StartsWith("\\Game\\Mods\\")) {
                var withoutPrefix = trimmed.Substring("\\Game\\".Length);
                var withoutExtension = withoutPrefix.Split('.')[0];

                result.Add(withoutExtension);
            }
        }

        return result;
    }

    private static UAsset? GetUAsset(string shortPath, Dictionary<string, string> files) {
        return files.TryGetValue(shortPath, out var fullPath)
                ? new UAsset(fullPath, EngineVersion.VER_UE4_27)
                : null;
    }

    private static Dictionary<string, string> GetUassetPaths(string directory) {
        return Directory.GetFiles(directory, "*.uasset", SearchOption.AllDirectories)
                .ToDictionary(fullPath => GetShortAssetPath(directory, fullPath));
    }

    private static string GetShortAssetPath(string directory, string path) {
        return GetShortAssetPath(Path.GetRelativePath(directory, path));
    }

    private static string GetShortAssetPath(string relativePath) {
        var convertedPath = relativePath.Replace('/', '\\');
        var idx = convertedPath.IndexOf("Content\\", StringComparison.Ordinal);

        if (idx < 0) {
            return convertedPath;
        }

        var shortPath = convertedPath.Substring(idx + "Content\\".Length);

        return Path.ChangeExtension(shortPath, null);
    }
}
