using System.CommandLine;

namespace UAssetDiffTool.Cli;

internal static class OptionExtensions {

    public static Option<string?> ExistingOrCreateableFile(this Option<string?> opt) {
        opt.AddValidator(result => {
            var value = result.GetValueOrDefault<string?>();

            if (string.IsNullOrEmpty(value)) {
                return;
            }

            try {
                var dir = Path.GetDirectoryName(value);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
            }
            catch {
                result.ErrorMessage = $"Invalid path: {value}";
            }
        });

        return opt;
    }

    public static Argument<string> BothFilesOrBothDirectories(
            this Argument<string> thisArgument,
            Argument<string> otherArgument
    ) {
        thisArgument.AddValidator(result => {
            var pathA = result.GetValueForArgument(thisArgument);
            var pathB = result.GetValueForArgument(otherArgument);

            var bothIsFiles = File.Exists(pathA) && File.Exists(pathB);
            var bothIsDirectories = Directory.Exists(pathA) && Directory.Exists(pathB);

            if (!bothIsFiles && !bothIsDirectories) {
                result.ErrorMessage = "Both arguments must be either existing files or existing directories.";
            }
        });

        return thisArgument;
    }
}
