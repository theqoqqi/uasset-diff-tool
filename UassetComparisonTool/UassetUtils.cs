using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.FieldTypes;
using UAssetAPI.UnrealTypes;

namespace UassetComparisonTool;

public static class UassetUtils {

    public static Dictionary<string, FProperty> CollectProperties(UAsset asset) {
        return CollectProperties(GetClassExport(asset));
    }

    public static Dictionary<string, FProperty> CollectProperties(StructExport export) {
        return CollectProperties(export, _ => true);
    }

    public static Dictionary<string, FProperty> CollectProperties(StructExport export, Func<FProperty, bool> filter) {
        return export.LoadedProperties
                .Where(filter)
                .GroupBy(p => p.Name.ToString())
                .Where(grouping => grouping.Count() == 1)
                .Select(grouping => grouping.First())
                .ToDictionary<FProperty, string>(p => p.Name.ToString());
    }

    public static Dictionary<string, FunctionExport> CollectFunctions(UAsset asset) {
        return asset.Exports
                .Where(IsFunction)
                .Cast<FunctionExport>()
                .ToDictionary<FunctionExport, string>(export => export.ObjectName.ToString());
    }

    public static bool HasBlueprints(UAsset? asset) {
        return asset?.Exports.Find(IsClassExport) != null;
    }

    public static ClassExport GetClassExport(UAsset asset) {
        return (ClassExport) asset.Exports.First(IsClassExport);
    }

    public static FunctionExport GetConstructionScript(UAsset asset) {
        return (FunctionExport) asset.Exports.First(IsUserConstructionScript);
    }

    public static bool IsUserConstructionScript(Export export) {
        return export.ObjectName.Value.Value == "UserConstructionScript";
    }

    public static bool IsFunction(Export export) {
        return CheckTypeName(export, "Function");
    }

    public static bool IsClassExport(Export export) {
        return CheckTypeName(export, "BlueprintGeneratedClass");
    }

    public static bool CheckTypeName(Export export, string typeName) {
        return export.GetExportClassType().Value.Value == typeName;
    }

    public static bool IsInputParam(FProperty p) {
        return p.PropertyFlags.HasFlag(EPropertyFlags.CPF_Parm) && !IsOutputParam(p);
    }

    public static bool IsOutputParam(FProperty p) {
        return p.PropertyFlags.HasFlag(EPropertyFlags.CPF_OutParm);
    }
}
