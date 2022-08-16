﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Anotar.NLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milki.Extensions.Configuration;
using Milki.OsuPlayer.Audio;
using Milki.OsuPlayer.Configuration;
using Milki.OsuPlayer.Data;
using Milki.OsuPlayer.Services;
using Milki.OsuPlayer.Shared;
using Milki.OsuPlayer.Shared.Utils;
using Milki.OsuPlayer.Utils;
using Milki.OsuPlayer.Windows;
using Milki.OsuPlayer.Wpf;
using NLog;
using NLog.Config;

namespace Milki.OsuPlayer;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    [STAThread]
    internal static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        CreateApplication();
    }

    public new static App Current { get; private set; }
    public ServiceProvider ServiceProvider { get; private set; }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += Application_DispatcherUnhandledException;

        I18NUtil.LoadI18N();
        ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("InvariantCulture",
            typeof(InvariantCultureLayoutRendererWrapper));

        if (!LoadConfig())
        {
            Shutdown(2);
            return;
        }

        StyleUtilities.SetAlignment();
        Unosquare.FFME.Library.FFmpegDirectory = Path.Combine(AppSettings.Directories.ToolDir, "ffmpeg");

        BuildServices();
        await InitializeServicesAsync();

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private bool LoadConfig()
    {
        try
        {
            _ = AppSettings.Default;
            return true;
        }
        catch (Exception ex)
        {
            var ok = MsgDialog.WarnOkCancel("Error occurs while loading configuration. " +
                                            "Click 'OK' to override current configuration.",
                instruction: "Invalid configuration",
                title: "Osu Player",
                detail: "Exception message: " + ex.Message);
            if (!ok) return false;
            File.Delete("./AppSettings.yaml");
            _ = AppSettings.Default;
            return true;
        }
    }

    private void BuildServices()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>();
        services.AddScoped<BeatmapSyncService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<LyricsService>();
        services.AddSingleton<OsuFileScanningService>();
        services.AddSingleton<OsuPlayList>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private async ValueTask InitializeServicesAsync()
    {
        ServiceProvider.GetService<LyricsService>()!.ReloadLyricProvider();
        ServiceProvider.GetService<OsuPlayList>()!.Mode = AppSettings.Default.PlaySection.PlayListMode;

        await using var dbContext = ServiceProvider.GetService<ApplicationDbContext>()!;
        await dbContext.Database.MigrateAsync();
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Fatal(e.Exception, "DispatcherUnhandledException");

        var exceptionWindow = new ExceptionWindow(e.Exception, true);
        var exit = exceptionWindow.ShowDialog();
        e.Handled = exit != true;
        if (!e.Handled)
        {
            Environment.Exit(1);
        }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        AppSettings.Default?.Save();
        LogManager.Shutdown();
    }

    private static void CreateApplication()
    {
        var mutex = new Mutex(true, "Milki.OsuPlayer", out bool createNew);
        if (!createNew)
        {
            var process = Process
                .GetProcessesByName(Process.GetCurrentProcess().ProcessName)
                .FirstOrDefault(k => k.Id != Environment.ProcessId && k.MainWindowHandle != IntPtr.Zero);
            if (process == null) return;
            ProcessUtils.ShowWindow(process.MainWindowHandle, ProcessUtils.SW_SHOW);
            ProcessUtils.SetForegroundWindow(process.MainWindowHandle);
            return;
        }

        LogManager.LoadConfiguration("NLog.config");
        Sentry.SentryNLog.Init(LogManager.Configuration, options =>
        {
            options.HttpProxy = HttpClient.DefaultProxy;
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });

        try
        {
            LogTo.Info("Application started.", true);
            var app = new App();
            Current = app;
            app.InitializeComponent();
            app.Run();
        }
        finally
        {
            mutex.ReleaseMutex();
            LogTo.Info("Application stopped.", true);
        }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(ex, "UnhandledException");

            var exceptionWindow = new ExceptionWindow(ex, false);
            exceptionWindow.ShowDialog();
        }

        if (!e.IsTerminating)
        {
            return;
        }

        Environment.Exit(1);
    }
}