using Serilog;

namespace Moongate.Server.Bootstrap;

/// <summary>
/// Copies bundled data assets into the configured data directory when missing.
/// Existing files are never overwritten.
/// </summary>
public static class DataAssetsBootstrapper
{
    public static int EnsureDataAssets(string sourceDataDirectory, string destinationDataDirectory, ILogger logger)
    {
        if (!Directory.Exists(sourceDataDirectory))
        {
            logger.Warning("Data assets source directory not found: {SourceDataDirectory}", sourceDataDirectory);

            return 0;
        }

        Directory.CreateDirectory(destinationDataDirectory);

        var copiedFiles = 0;
        var sourceFiles = Directory.GetFiles(sourceDataDirectory, "*", SearchOption.AllDirectories);

        foreach (var sourceFile in sourceFiles)
        {
            var relativePath = Path.GetRelativePath(sourceDataDirectory, sourceFile);
            var destinationFile = Path.Combine(destinationDataDirectory, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            if (File.Exists(destinationFile))
            {
                continue;
            }

            File.Copy(sourceFile, destinationFile);
            copiedFiles++;
        }

        logger.Information(
            "Data assets synchronization completed. Copied {CopiedFiles} missing files into {DestinationDataDirectory}",
            copiedFiles,
            destinationDataDirectory
        );

        return copiedFiles;
    }
}
