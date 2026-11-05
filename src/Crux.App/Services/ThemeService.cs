using System.Windows;
using System.Windows.Threading;
using Crux.Core;
using Microsoft.Win32;

namespace Crux.App.Services;

internal sealed class ThemeService : IDisposable
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";
    private const string PaletteMarker = "Palette.";

    private static readonly Uri DarkPalette = new("pack://application:,,,/Crux;component/Themes/Palette.Dark.xaml");
    private static readonly Uri LightPalette = new("pack://application:,,,/Crux;component/Themes/Palette.Light.xaml");

    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
    private AppTheme preference = AppTheme.Dark;
    private bool followingSystem;

    public event EventHandler? Changed;

    public bool IsDark { get; private set; } = true;

    public void Apply(AppTheme theme)
    {
        preference = theme;
        FollowSystem(theme == AppTheme.System);
        Refresh();
    }

    public void Dispose() => FollowSystem(false);

    private void FollowSystem(bool follow)
    {
        if (follow == followingSystem)
        {
            return;
        }

        followingSystem = follow;
        if (follow)
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }
        else
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color)
        {
            dispatcher.BeginInvoke(Refresh);
        }
    }

    private void Refresh()
    {
        var dark = preference switch
        {
            AppTheme.Light => false,
            AppTheme.System => !SystemUsesLightTheme(),
            _ => true,
        };

        if (dark == IsDark)
        {
            return;
        }

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var index = dictionaries.ToList().FindIndex(dictionary =>
            dictionary.Source?.OriginalString.Contains(PaletteMarker, StringComparison.Ordinal) == true);
        if (index < 0)
        {
            throw new InvalidOperationException("The palette dictionary is missing from the application resources.");
        }

        dictionaries[index] = new ResourceDictionary { Source = dark ? DarkPalette : LightPalette };
        IsDark = dark;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool SystemUsesLightTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
        return key?.GetValue(AppsUseLightThemeValue) is int value && value != 0;
    }
}
