using UAssetAPI.UnrealTypes;

namespace UassetComparisonTool.Diffs;

public class ValueChange<T> : IChangeable {

    public DiffType DiffType { get; }

    public readonly T? From;

    public readonly T? To;

    private ValueChange(T? from, T? to) {
        DiffType = ResolveDiffType(from, to);
        From = from;
        To = to;
    }

    private static DiffType ResolveDiffType(T? from, T? to) {
        if (Equals(from, to)) {
            return DiffType.Unchanged;
        }

        if (from is null) {
            return DiffType.Added;
        }

        if (to is null) {
            return DiffType.Removed;
        }

        return DiffType.Changed;
    }

    public static ValueChange<string> Create(DiffContext context, FPackageIndex a, FPackageIndex b) {
        var objectTypeA = a.Index > 0
            ? a.ToExport(context.AssetA).ObjectName.ToString()
            : a.ToImport(context.AssetA).ObjectName.ToString();
        var objectTypeB = b.Index > 0
            ? b.ToExport(context.AssetB).ObjectName.ToString()
            : b.ToImport(context.AssetB).ObjectName.ToString();

        return new ValueChange<string>(objectTypeA, objectTypeB);
    }

    public static ValueChange<T> Create(T? from, T? to) {
        return new ValueChange<T>(from, to);
    }

    public static ValueChange<T> Default() {
        return new ValueChange<T>(default, default);
    }
}
