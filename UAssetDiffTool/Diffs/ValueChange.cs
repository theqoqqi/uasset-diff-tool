using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UAssetDiffTool.Diffs;

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
        var objectTypeA = StringifyPackageIndex(a, context.AssetA);
        var objectTypeB = StringifyPackageIndex(b, context.AssetB);

        return new ValueChange<string>(objectTypeA, objectTypeB);
    }

    private static string? StringifyPackageIndex(FPackageIndex packageIndex, UAsset? asset) {
        return packageIndex.Index > 0
                ? packageIndex.ToExport(asset).ObjectName.ToString()
                : packageIndex.ToImport(asset).ObjectName.ToString();
    }

    public static ValueChange<T> Create(T? from, T? to) {
        return new ValueChange<T>(from, to);
    }

    public static ValueChange<T> Default() {
        return new ValueChange<T>(default, default);
    }
}
