using Newtonsoft.Json;

namespace UAssetDiffTool.Diffs;

public class FlagsChange<T> : IChangeable where T : struct, Enum {

    public DiffType DiffType { get; }

    [JsonProperty]
    public readonly T? From;

    [JsonProperty]
    public readonly T? To;

    [JsonProperty]
    public readonly T Added;

    [JsonProperty]
    public readonly T Removed;

    public readonly bool HasAddedFlags;

    public readonly bool HasRemovedFlags;

    private FlagsChange(T? from, T? to) {
        DiffType = ResolveDiffType(from, to);
        From = from;
        To = to;
        Added = BitwiseAndNot(To, From);
        Removed = BitwiseAndNot(From, To);
        HasAddedFlags = !Added.Equals(default(T));
        HasRemovedFlags = !Removed.Equals(default(T));
    }

    private DiffType ResolveDiffType(T? from, T? to) {
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

    private static T BitwiseAndNot(T? a, T? b) {
        var ua = Convert.ToUInt64(a);
        var ub = Convert.ToUInt64(b);
        var result = ua & ~ub;

        return (T) Enum.ToObject(typeof(T), result);
    }

    public static FlagsChange<T> Create(T? from, T? to) {
        return new FlagsChange<T>(from, to);
    }

    public static FlagsChange<T> Default() {
        return new FlagsChange<T>(default, default);
    }
}
