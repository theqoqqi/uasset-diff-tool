using UAssetAPI;
using UAssetAPI.FieldTypes;
using UAssetAPI.UnrealTypes;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool.Diffs;

public class PropertyDiff : Diff {

    public FlagsChange<EPropertyFlags> PropertyFlags = FlagsChange<EPropertyFlags>.Default();

    public ValueChange<string> Type { get; private set; } = ValueChange<string>.Default();
    
    public ValueChange<string> StructClass { get; private set; } = ValueChange<string>.Default();

    public ValueChange<string> PropertyClass { get; private set; } = ValueChange<string>.Default();

    public ValueChange<EArrayDim> ArrayDim { get; private set; } = ValueChange<EArrayDim>.Default();

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

    public static Dictionary<string, PropertyDiff> Create(DiffContext context, UAsset assetA, UAsset assetB) {
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
                name => new PropertyDiff(DiffType.Unchanged, name)
        );
    }

    public static PropertyDiff Create(DiffContext context, string name, FProperty a, FProperty b) {
        var diff = new PropertyDiff(DiffType.Unchanged, name);

        FindDiffs(context, diff, a, b);
        diff.ResolveDiffType();
        
        return diff;
    }

    private static void FindDiffs(DiffContext context, PropertyDiff diff, FProperty a, FProperty b) {
        diff.PropertyFlags = FlagsChange<EPropertyFlags>.Create(a.PropertyFlags, b.PropertyFlags);
        diff.ArrayDim = ValueChange<EArrayDim>.Create(a.ArrayDim, b.ArrayDim);
        diff.Type = ValueChange<string>.Create(a.SerializedType.ToString(), b.SerializedType.ToString());

        if (a is FStructProperty structA && b is FStructProperty structB) {
            diff.StructClass = ValueChange<string>.Create(context, structA.Struct, structB.Struct);
        }

        if (a is FObjectProperty objectA && b is FObjectProperty objectB) {
            diff.PropertyClass = ValueChange<string>.Create(context, objectA.PropertyClass, objectB.PropertyClass);
        }

        if (a is FArrayProperty arrayA && b is FArrayProperty arrayB) {
            diff.AddInnerProperty(Create(context, "Inner", arrayA.Inner, arrayB.Inner));
        }
        
        if (a is FSetProperty setA && b is FSetProperty setB) {
            diff.AddInnerProperty(Create(context, "ElementProp", setA.ElementProp, setB.ElementProp));
        }
        
        if (a is FMapProperty mapA && b is FMapProperty mapB) {
            diff.AddInnerProperty(Create(context, "KeyProp", mapA.KeyProp, mapB.KeyProp));
            diff.AddInnerProperty(Create(context, "ValueProp", mapA.ValueProp, mapB.ValueProp));
        }
    }
}
