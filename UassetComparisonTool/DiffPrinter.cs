using UassetComparisonTool.Diffs;

namespace UassetComparisonTool;

public class DiffPrinter {

    private TextWriter Writer;

    public DiffPrinter(TextWriter writer) {
        Writer = writer;
    }

    public void PrintDiff(AssetDiff assetDiff) {
        PrintDiffType(assetDiff, "Asset", 0);

        if (assetDiff.ChangedProperties.Any()) {
            Writer.WriteLine("  Property changes:");

            foreach (var pd in assetDiff.ChangedProperties) {
                PrintPropertyDiff(pd, 2);
            }
        }

        if (assetDiff.ChangedFunctions.Any()) {
            Writer.WriteLine("  Function changes:");

            foreach (var fd in assetDiff.ChangedFunctions) {
                PrintFunctionDiff(fd, 2);
            }
        }
    }

    private void PrintPropertyDiff(PropertyDiff diff, int indent) {
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

    private void PrintFunctionDiff(FunctionDiff diff, int indent) {
        var prefix = Indent(indent);

        PrintDiffType(diff, "Function", indent);

        if (diff.DiffType == DiffType.Changed) {
            if (diff.ChangedInputProperties.Any()) {
                Writer.WriteLine($"{prefix}    Input param changes:");

                foreach (var pd in diff.ChangedInputProperties) {
                    PrintPropertyDiff(pd, indent + 3);
                }
            }

            if (diff.ChangedOutputProperties.Any()) {
                Writer.WriteLine($"{prefix}    Output param changes:");

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
        
        Writer.WriteLine($"{Indent(indentLevel)}{title}: {change.From} => {change.To}");
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
            
        Writer.WriteLine($"{Indent(indentLevel)}{sign} {title} '{diff.Name}' {diff.DiffType}");
    }

    private static string Indent(int level) {
        return new string(' ', level * 2);
    }
}
