using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UAssetDiffTool.Diffs.Json;

public class DiffContractResolver : DefaultContractResolver {

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        var prop = base.CreateProperty(member, memberSerialization);

        prop.ShouldSerialize = instance => {
            var value = prop.ValueProvider?.GetValue(instance);

            if (value is IDictionary dict && dict.Count == 0) {
                return false;
            }

            return value is not IChangeable {
                    DiffType: DiffType.Unchanged
            };
        };

        return prop;
    }
}
