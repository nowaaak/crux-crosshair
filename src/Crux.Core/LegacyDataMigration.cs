namespace Crux.Core;

public static class LegacyDataMigration
{
    private const string StagingSuffix = ".migrating";

    public static bool Migrate(string legacyDirectory, string targetDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legacyDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);

        if (Directory.Exists(targetDirectory) || !Directory.Exists(legacyDirectory))
        {
            return false;
        }

        var stagingDirectory = targetDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + StagingSuffix;
        if (Directory.Exists(stagingDirectory))
        {
            Directory.Delete(stagingDirectory, recursive: true);
        }

        CopyDirectory(legacyDirectory, stagingDirectory);
        Directory.Move(stagingDirectory, targetDirectory);
        return true;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        foreach (var directory in Directory.EnumerateDirectories(source))
        {
            CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
        }
    }
}
