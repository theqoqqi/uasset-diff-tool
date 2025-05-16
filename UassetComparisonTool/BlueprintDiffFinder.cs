using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.FieldTypes;
using UAssetAPI.UnrealTypes;
using static UassetComparisonTool.UassetUtils;

namespace UassetComparisonTool;

public class BlueprintDiffFinder {

    private UAsset assetA;

    private UAsset assetB;

    public void FindDiff(UAsset assetA, UAsset assetB) {
        if (!HasBlueprints(assetA) || !HasBlueprints(assetB)) {
            return;
        }

        this.assetA = assetA;
        this.assetB = assetB;
        
        FindPropertyDiffs(assetA, assetB);
        FindFunctionDiffs(assetA, assetB);
    }

    private void FindFunctionDiffs(UAsset assetA, UAsset assetB) {
        var functionsA = CollectFunctions(assetA);
        var functionsB = CollectFunctions(assetB);

        Console.WriteLine("Function diffs:");
        
        FindDiffs(
                functionsA,
                functionsB,
                f => f.ObjectName.ToString(),
                FindFunctionDiffs
        );

        Console.WriteLine();
    }

    private void FindFunctionDiffs(FunctionExport a, FunctionExport b) {
        var inputParamsA = CollectProperties(a, IsInputParam);
        var inputParamsB = CollectProperties(b, IsInputParam);
        var outputParamsA = CollectProperties(a, IsOutputParam);
        var outputParamsB = CollectProperties(b, IsOutputParam);
        
        Console.WriteLine(a.ObjectName + " function diffs:");

        Console.WriteLine("Input params:");
        FindPropertyDiffs(inputParamsA, inputParamsB);
        Console.WriteLine("Output params:");
        FindPropertyDiffs(outputParamsA, outputParamsB);

        Console.WriteLine();
    }

    private void FindPropertyDiffs(UAsset assetA, UAsset assetB) {
        var propertiesA = CollectProperties(assetA);
        var propertiesB = CollectProperties(assetB);

        Console.WriteLine("Property diffs:");

        FindPropertyDiffs(propertiesA, propertiesB);

        Console.WriteLine();
    }

    private void FindPropertyDiffs(
            Dictionary<string, FProperty> propertiesA,
            Dictionary<string, FProperty> propertiesB
    ) {
        FindDiffs(
                propertiesA,
                propertiesB,
                p => p.Name.ToString(),
                FindPropertyDiffs
        );
    }

    private void FindPropertyDiffs(FProperty a, FProperty b) {
        var typeA = a.GetType();
        var typeB = b.GetType();

        if (a.ArrayDim != b.ArrayDim) {
            Console.WriteLine($"Array dim changed: {a.ArrayDim} => {b.ArrayDim}");
        }

        if (typeA != typeB) {
            Console.WriteLine($"Type changed: {typeA} => {typeB}");
            return;
        }

        if (a is FObjectProperty objectA && b is FObjectProperty objectB) {
            FindPackageIndexDiffs(objectA.PropertyClass, objectB.PropertyClass, "Property class");
        }

        if (a is FArrayProperty arrayA && b is FArrayProperty arrayB) {
            FindPropertyDiffs(arrayA.Inner, arrayB.Inner);
        }
        
        if (a is FSetProperty setA && b is FSetProperty setB) {
            FindPropertyDiffs(setA.ElementProp, setB.ElementProp);
        }
        
        if (a is FMapProperty mapA && b is FMapProperty mapB) {
            FindPropertyDiffs(mapA.KeyProp, mapB.KeyProp);
            FindPropertyDiffs(mapA.ValueProp, mapB.ValueProp);
        }
    }

    private void FindPackageIndexDiffs(FPackageIndex a, FPackageIndex b, string title) {
        var objectTypeA = a.ToImport(assetA).ObjectName;
        var objectTypeB = b.ToImport(assetB).ObjectName;
            
        if (objectTypeA != objectTypeB) {
            Console.WriteLine($"{title} changed: {objectTypeA} => {objectTypeB}");
        }
    }

    private bool FindStringDiffs<T>(T? a, T? b, Func<T, string> getter) {
        if (a is null) {
            Console.WriteLine("Added: " + getter(b!));
            return true;
        }

        if (b is null) {
            Console.WriteLine("Removed: " + getter(a));
            return true;
        }

        return false;
    }

    private void FindDiffs<T>(
            Dictionary<string, T> dictionaryA,
            Dictionary<string, T> dictionaryB,
            Func<T, string> nameGetter,
            Action<T, T> findDiffs
    ) {
        foreach (var key in dictionaryA.Keys.Union(dictionaryB.Keys)) {
            var itemA = dictionaryA.GetValueOrDefault(key);
            var itemB = dictionaryB.GetValueOrDefault(key);

            if (!FindStringDiffs(itemA, itemB, nameGetter)) {
                findDiffs(itemA!, itemB!);
            }
        }
    }
}
