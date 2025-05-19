using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using static UAssetDiffTool.UassetUtils;

namespace UAssetDiffTool.Diffs;

public class FunctionDiff(DiffType diffType, string name) : Diff(diffType, name) {

    [JsonProperty]
    public FlagsChange<EFunctionFlags> FunctionFlags = FlagsChange<EFunctionFlags>.Default();

    [JsonProperty]
    public Dictionary<string, PropertyDiff> InputProperties { get; private set; } = new();

    [JsonProperty]
    public Dictionary<string, PropertyDiff> OutputProperties { get; private set; } = new();

    public IEnumerable<PropertyDiff> ChangedInputProperties =>
            InputProperties.Values.Where(d => d.DiffType != DiffType.Unchanged);

    public IEnumerable<PropertyDiff> ChangedOutputProperties =>
            OutputProperties.Values.Where(d => d.DiffType != DiffType.Unchanged);

    protected override IList<IChangeable> CollectChildren() {
        var children = new List<IChangeable> {
                FunctionFlags,
        };

        children.AddRange(InputProperties.Values);
        children.AddRange(OutputProperties.Values);

        return children;
    }

    public static Dictionary<string, FunctionDiff> Create(DiffContext context, UAsset? assetA, UAsset? assetB) {
        var functionsA = CollectFunctions(assetA);
        var functionsB = CollectFunctions(assetB);

        return CreateFromDictionary(
                context,
                functionsA,
                functionsB,
                FindFunctionDiffs,
                (diffType, name) => new FunctionDiff(diffType, name)
        );
    }

    private static void FindFunctionDiffs(
            DiffContext context,
            FunctionDiff diff,
            FunctionExport? a,
            FunctionExport? b
    ) {
        var inputParamsA = CollectProperties(a, IsInputParam);
        var inputParamsB = CollectProperties(b, IsInputParam);
        var outputParamsA = CollectProperties(a, IsOutputParam);
        var outputParamsB = CollectProperties(b, IsOutputParam);

        diff.FunctionFlags = FlagsChange<EFunctionFlags>.Create(a?.FunctionFlags, b?.FunctionFlags);
        diff.InputProperties = PropertyDiff.Create(context, inputParamsA, inputParamsB);
        diff.OutputProperties = PropertyDiff.Create(context, outputParamsA, outputParamsB);
    }
}
