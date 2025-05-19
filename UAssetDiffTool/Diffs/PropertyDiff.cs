using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.FieldTypes;
using UAssetAPI.UnrealTypes;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool.Diffs;

public class PropertyDiff : Diff {

    [JsonProperty]
    public FlagsChange<EPropertyFlags> PropertyFlags = FlagsChange<EPropertyFlags>.Default();

    [JsonProperty]
    public ValueChange<string> Type { get; private set; } = ValueChange<string>.Default();

    [JsonProperty]
    public ValueChange<string> StructClass { get; private set; } = ValueChange<string>.Default();

    [JsonProperty]
    public ValueChange<string> PropertyClass { get; private set; } = ValueChange<string>.Default();

    [JsonProperty]
    public ValueChange<EArrayDim?> ArrayDim { get; private set; } = ValueChange<EArrayDim?>.Default();

    [JsonProperty]
    public Dictionary<string, PropertyDiff> InnerProperties { get; } = new();

    private PropertyDiff(DiffType diffType, string name) : base(diffType, name) {

    }

    protected override IList<IChangeable> CollectChildren() {
        var children = new List<IChangeable>();

        children.AddRange(InnerProperties.Values);
        children.AddRange([
                PropertyFlags,
                Type,
                StructClass,
                PropertyClass,
                ArrayDim,
        ]);

        return children;
    }

    private void AddInnerProperty(PropertyDiff diff) {
        InnerProperties.Add(diff.Name, diff);
    }

    public static Dictionary<string, PropertyDiff> Create(DiffContext context, UAsset? assetA, UAsset? assetB) {
        var propertiesA = CollectProperties(assetA);
        var propertiesB = CollectProperties(assetB);

        return Create(context, propertiesA, propertiesB);
    }

    public static Dictionary<string, PropertyDiff> Create(
            DiffContext context,
            Dictionary<string, FProperty> propertiesA,
            Dictionary<string, FProperty> propertiesB
    ) {
        return CreateFromDictionary(
                context,
                propertiesA,
                propertiesB,
                FindDiffs,
                (diffType, name) => new PropertyDiff(diffType, name)
        );
    }

    private static PropertyDiff Create(DiffContext context, string name, FProperty? a, FProperty? b) {
        var diff = new PropertyDiff(DiffType.Unchanged, name);

        if (a is not null || b is not null) {
            FindDiffs(context, diff, a, b);
            diff.ResolveDiffType();
        }

        return diff;
    }

    private static void FindDiffs(DiffContext context, PropertyDiff diff, FProperty? a, FProperty? b) {
        diff.PropertyFlags = FlagsChange<EPropertyFlags>.Create(a?.PropertyFlags, b?.PropertyFlags);
        diff.ArrayDim = ValueChange<EArrayDim?>.Create(a?.ArrayDim, b?.ArrayDim);
        diff.Type = ValueChange<string>.Create(a?.SerializedType.ToString(), b?.SerializedType.ToString());

        if (a is FStructProperty || b is FStructProperty) {
            diff.StructClass = ValueChange<string>.Create(
                    context,
                    (a as FStructProperty)?.Struct,
                    (b as FStructProperty)?.Struct
            );
        }

        if (a is FObjectProperty || b is FObjectProperty) {
            diff.PropertyClass = ValueChange<string>.Create(
                    context,
                    (a as FObjectProperty)?.PropertyClass,
                    (b as FObjectProperty)?.PropertyClass
            );
        }

        if (a is FArrayProperty || b is FArrayProperty) {
            diff.AddInnerProperty(Create(
                    context,
                    "Inner",
                    (a as FArrayProperty)?.Inner,
                    (b as FArrayProperty)?.Inner
            ));
        }

        if (a is FSetProperty || b is FSetProperty) {
            diff.AddInnerProperty(Create(
                    context,
                    "ElementProp",
                    (a as FSetProperty)?.ElementProp,
                    (b as FSetProperty)?.ElementProp
            ));
        }

        if (a is FMapProperty || b is FMapProperty) {
            diff.AddInnerProperty(Create(
                    context,
                    "KeyProp",
                    (a as FMapProperty)?.KeyProp,
                    (b as FMapProperty)?.KeyProp
            ));

            diff.AddInnerProperty(Create(
                    context,
                    "ValueProp",
                    (a as FMapProperty)?.ValueProp,
                    (b as FMapProperty)?.ValueProp
            ));
        }
    }
}
