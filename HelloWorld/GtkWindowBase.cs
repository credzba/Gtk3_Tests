// GtkWindowBase.cs
using System;
using System.IO;
using System.Reflection;
using Gtk;
using Gdk;
using GLib;

public abstract class GtkWindowBase
{
    protected Gtk.Window window;
    private string tempUiPath = null;

    /// <summary>
    /// Constructor loads the UI file (embedded resource or external file)
    /// </summary>
    /// <param name="uiFile">The filename of the UI</param>
    /// <param name="embeddedResourceName">The embedded resource name</param>
    public GtkWindowBase(string uiFile, string embeddedResourceName)
    {
        // Ensure GTK initialized
        Gtk.Application.Init();

        // Load theme
        const uint STYLE_PROVIDER_PRIORITY_APPLICATION = 600;
        var cssProvider = new CssProvider();
        var themeFile = System.IO.File.Exists("dark.css") ? "dark.css" : "light.css";
        if (System.IO.File.Exists(themeFile))
        {
            cssProvider.LoadFromPath(themeFile);
            StyleContext.AddProviderForScreen(
                Gdk.Screen.Default,
                cssProvider,
                STYLE_PROVIDER_PRIORITY_APPLICATION
            );
        }
        // Platform-specific theme on Windows
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            Gtk.Settings.Default.ThemeName = "win32";
        }

        Builder builder = new Builder();

        // Try to load embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream(embeddedResourceName);

        if (File.Exists(uiFile))
        {
            builder.AddFromFile(uiFile);
        }
        else if (resourceStream != null)
        {
            tempUiPath = Path.Combine(Path.GetTempPath(), uiFile);
            using (var file = File.OpenWrite(tempUiPath))
                resourceStream.CopyTo(file);
            builder.AddFromFile(tempUiPath);
        }
        else
        {
            Console.Error.WriteLine($"ERROR: UI file '{uiFile}' not found (not embedded, no external file).");
            Environment.Exit(1);
        }

        // Get the main window object
        window = (Gtk.Window)builder.GetObject("main_window");

        // Let derived classes handle further widget setup and signals
        Setup(builder);
    }

    /// <summary>
    /// Derived classes implement this to setup widget references and signals
    /// </summary>
    protected abstract void Setup(Builder builder);

    /// <summary>
    /// Show the window and run the GTK main loop
    /// </summary>
    public void Run()
    {
        window.ShowAll();
        Gtk.Application.Run();

        // Cleanup temp UI file if used
        if (tempUiPath != null && File.Exists(tempUiPath))
            File.Delete(tempUiPath);
    }
}
