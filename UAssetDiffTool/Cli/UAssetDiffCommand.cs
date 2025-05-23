﻿using System.CommandLine;
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
    ).BothFilesOrBothDirectories(PathA);

    public static readonly Option<string?> OutputPath = new Option<string?>(
            aliases: ["--output", "-o"],
            getDefaultValue: () => null,
            description: "If set, write diff to this file; otherwise write to console."
    ).ExistingOrCreateableFile();

    public static readonly Option<string?> JsonOutputPath = new Option<string?>(
            aliases: ["--json-output", "-j"],
            getDefaultValue: () => null,
            description: "If set, also writes a detailed JSON report of the diffs to the specified file." +
                         "\nThe resulting JSON can be visualized using the web viewer (see README on GitHub)."
    ).ExistingOrCreateableFile();

    public static readonly Option<bool> PrettyJson = new Option<bool>(
            aliases: ["--pretty-json", "-J"],
            description: "Enable indented (pretty) formatting for JSON output."
    );

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

    public static readonly Option<bool> ExpandAddedItems = new Option<bool>(
            name: "--expand-added-items",
            description: "Show child diffs for items marked as Added"
    );

    public UAssetDiffCommand() : base(Description) {
        Add(PathA);
        Add(PathB);
        Add(OutputPath);
        Add(JsonOutputPath);
        Add(PrettyJson);
        Add(RenamedFiles);
        Add(FilterByDeps);
        Add(DiffTypes);
        Add(BlueprintsOnly);
        Add(ExpandAddedItems);
    }

    public void SetHandler(Action<UAssetDiffCommandContext> handler) {
        this.SetHandler(context => {
            handler(new UAssetDiffCommandContext(context.ParseResult));
        });
    }
}
