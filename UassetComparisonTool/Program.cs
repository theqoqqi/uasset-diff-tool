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
        var context = DiffContext.From(assetA, assetB);
        var nameA = Path.GetFileNameWithoutExtension(fileA);
        var nameB = Path.GetFileNameWithoutExtension(fileB);
        var assetName = nameA == nameB ? nameA : $"{nameA} => {nameB}";
        var assetDiff = AssetDiff.Create(context, assetName, assetA, assetB);

        PrintAssetDiff(assetDiff);
    }

    private static void CompareDirectories(string dirA, string dirB) {
        var filesA = GetUassetPaths(dirA);
        var filesB = GetUassetPaths(dirB);

        var allKeys = filesA.Keys.Union(filesB.Keys).OrderBy(k => k);

        foreach (var relPath in allKeys) {
            var assetA = GetUAsset(relPath, filesA);
            var assetB = GetUAsset(relPath, filesB);
            var context = DiffContext.From(assetA, assetB);
            var assetDiff = AssetDiff.Create(context, relPath, assetA, assetB);

            PrintAssetDiff(assetDiff);
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

    private static void PrintAssetDiff(AssetDiff assetDiff) {
        PrintDiffType(assetDiff, "Asset", 0);

        if (assetDiff.ChangedProperties.Any()) {
            Console.WriteLine("  Property changes:");

            foreach (var pd in assetDiff.ChangedProperties) {
                PrintPropertyDiff(pd, 2);
            }
        }

        if (assetDiff.ChangedFunctions.Any()) {
            Console.WriteLine("  Function changes:");

            foreach (var fd in assetDiff.ChangedFunctions) {
                PrintFunctionDiff(fd, 2);
            }
        }
    }

    private static void PrintPropertyDiff(PropertyDiff diff, int indent) {
        PrintDiffType(diff, "Property", indent);

        if (diff.DiffType == DiffType.Changed) {
            
            PrintValueChange(diff.Type, "Type", indent + 2);
            PrintValueChange(diff.StructClass, "StructClass", indent + 2);
            PrintValueChange(diff.PropertyClass, "PropertyClass", indent + 2);
            PrintValueChange(diff.ArrayDim, "ArrayDim", indent + 2);

            foreach (var inner in diff.InnerProperties.Values) {
                PrintPropertyDiff(inner, indent + 1);
            }
        }
    }

    private static void PrintFunctionDiff(FunctionDiff diff, int indent) {
        var prefix = Indent(indent);

        PrintDiffType(diff, "Function", indent);

        if (diff.DiffType == DiffType.Changed) {
            if (diff.ChangedInputProperties.Any()) {
                Console.WriteLine($"{prefix}    Input param changes:");

                foreach (var pd in diff.ChangedInputProperties) {
                    PrintPropertyDiff(pd, indent + 3);
                }
            }

            if (diff.ChangedOutputProperties.Any()) {
                Console.WriteLine($"{prefix}    Output param changes:");

                foreach (var pd in diff.ChangedOutputProperties) {
                    PrintPropertyDiff(pd, indent + 3);
                }
            }
        }
    }

    private static void PrintValueChange<T>(ValueChange<T> change, string title, int indentLevel) {
        if (change.DiffType == DiffType.Unchanged) {
            return;
        }
        
        Console.WriteLine($"{Indent(indentLevel)}{title}: {change.From} => {change.To}");
    }

    private static void PrintDiffType(Diff diff, string title, int indentLevel) {
        if (diff.DiffType == DiffType.Unchanged) {
            return;
        }

        var signs = new Dictionary<DiffType, string> {
                { DiffType.Added, "+" },
                { DiffType.Removed, "-" },
                { DiffType.Changed, "~" },
        };
        var sign = signs[diff.DiffType];
            
        Console.WriteLine($"{Indent(indentLevel)}{sign} {title} '{diff.Name}' {diff.DiffType}");
    }

    private static string Indent(int level) {
        return new string(' ', level * 2);
    }
}
