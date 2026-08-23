using Avalonia;
using System;
using System.IO;

namespace WerkPilot.Desktop;

internal static class Program
{
    private static readonly string DiagnosticFile =
        Path.Combine(AppContext.BaseDirectory, "WerkPilot-Startdiagnose.txt");

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            File.WriteAllText(
                DiagnosticFile,
                $"WerkPilot Startdiagnose{Environment.NewLine}"
                + $"Zeit: {DateTimeOffset.Now:O}{Environment.NewLine}"
                + $"Basisordner: {AppContext.BaseDirectory}{Environment.NewLine}"
                + $"Betriebssystem: {Environment.OSVersion}{Environment.NewLine}"
                + $"Runtime: {Environment.Version}{Environment.NewLine}"
                + $"Status: Programmstart erreicht.{Environment.NewLine}");

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            File.AppendAllText(
                DiagnosticFile,
                $"Status: Avalonia wurde regulär beendet.{Environment.NewLine}");

            return 0;
        }
        catch (Exception exception)
        {
            try
            {
                File.AppendAllText(
                    DiagnosticFile,
                    $"{Environment.NewLine}STATUS: STARTFEHLER{Environment.NewLine}"
                    + exception
                    + Environment.NewLine);
            }
            catch
            {
                // Diagnose darf den ursprünglichen Fehler nicht verdecken.
            }

            return 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
