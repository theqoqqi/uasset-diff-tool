namespace UassetComparisonTool.Diffs;

public class FlagsChange<T> : IChangeable where T : struct, Enum {

    public DiffType DiffType { get; }

    public readonly T From;

    public readonly T To;

    public readonly T Added;

    public readonly T Removed;

    public readonly bool HasAddedFlags;

    public readonly bool HasRemovedFlags;

    private FlagsChange(T from, T to) {
        DiffType = ResolveDiffType(from, to);
        From = from;
        To = to;
        Added = BitwiseAndNot(To, From);
        Removed = BitwiseAndNot(From, To);
        HasAddedFlags = !Added.Equals(default(T));
        HasRemovedFlags = !Removed.Equals(default(T));
    }

    private DiffType ResolveDiffType(T from, T to) {
        return from.Equals(to) ? DiffType.Unchanged : DiffType.Changed;
    }

    private static T BitwiseAndNot(T a, T b) {
        var ua = Convert.ToUInt64(a);
        var ub = Convert.ToUInt64(b);
        var result = ua & ~ub;

        return (T) Enum.ToObject(typeof(T), result);
    }
    
    public static FlagsChange<T> Create(T from, T to) {
        return new FlagsChange<T>(from, to);
    }

    public static FlagsChange<T> Default() {
        return new FlagsChange<T>(default, default);
    }
}
