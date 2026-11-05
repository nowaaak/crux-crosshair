using Crux.Core;

namespace Crux.Core.Tests;

public sealed class LegacyDataMigrationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "CruxTests", Guid.NewGuid().ToString("N"));

    private string LegacyDirectory => Path.Combine(root, "NdCrosshair");

    private string TargetDirectory => Path.Combine(root, "CruxCrosshair");

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Migrate_CopiesConfigAndImages_WhenOnlyLegacyExists()
    {
        Directory.CreateDirectory(Path.Combine(LegacyDirectory, "images"));
        File.WriteAllText(Path.Combine(LegacyDirectory, "config.json"), "{\"legacy\":true}");
        File.WriteAllBytes(Path.Combine(LegacyDirectory, "images", "a.png"), [1, 2, 3]);

        Assert.True(LegacyDataMigration.Migrate(LegacyDirectory, TargetDirectory));

        Assert.Equal("{\"legacy\":true}", File.ReadAllText(Path.Combine(TargetDirectory, "config.json")));
        Assert.Equal([1, 2, 3], File.ReadAllBytes(Path.Combine(TargetDirectory, "images", "a.png")));
        Assert.True(File.Exists(Path.Combine(LegacyDirectory, "config.json")));
        Assert.False(Directory.Exists(TargetDirectory + ".migrating"));
    }

    [Fact]
    public void Migrate_DoesNothing_WhenTargetExists()
    {
        Directory.CreateDirectory(LegacyDirectory);
        File.WriteAllText(Path.Combine(LegacyDirectory, "config.json"), "legacy");
        Directory.CreateDirectory(TargetDirectory);
        File.WriteAllText(Path.Combine(TargetDirectory, "config.json"), "current");

        Assert.False(LegacyDataMigration.Migrate(LegacyDirectory, TargetDirectory));

        Assert.Equal("current", File.ReadAllText(Path.Combine(TargetDirectory, "config.json")));
    }

    [Fact]
    public void Migrate_DoesNothing_WhenLegacyIsMissing()
    {
        Assert.False(LegacyDataMigration.Migrate(LegacyDirectory, TargetDirectory));

        Assert.False(Directory.Exists(TargetDirectory));
    }

    [Fact]
    public void Migrate_ReplacesLeftoverStaging_FromInterruptedRun()
    {
        Directory.CreateDirectory(LegacyDirectory);
        File.WriteAllText(Path.Combine(LegacyDirectory, "config.json"), "legacy");
        Directory.CreateDirectory(TargetDirectory + ".migrating");
        File.WriteAllText(Path.Combine(TargetDirectory + ".migrating", "partial.tmp"), "partial");

        Assert.True(LegacyDataMigration.Migrate(LegacyDirectory, TargetDirectory));

        Assert.Equal("legacy", File.ReadAllText(Path.Combine(TargetDirectory, "config.json")));
        Assert.False(File.Exists(Path.Combine(TargetDirectory, "partial.tmp")));
    }
}
