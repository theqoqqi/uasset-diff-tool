using UAssetDiffTool.Diffs;

namespace UAssetDiffTool;

public class DiffPrinter(TextWriter writer, IEnumerable<DiffType> filteredDiffTypes, bool expandAddedItems) {

    public void PrintDiffs(IEnumerable<AssetDiff> assetDiffs) {
        foreach (var assetDiff in assetDiffs) {
            PrintDiff(assetDiff);
        }
    }

    public void PrintDiff(AssetDiff assetDiff) {
        if (!filteredDiffTypes.Contains(assetDiff.DiffType)) {
            return;
        }

        PrintDiffType(assetDiff, "Asset", 0);

        if (ShouldPrintChildDiffs(assetDiff.DiffType)) {
            PrintValueChange(assetDiff.Path, "Path", 1);

            if (assetDiff.ChangedProperties.Any()) {
                writer.WriteLine("  Property changes:");

                foreach (var pd in assetDiff.ChangedProperties) {
                    PrintPropertyDiff(pd, 2);
                }
            }

            if (assetDiff.ChangedFunctions.Any()) {
                writer.WriteLine("  Function changes:");

                foreach (var fd in assetDiff.ChangedFunctions) {
                    PrintFunctionDiff(fd, 2);
                }
            }
        }

        writer.Flush();
    }

    private void PrintPropertyDiff(PropertyDiff diff, int indent) {
        PrintDiffType(diff, "Property", indent);

        if (ShouldPrintChildDiffs(diff.DiffType)) {
            PrintFlagsChange(diff.PropertyFlags, "Flags", indent + 2);

            PrintValueChange(diff.Type, "Type", indent + 2);
            PrintValueChange(diff.StructClass, "StructClass", indent + 2);
            PrintValueChange(diff.PropertyClass, "PropertyClass", indent + 2);
            PrintValueChange(diff.ArrayDim, "ArrayDim", indent + 2);

            foreach (var inner in diff.InnerProperties.Values) {
                PrintPropertyDiff(inner, indent + 1);
            }
        }
    }

    private void PrintFunctionDiff(FunctionDiff diff, int indent) {
        var prefix = Indent(indent);

        PrintDiffType(diff, "Function", indent);

        if (ShouldPrintChildDiffs(diff.DiffType)) {
            PrintFlagsChange(diff.FunctionFlags, "Flags", indent + 2);

            if (diff.ChangedInputProperties.Any()) {
                writer.WriteLine($"{prefix}    Input param changes:");

                foreach (var pd in diff.ChangedInputProperties) {
                    PrintPropertyDiff(pd, indent + 3);
                }
            }

            if (diff.ChangedOutputProperties.Any()) {
                writer.WriteLine($"{prefix}    Output param changes:");

                foreach (var pd in diff.ChangedOutputProperties) {
                    PrintPropertyDiff(pd, indent + 3);
                }
            }
        }
    }

    private void PrintValueChange<T>(ValueChange<T> change, string title, int indentLevel) {
        if (change.DiffType == DiffType.Unchanged) {
            return;
        }

        writer.WriteLine($"{Indent(indentLevel)}{title}: {change.From} => {change.To}");
    }

    private void PrintFlagsChange<T>(FlagsChange<T> change, string title, int indentLevel) where T : struct, Enum {
        if (change.DiffType == DiffType.Unchanged) {
            return;
        }

        writer.WriteLine($"{Indent(indentLevel)}{title}: {change.DiffType}");

        if (change.HasAddedFlags) {
            writer.WriteLine($"{Indent(indentLevel + 1)}Added: {change.Added}");
        }

        if (change.HasRemovedFlags) {
            writer.WriteLine($"{Indent(indentLevel + 1)}Removed: {change.Removed}");
        }
    }

    private void PrintDiffType(Diff diff, string title, int indentLevel) {
        if (diff.DiffType == DiffType.Unchanged) {
            return;
        }

        var signs = new Dictionary<DiffType, string> {
                { DiffType.Added, "+" },
                { DiffType.Removed, "-" },
                { DiffType.Changed, "~" },
        };
        var sign = signs[diff.DiffType];

        writer.WriteLine($"{Indent(indentLevel)}{sign} {title} '{diff.Name}' {diff.DiffType}");
    }

    private bool ShouldPrintChildDiffs(DiffType diffType) {
        return expandAddedItems
                ? diffType is DiffType.Changed or DiffType.Added
                : diffType is DiffType.Changed;
    }

    private static string Indent(int level) {
        return new string(' ', level * 2);
    }
}
