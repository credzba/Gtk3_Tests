// GtkWindowBase.cs
using System;
using System.IO;
using System.Reflection;
using Gtk;
using Gdk;
using GLib;

namespace UI
{
    public abstract class GtkWindowBase
    {
        protected Gtk.Window window;
        private string tempUiPath = null;

        /// <summary>
        /// Constructor loads the UI file (embedded resource or external file)
        /// </summary>
        /// <param name="uiFile">The filename of the UI</param>
        /// <param name="embeddedResourceName">The embedded resource name</param>
        /// <param name="windowId">The id of the GtkWindow object in the UI file (default: "main_window")</param>
        public GtkWindowBase(string uiFile, string embeddedResourceName, string windowId = "main_window")
        {
            Environment.SetEnvironmentVariable("GTK_DEBUG", "interactive");
            Environment.SetEnvironmentVariable("G_MESSAGES_DEBUG", "all");

            CustomMainLoop.Instance.Init();

            Builder builder = new Builder();

            // Resolve UI file path relative to the executable directory
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string exeUiPath = Path.Combine(exeDir, uiFile);

            // Try to load embedded resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(embeddedResourceName);

            if (File.Exists(exeUiPath))
            {
                builder.AddFromFile(exeUiPath);
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
            window = (Gtk.Window)builder.GetObject(windowId);

            // Let derived classes handle further widget setup and signals
            Setup(builder);
        }

        public void ApplyTheme(string themeFilePath)
        {
            if (File.Exists(themeFilePath))
            {
                var cssProvider = new CssProvider();
                Console.WriteLine($"Loading CSS from {themeFilePath}");
                cssProvider.LoadFromPath(themeFilePath);

                StyleContext.AddProviderForScreen(
                    Gdk.Screen.Default,
                    cssProvider,
                    600  // STYLE_PROVIDER_PRIORITY_APPLICATION
                );
            }
            else
            {
                Console.WriteLine($"CSS file not found: {themeFilePath}");
            }
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
            CustomMainLoop.Instance.Run();

            // Cleanup temp UI file if used
            if (tempUiPath != null && File.Exists(tempUiPath))
                File.Delete(tempUiPath);
        }
    }
}
