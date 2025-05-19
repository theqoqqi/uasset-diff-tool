using System.Collections.Concurrent;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetDiffTool.Diffs;

namespace UAssetDiffTool.Cli {

    public class UAssetDiffCommandContext {

        public readonly bool IsSingleFileDiff;

        public readonly UAsset? AssetA;

        public readonly UAsset? AssetB;

        public readonly Dictionary<string, UAsset>? AssetsA;

        public readonly Dictionary<string, UAsset>? AssetsB;

        public readonly DiffPrinter DiffPrinter;

        public readonly Dictionary<string, string> RenamedFiles;

        public readonly HashSet<string>? FilterByDeps;

        public readonly bool BlueprintsOnly;

        public UAssetDiffCommandContext(UAssetDiffCommand.ParsedSymbols symbols) {
            IsSingleFileDiff = IsFiles(symbols.PathA, symbols.PathB);
            AssetA = GetSingleAsset(symbols.PathA);
            AssetB = GetSingleAsset(symbols.PathB);
            AssetsA = CollectAssets(symbols.PathA);
            AssetsB = CollectAssets(symbols.PathB);
            DiffPrinter = CreateDiffPrinter(symbols.OutputPath, symbols.DiffTypes, symbols.ExpandAddedItems);
            RenamedFiles = ParseRenamedFiles(symbols.RenamedFiles);
            FilterByDeps = ParseDependencyFile(symbols.FilterByDeps);
            BlueprintsOnly = symbols.BlueprintsOnly;
        }

        private UAsset? GetSingleAsset(string path) {
            return IsSingleFileDiff ? GetAsset(path) : null;
        }

        private Dictionary<string, UAsset>? CollectAssets(string path) {
            if (IsSingleFileDiff) {
                return null;
            }

            var assets = new ConcurrentDictionary<string, UAsset>();
            var paths = Directory.GetFiles(path, "*.uasset", SearchOption.AllDirectories);

            Parallel.ForEach(paths, assetPath => {
                AddAsset(assets, assetPath);
            });

            return new Dictionary<string, UAsset>(assets);
        }

        private static void AddAsset(IDictionary<string, UAsset> assets, string path) {
            assets.Add(GetShortAssetPath(path), GetAsset(path));
        }

        private static UAsset GetAsset(string path) {
            return new UAsset(path, EngineVersion.VER_UE4_27);
        }

        private static DiffPrinter CreateDiffPrinter(string? outputPath, DiffType[] diffTypes, bool expandAddedItems) {
            return new DiffPrinter(GetWriter(outputPath), diffTypes, expandAddedItems);
        }

        private static TextWriter GetWriter(string? outputPath) {
            if (outputPath is null) {
                return Console.Out;
            }

            return new StreamWriter(outputPath) {
                    AutoFlush = false
            };
        }

        private bool IsFiles(string pathA, string pathB) {
            return File.Exists(pathA) && File.Exists(pathB);
        }

        private static Dictionary<string, string> ParseRenamedFiles(FileInfo? renamedFiles) {
            if (renamedFiles is null) {
                return new Dictionary<string, string>();
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(renamedFiles.FullName)) {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2) {
                    var oldKey = GetShortAssetPath(parts[0]);
                    var newKey = GetShortAssetPath(parts[1]);

                    map[oldKey] = newKey;
                }
            }

            return map;
        }

        private static HashSet<string>? ParseDependencyFile(FileSystemInfo? filterByDeps) {
            if (filterByDeps is null) {
                return null;
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(filterByDeps.FullName)) {
                var trimmed = line.Trim().Replace('/', '\\');

                if (trimmed.StartsWith("\\Game\\") && !trimmed.StartsWith("\\Game\\Mods\\")) {
                    var withoutPrefix = trimmed.Substring("\\Game\\".Length);
                    var withoutExtension = withoutPrefix.Split('.')[0];

                    result.Add(withoutExtension);
                }
            }

            return result;
        }

        private static string GetShortAssetPath(string relativePath) {
            var convertedPath = relativePath.Replace('/', '\\');
            var idx = convertedPath.IndexOf("Content\\", StringComparison.Ordinal);

            if (idx < 0) {
                return convertedPath;
            }

            var shortPath = convertedPath.Substring(idx + "Content\\".Length);

            return Path.ChangeExtension(shortPath, null);
        }
    }
}
