namespace UassetComparisonTool.Diffs;

public abstract class Diff(DiffType diffType, string name) : IChangeable {

    public DiffType DiffType { get; private set; } = diffType;

    public readonly string Name = name;

    protected void ResolveDiffType() {
        if (DiffType != DiffType.Unchanged) {
            return;
        }

        var children = CollectChildren();

        foreach (var child in children) {
            (child as Diff)?.ResolveDiffType();
        }
        
        DiffType = GetChildrenDiffType(children);
    }

    private DiffType GetChildrenDiffType(IEnumerable<IChangeable> children) {
        return children.All(c => c.DiffType == DiffType.Unchanged)
                ? DiffType.Unchanged
                : DiffType.Changed;
    }

    protected virtual IList<IChangeable> CollectChildren() {
        return [];
    }

    protected static Dictionary<string, D> CreateFromDictionary<T, D>(
            DiffContext context,
            Dictionary<string, T> dictionaryA,
            Dictionary<string, T> dictionaryB,
            Action<DiffContext, D, T, T> findDiffs,
            Func<string, D> diffFactory
    ) where D : Diff {
        var diffs = new Dictionary<string, D>();
        
        foreach (var key in dictionaryA.Keys.Union(dictionaryB.Keys)) {
            var diff = diffFactory(key);
            var itemA = dictionaryA.GetValueOrDefault(key);
            var itemB = dictionaryB.GetValueOrDefault(key);

            if (!FindAddRemoveDiffs(diff, itemA, itemB)) {
                findDiffs(context, diff, itemA!, itemB!);
                diff.ResolveDiffType();
            }

            diffs.Add(key, diff);
        }

        return diffs;
    }
    
    protected static bool FindAddRemoveDiffs<T>(Diff diff, T? a, T? b) {
        if (a is null) {
            diff.DiffType = DiffType.Added;
            return true;
        }

        if (b is null) {
            diff.DiffType = DiffType.Removed;
            return true;
        }

        return false;
    }
}
