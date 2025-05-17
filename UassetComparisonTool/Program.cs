using System.CommandLine;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UassetComparisonTool.Diffs;
using static UassetComparisonTool.UassetUtils;

namespace UassetComparisonTool;

internal static class Program {

    private static readonly Argument<string> PathA = new Argument<string>(
            name: "pathA",
            description: "First .uasset file or directory to compare."
    );

    private static readonly Argument<string> PathB = new Argument<string>(
            name: "pathB",
            description: "Second .uasset file or directory to compare."
    );

    private static readonly Option<string?> OutputPath = new Option<string?>(
            aliases: ["--output", "-o"],
            getDefaultValue: () => null,
            description: "If set, write diff to this file; otherwise write to console."
    ).ExistingOrCreateableFile();

    private static readonly Option<FileInfo?> FilterByDeps = new Option<FileInfo?>(
            aliases: ["--filter-by-deps", "-D"],
            description: "Only show diffs for assets listed in this dependency file."
    ).LegalFilePathsOnly();

    private static readonly Option<DiffType[]> DiffTypes = new Option<DiffType[]>(
            aliases: ["--diff-types", "-d"],
            getDefaultValue: () => [
                    DiffType.Added,
                    DiffType.Removed,
                    DiffType.Changed
            ],
            description: "Which diff types to include (comma or space separated)."
    ) {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
    };

    private static readonly Option<bool> BlueprintsOnly = new Option<bool>(
            name: "--blueprints-only",
            description: "Only include assets that are Blueprint classes (i.e. have functions or properties)."
    );

    private static readonly RootCommand Command = new RootCommand("UAsset comparison tool") {
            PathA,
            PathB,
            OutputPath,
            FilterByDeps,
            DiffTypes,
            BlueprintsOnly
    };

    private static async Task<int> Main(string[] args) {
        Command.SetHandler(
                RunComparison,
                PathA,
                PathB,
                OutputPath,
                FilterByDeps,
                DiffTypes,
                BlueprintsOnly
        );

        return await Command.InvokeAsync(args);
    }

    private static void RunComparison(
            string pathA,
            string pathB,
            string? outputPath,
            FileInfo? filterByDeps,
            DiffType[] diffTypes,
            bool blueprintsOnly
    ) {
        var writer = GetWriter(outputPath);
        var diffPrinter = new DiffPrinter(writer, diffTypes);

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            var assetDiffs = CompareDirectories(pathA, pathB, blueprintsOnly);
            var filteredAssetDiffs = FilterAssetDiffs(assetDiffs, filterByDeps);

            diffPrinter.PrintDiffs(filteredAssetDiffs.Values);
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            diffPrinter.PrintDiff(CompareFiles(pathA, pathB));
            return;
        }

        Console.WriteLine("Both arguments must be either existing files or existing directories.");
    }

    private static Dictionary<string, AssetDiff> FilterAssetDiffs(
            Dictionary<string, AssetDiff> assetDiffs,
            FileSystemInfo? filterByDeps
    ) {
        if (filterByDeps == null) {
            return assetDiffs;
        }

        var allowedPaths = ParseDependencyFile(filterByDeps.FullName);

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

    private static Dictionary<string, AssetDiff> CompareDirectories(string dirA, string dirB, bool blueprintsOnly) {
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);
        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        return allKeys
                .Select(shortPath => GetAssetDiff(shortPath, filesA, filesB, blueprintsOnly))
                .OfType<AssetDiff>()
                .ToDictionary(diff => diff.Name);
    }

    private static AssetDiff? GetAssetDiff(
            string shortPath,
            Dictionary<string, string> filesA,
            Dictionary<string, string> filesB,
            bool blueprintsOnly
    ) {
        var assetA = GetUAsset(shortPath, filesA);
        var assetB = GetUAsset(shortPath, filesB);
        var context = DiffContext.From(assetA, assetB, shortPath, shortPath);

        if (blueprintsOnly && !(HasBlueprints(assetA) || HasBlueprints(assetB))) {
            return null;
        }

        return AssetDiff.Create(context, shortPath, assetA, assetB);
    }

    private static HashSet<string> ParseDependencyFile(string filePath) {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(filePath)) {
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

internal static class OptionExtensions {
    public static Option<string?> ExistingOrCreateableFile(this Option<string?> opt) {
        opt.AddValidator(result => {
            var value = result.GetValueOrDefault<string?>();

            if (string.IsNullOrEmpty(value)) {
                return;
            }

            try {
                var dir = Path.GetDirectoryName(value);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
            }
            catch {
                result.ErrorMessage = $"Invalid path: {value}";
            }
        });

        return opt;
    }
}
