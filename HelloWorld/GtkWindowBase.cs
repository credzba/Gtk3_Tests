// GtkWindowBase.cs
using System;
using System.IO;
using System.Reflection;
using Gtk;
using GLib;

public abstract class GtkWindowBase
{
    protected Window window;
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

        Builder builder = new Builder();

        // Try to load embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream(embeddedResourceName);

        if (resourceStream != null)
        {
            tempUiPath = Path.Combine(Path.GetTempPath(), uiFile);
            using (var file = File.OpenWrite(tempUiPath))
                resourceStream.CopyTo(file);

            builder.AddFromFile(tempUiPath);
        }
        else if (File.Exists(uiFile))
        {
            builder.AddFromFile(uiFile);
        }
        else
        {
            Console.Error.WriteLine($"ERROR: UI file '{uiFile}' not found (not embedded, no external file).");
            Environment.Exit(1);
        }

        // Get the main window object
        window = (Window)builder.GetObject("main_window");

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
