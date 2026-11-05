using System.IO;
using System.Windows.Media;

namespace Crux.App;

internal static class AppIdentity
{
#if DEBUG
    public const string Id = "CruxCrosshair.Dev";
    public const string LegacyId = "NdCrosshair.Dev";
    public const string DisplayName = "Crux Dev";
    public const string IconFile = "app-dev.ico";
    public static readonly Color LogoGradientStart = Color.FromRgb(74, 222, 128);
    public static readonly Color LogoGradientEnd = Color.FromRgb(21, 128, 61);
#else
    public const string Id = "CruxCrosshair";
    public const string LegacyId = "NdCrosshair";
    public const string DisplayName = "Crux";
    public const string IconFile = "app.ico";
    public static readonly Color LogoGradientStart = Color.FromRgb(96, 205, 255);
    public static readonly Color LogoGradientEnd = Color.FromRgb(37, 99, 235);
#endif


    public const string InstanceMutexName = @"Local\" + Id + ".Instance";
    public const string ShowSettingsEventName = @"Local\" + Id + ".ShowSettings";

    public static Uri IconUri => new($"pack://application:,,,/Assets/{IconFile}");

    public static string DataDirectory => DataDirectoryFor(Id);

    public static string LegacyDataDirectory => DataDirectoryFor(LegacyId);

    public static string ConfigFilePath => Path.Combine(DataDirectory, "config.json");

    public static bool IsLegacyInstallPresent => File.Exists(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LegacyId,
        "Update.exe"));

    private static string DataDirectoryFor(string id) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        id);
}
