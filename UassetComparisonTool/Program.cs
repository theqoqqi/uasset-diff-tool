using System.CommandLine;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UassetComparisonTool.Diffs;

namespace UassetComparisonTool;

internal static class Program {

    private static async Task<int> Main(string[] args) {
        var pathA = new Argument<string>(
                name: "pathA",
                description: "First .uasset file or directory to compare."
        );

        var pathB = new Argument<string>(
                name: "pathB",
                description: "Second .uasset file or directory to compare."
        );

        var outputPath = new Option<string?>(
                aliases: ["--output", "-o"],
                getDefaultValue: () => null,
                description: "If set, write diff to this file; otherwise write to console."
        ).ExistingOrCreateableFile();

        var rootCommand = new RootCommand("UAsset comparison tool") {
                pathA,
                pathB,
                outputPath
        };

        rootCommand.SetHandler(RunComparison, pathA, pathB, outputPath);

        return await rootCommand.InvokeAsync(args);
    }

    private static void RunComparison(string pathA, string pathB, string? outputPath) {
        var writer = GetWriter(outputPath);
        var diffPrinter = new DiffPrinter(writer);

        if (Directory.Exists(pathA) && Directory.Exists(pathB)) {
            CompareDirectories(diffPrinter, pathA, pathB);
            writer.Flush();
            return;
        }

        if (File.Exists(pathA) && File.Exists(pathB)) {
            CompareFiles(diffPrinter, pathA, pathB);
            writer.Flush();
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

    private static void CompareFiles(DiffPrinter diffPrinter, string fileA, string fileB) {
        var assetA = new UAsset(fileA, EngineVersion.VER_UE4_27);
        var assetB = new UAsset(fileB, EngineVersion.VER_UE4_27);
        var context = DiffContext.From(assetA, assetB);
        var nameA = Path.GetFileNameWithoutExtension(fileA);
        var nameB = Path.GetFileNameWithoutExtension(fileB);
        var assetName = nameA == nameB ? nameA : $"{nameA} => {nameB}";
        var assetDiff = AssetDiff.Create(context, assetName, assetA, assetB);

        diffPrinter.PrintDiff(assetDiff);
    }

    private static void CompareDirectories(DiffPrinter diffPrinter, string dirA, string dirB) {
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
