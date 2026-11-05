using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Crux.App.Localization;
using Crux.App.Services;
using Crux.Core;

namespace Crux.App;

public partial class App : Application
{
    private Mutex? instanceMutex;
    private EventWaitHandle? showSettingsEvent;
    private RegisteredWaitHandle? showSettingsWait;
    private AppController? controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Resources["AppDisplayName"] = AppIdentity.DisplayName;
        Resources["LogoBackgroundBrush"] = new SolidColorBrush(AppIdentity.LogoBackground);
        Resources["LogoForegroundBrush"] = new SolidColorBrush(AppIdentity.LogoForeground);

        instanceMutex = new Mutex(true, AppIdentity.InstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            SignalRunningInstance();
            instanceMutex.Dispose();
            instanceMutex = null;
            Shutdown();
            return;
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        System.Windows.Forms.Application.EnableVisualStyles();

        try
        {
            var migratedFromLegacy = LegacyDataMigration.Migrate(AppIdentity.LegacyDataDirectory, AppIdentity.DataDirectory);
            controller = new AppController(new ConfigStore(AppIdentity.ConfigFilePath), migratedFromLegacy);
            controller.Start(e.Args.Contains(AutostartService.TrayArgument, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception exception)
        {
            MessageBox.Show(Loc.Format("StartupFailed", exception.Message), AppIdentity.DisplayName, MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        showSettingsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, AppIdentity.ShowSettingsEventName);
        showSettingsWait = ThreadPool.RegisterWaitForSingleObject(
            showSettingsEvent,
            (_, _) => Dispatcher.BeginInvoke(() => controller?.ShowSettings()),
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        showSettingsWait?.Unregister(null);
        showSettingsEvent?.Dispose();
        controller?.Dispose();
        instanceMutex?.ReleaseMutex();
        instanceMutex?.Dispose();
        base.OnExit(e);
    }

    private static void SignalRunningInstance()
    {
        if (EventWaitHandle.TryOpenExisting(AppIdentity.ShowSettingsEventName, out var runningInstanceEvent))
        {
            using (runningInstanceEvent)
            {
                runningInstanceEvent.Set();
            }
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ToastService.ShowGlobal(Loc.Format("UnexpectedError", e.Exception.Message));
        e.Handled = true;
    }
}
