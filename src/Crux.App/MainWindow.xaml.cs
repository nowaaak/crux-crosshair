using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crux.App.Controls;
using Crux.App.Localization;
using Crux.App.Native;
using Crux.App.Services;

namespace Crux.App;

internal partial class MainWindow : Window
{
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;
    private const int DarkBorderColor = 0x00262626;
    private const int LightBorderColor = 0x00DEDEDE;
    private const double MaximizedInset = 7;

    private readonly MainViewModel viewModel;
    private readonly ThemeService theme;
    private TaskCompletionSource<bool>? dialogResult;

    public MainWindow(MainViewModel viewModel, ThemeService theme)
    {
        this.viewModel = viewModel;
        this.theme = theme;
        DataContext = viewModel;
        InitializeComponent();
        Title = AppIdentity.DisplayName;
        Icon = BitmapFrame.Create(AppIdentity.IconUri);
        StateChanged += OnStateChanged;
        PreviewKeyDown += OnWindowPreviewKeyDown;
        theme.Changed += OnThemeChanged;
        Closed += (_, _) => theme.Changed -= OnThemeChanged;
    }

    public Task<bool> ConfirmAsync(string title, string message, string confirmText, bool destructive)
    {
        dialogResult?.TrySetResult(false);
        dialogResult = new TaskCompletionSource<bool>();

        DialogTitle.Text = title;
        DialogMessage.Text = message;
        DialogConfirm.Content = confirmText;
        DialogConfirm.Style = (Style)FindResource(destructive ? "DangerButton" : "AccentButton");
        DialogLayer.Visibility = Visibility.Visible;
        DialogCancel.Focus();
        return dialogResult.Task;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var handle = new WindowInteropHelper(this).Handle;
        Dwmapi.SetInt(handle, DWMWA_WINDOW_CORNER_PREFERENCE, DWMWCP_ROUND);
        ApplyFrameTheme();
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ApplyFrameTheme();

    private void ApplyFrameTheme()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == 0)
        {
            return;
        }

        Dwmapi.SetInt(handle, Dwmapi.DWMWA_USE_IMMERSIVE_DARK_MODE, theme.IsDark ? 1 : 0);
        Dwmapi.SetInt(handle, Dwmapi.DWMWA_BORDER_COLOR, theme.IsDark ? DarkBorderColor : LightBorderColor);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && viewModel.MinimizeToTray)
        {
            Hide();
            return;
        }

        var maximized = WindowState == WindowState.Maximized;
        RootGrid.Margin = maximized ? new Thickness(MaximizedInset) : new Thickness(0);
        MaximizeButton.Content = new IconElement
        {
            Data = (Geometry)FindResource(maximized ? "Icon.Copy" : "Icon.Square"),
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = maximized ? new ScaleTransform(-1, 1) : Transform.Identity,
        };
        MaximizeButton.SetValue(
            System.Windows.Automation.AutomationProperties.NameProperty,
            Loc.T(maximized ? "WindowRestore" : "WindowMaximize"));
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximize(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnDialogCancel(object sender, RoutedEventArgs e) => CloseDialog(false);

    private void OnDialogConfirm(object sender, RoutedEventArgs e) => CloseDialog(true);

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DialogLayer.Visibility == Visibility.Visible && e.Key == Key.Escape)
        {
            e.Handled = true;
            CloseDialog(false);
        }
    }

    private void CloseDialog(bool confirmed)
    {
        DialogLayer.Visibility = Visibility.Collapsed;
        var result = dialogResult;
        dialogResult = null;
        result?.TrySetResult(confirmed);
    }
}
