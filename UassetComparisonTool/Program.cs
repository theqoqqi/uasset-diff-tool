using System.CommandLine;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UassetComparisonTool.Diffs;

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

    private static readonly RootCommand Command = new RootCommand("UAsset comparison tool") {
            PathA,
            PathB,
            OutputPath,
            DiffTypes
    };

    private static async Task<int> Main(string[] args) {
        Command.SetHandler(RunComparison, PathA, PathB, OutputPath, DiffTypes);

        return await Command.InvokeAsync(args);
    }

    private static void RunComparison(string pathA, string pathB, string? outputPath, DiffType[] diffTypes) {
        var writer = GetWriter(outputPath);
        var diffPrinter = new DiffPrinter(writer, diffTypes);

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            var assetDiffs = CompareDirectories(pathA, pathB);

            diffPrinter.PrintDiffs(assetDiffs.Values);
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            diffPrinter.PrintDiff(CompareFiles(pathA, pathB));
            return;
        }

        Console.WriteLine("Both arguments must be either existing files or existing directories.");
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

    private static Dictionary<string, AssetDiff> CompareDirectories(string dirA, string dirB) {
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);
        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        return allKeys
                .Select(shortPath => GetAssetDiff(shortPath, filesA, filesB))
                .ToDictionary(diff => diff.Name);
    }

    private static AssetDiff GetAssetDiff(
            string shortPath,
            Dictionary<string, string> filesA,
            Dictionary<string, string> filesB
    ) {
        var assetA = GetUAsset(shortPath, filesA);
        var assetB = GetUAsset(shortPath, filesB);
        var context = DiffContext.From(assetA, assetB);

        return AssetDiff.Create(context, shortPath, assetA, assetB);
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
        var shortPath = Path.GetRelativePath(directory, path);
        var idx = shortPath.IndexOf("Content" + Path.DirectorySeparatorChar, StringComparison.Ordinal);

        if (idx < 0) {
            return shortPath;
        }

        var relativePath = shortPath.Substring(idx + "Content/".Length);

        return Path.ChangeExtension(relativePath, null).Replace('\\', '/');
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
