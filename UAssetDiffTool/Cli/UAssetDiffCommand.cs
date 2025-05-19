using System.CommandLine;
using System.CommandLine.Parsing;
using UAssetDiffTool.Diffs;

namespace UAssetDiffTool.Cli;

public class UAssetDiffCommand : RootCommand {

    private new const string Description = "UAsset diff tool";

    public static readonly Argument<string> PathA = new Argument<string>(
            name: "pathA",
            description: "First .uasset file or directory to compare."
    );

    public static readonly Argument<string> PathB = new Argument<string>(
            name: "pathB",
            description: "Second .uasset file or directory to compare."
    );

    public static readonly Option<string?> OutputPath = new Option<string?>(
            aliases: ["--output", "-o"],
            getDefaultValue: () => null,
            description: "If set, write diff to this file; otherwise write to console."
    ).ExistingOrCreateableFile();

    public static readonly Option<FileInfo?> RenamedFiles = new Option<FileInfo?>(
            aliases: ["--renamed-files", "-r"],
            description: "File with old->new asset path mappings (space separated)."
    ).LegalFilePathsOnly();

    public static readonly Option<FileInfo?> FilterByDeps = new Option<FileInfo?>(
            aliases: ["--filter-by-deps", "-D"],
            description: "Only show diffs for assets listed in this dependency file."
    ).LegalFilePathsOnly();

    public static readonly Option<DiffType[]> DiffTypes = new Option<DiffType[]>(
            aliases: ["--diff-types", "-d"],
            getDefaultValue: () => [
                    DiffType.Added,
                    DiffType.Removed,
                    DiffType.Changed
            ],
            description: "Which diff types to include (comma or space separated)."
    ) {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<bool> BlueprintsOnly = new Option<bool>(
            name: "--blueprints-only",
            description: "Only include assets that are Blueprint classes (i.e. have functions or properties)."
    );

    public UAssetDiffCommand() : base(Description) {
        Add(PathA);
        Add(PathB);
        Add(OutputPath);
        Add(RenamedFiles);
        Add(FilterByDeps);
        Add(DiffTypes);
        Add(BlueprintsOnly);
    }

    public void SetHandler(Action<ParsedSymbols> handler) {
        this.SetHandler(context => {
            handler(new ParsedSymbols(context.ParseResult));
        });
    }

    public class ParsedSymbols(ParseResult result) {

        public readonly string PathA = result.GetValueForArgument(UAssetDiffCommand.PathA);

        public readonly string PathB = result.GetValueForArgument(UAssetDiffCommand.PathB);

        public readonly string? OutputPath = result.GetValueForOption(UAssetDiffCommand.OutputPath);

        public readonly FileInfo? RenamedFiles = result.GetValueForOption(UAssetDiffCommand.RenamedFiles);

        public readonly FileInfo? FilterByDeps = result.GetValueForOption(UAssetDiffCommand.FilterByDeps);

        public readonly DiffType[] DiffTypes = result.GetValueForOption(UAssetDiffCommand.DiffTypes) ?? [];

        public readonly bool BlueprintsOnly = result.GetValueForOption(UAssetDiffCommand.BlueprintsOnly);
    }
}
