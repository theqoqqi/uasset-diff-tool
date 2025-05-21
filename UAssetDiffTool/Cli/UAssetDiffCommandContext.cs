using System.Collections.Concurrent;
using System.CommandLine.Parsing;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetDiffTool.Diffs;
using UAssetDiffTool.Diffs.Json;

namespace UAssetDiffTool.Cli {

    public class UAssetDiffCommandContext {

        public readonly bool IsSingleFileDiff;

        public readonly UAsset? AssetA;

        public readonly UAsset? AssetB;

        public readonly Dictionary<string, UAsset>? AssetsA;

        public readonly Dictionary<string, UAsset>? AssetsB;

        public readonly DiffPrinter DiffPrinter;

        public readonly JsonDiffWriter? JsonDiffWriter;

        public readonly Dictionary<string, string> RenamedFiles;

        public readonly HashSet<string>? FilterByDeps;

        public readonly bool BlueprintsOnly;

        public UAssetDiffCommandContext(ParseResult parseResult) {
            var pathA = parseResult.GetValueForArgument(UAssetDiffCommand.PathA);
            var pathB = parseResult.GetValueForArgument(UAssetDiffCommand.PathB);
            var outputPath = parseResult.GetValueForOption(UAssetDiffCommand.OutputPath);
            var jsonOutputPath = parseResult.GetValueForOption(UAssetDiffCommand.JsonOutputPath);
            var prettyJson = parseResult.GetValueForOption(UAssetDiffCommand.PrettyJson);
            var renamedFiles = parseResult.GetValueForOption(UAssetDiffCommand.RenamedFiles);
            var filterByDeps = parseResult.GetValueForOption(UAssetDiffCommand.FilterByDeps);
            var diffTypes = parseResult.GetValueForOption(UAssetDiffCommand.DiffTypes) ?? [];
            var blueprintsOnly = parseResult.GetValueForOption(UAssetDiffCommand.BlueprintsOnly);
            var expandAddedItems = parseResult.GetValueForOption(UAssetDiffCommand.ExpandAddedItems);

            DiffPrinter = CreateDiffPrinter(outputPath, diffTypes, expandAddedItems);
            JsonDiffWriter = CreateJsonDiffWriter(jsonOutputPath, prettyJson);
            RenamedFiles = ParseRenamedFiles(renamedFiles);
            FilterByDeps = ParseDependencyFile(filterByDeps);
            BlueprintsOnly = blueprintsOnly;

            IsSingleFileDiff = IsFiles(pathA, pathB);
            AssetA = GetSingleAsset(pathA);
            AssetB = GetSingleAsset(pathB);
            AssetsA = CollectAssets(pathA, FilterByDeps);
            AssetsB = CollectAssets(pathB, ApplyRenames(FilterByDeps, RenamedFiles));
        }

        private UAsset? GetSingleAsset(string path) {
            return IsSingleFileDiff ? GetAsset(path) : null;
        }

        private Dictionary<string, UAsset>? CollectAssets(string path, ICollection<string>? filteredPaths) {
            if (IsSingleFileDiff) {
                return null;
            }

            var assets = new ConcurrentDictionary<string, UAsset>();
            var paths = GetAssetPaths(path, filteredPaths);
            var totalAmount = paths.Length;
            var loadedAmount = 0;

            Parallel.ForEach(paths, assetPath => {
                Console.WriteLine($"{loadedAmount++} / {totalAmount}. Reading {assetPath}...");
                AddAsset(assets, assetPath);
            });

            return new Dictionary<string, UAsset>(assets);
        }

        private static string[] GetAssetPaths(string path, ICollection<string>? filteredPaths) {
            var paths = Directory.GetFiles(path, "*.uasset", SearchOption.AllDirectories);

            if (filteredPaths is null) {
                return paths;
            }
            
            return paths.Where(IsFiltered).ToArray();

            bool IsFiltered(string p) {
                return filteredPaths.Contains(GetShortAssetPath(p));
            }
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

        private JsonDiffWriter? CreateJsonDiffWriter(string? jsonOutputPath, bool prettyJson) {
            if (jsonOutputPath is null) {
                return null;
            }

            var formatting = prettyJson ? Formatting.Indented : Formatting.None;

            return new JsonDiffWriter(jsonOutputPath, formatting);
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

        private static HashSet<string>? ApplyRenames(
                IEnumerable<string>? paths,
                IReadOnlyDictionary<string, string> renamedFiles
        ) {
            return paths?
                    .Select(path => renamedFiles.GetValueOrDefault(path, path))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
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
