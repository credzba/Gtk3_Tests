// TabBase.cs
// Base class for all settings tab panels.
// Subclasses provide a root Widget (a Box) that is embedded into the Settings notebook.
// Content is built lazily on first activation.

using System;
using System.IO;
using System.Reflection;
using Gtk;
using NLog;

namespace UI.Settings
{
    public abstract class TabBase
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Box   _container;   // always present; content packed in on first activation
        private bool  _populated = false;

        // ── Abstract surface ───────────────────────────────────────────────

        /// <summary>Display name used as the notebook tab label.</summary>
        public abstract string TabName { get; }

        /// <summary>Filename (no path) of the .ui file for this tab's content.</summary>
        protected abstract string UiFile { get; }

        /// <summary>
        /// Embedded resource name, e.g. "ShardManager.TechnicalTab.ui".
        /// Defaults to "ShardManager.{UiFile}".
        /// </summary>
        protected virtual string EmbeddedResourceName =>
            "ShardManager." + UiFile;

        /// <summary>
        /// Called after the builder has loaded the UI.
        /// Subclasses bind widget fields and connect signals here.
        /// </summary>
        protected abstract void OnPopulate(Builder builder);

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the container widget.  Always the same object — notebook pages hold this.
        /// Content is empty until EnsurePopulated() is called.
        /// </summary>
        public Widget ContainerWidget
        {
            get
            {
                if (_container == null)
                    _container = new Box(Orientation.Vertical, 0) { Visible = true };
                return _container;
            }
        }

        /// <summary>Called by SettingsWindow when the tab page is first switched to.</summary>
        public void EnsurePopulated()
        {
            if (_populated) return;
            _populated = true;

            try
            {
                var builder = LoadBuilder();
                var root    = (Widget)builder.GetObject("tab_root");
                if (root == null)
                    throw new Exception($"UI file '{UiFile}' has no object with id 'tab_root'.");

                // Strip the GtkWindow wrapper added for Glade compatibility.
                // The ui file sets visible=True so GTK realizes the window immediately;
                // we must Hide() it first so its GDK window is freed before we
                // Unparent tab_root.  We intentionally do NOT Destroy() the wrapper
                // — the builder manages its lifetime and will drop its ref when GCd.
                if (root.Parent is Gtk.Window wrapperWin)
                {
                    wrapperWin.Hide();
                    root.Unparent();
                }

                _container.PackStart(root, true, true, 0);
                _container.ShowAll();

                OnPopulate(builder);
                logger.Debug("Tab '{0}' populated.", TabName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to populate tab '{0}'", TabName);
                // Show error label so the window still opens
                var lbl = new Label($"Error loading {TabName}: {ex.Message}") { Visible = true };
                _container.PackStart(lbl, true, true, 0);
                _container.ShowAll();
            }
        }

        // ── UI file loading ────────────────────────────────────────────────


        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>Sets a ComboBoxText active by matching text; falls back to index 0.</summary>
        protected static void SetComboText(ComboBoxText combo, string text)
        {
            if (combo == null) return;
            var model = combo.Model;
            TreeIter iter;
            if (model == null || !model.GetIterFirst(out iter)) { combo.Active = 0; return; }
            int i = 0;
            do
            {
                if ((string)model.GetValue(iter, 0) == text) { combo.Active = i; return; }
                i++;
            } while (model.IterNext(ref iter));
            combo.Active = 0;
        }

        private Builder LoadBuilder()
        {
            var builder = new Builder();

            // 1. External file next to the executable
            string exeDir   = AppDomain.CurrentDomain.BaseDirectory;
            string external = Path.Combine(exeDir, UiFile);
            if (File.Exists(external))
            {
                builder.AddFromFile(external);
                return builder;
            }

            // 2. Embedded resource
            var asm    = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(EmbeddedResourceName);
            if (stream != null)
            {
                string tmp = Path.Combine(Path.GetTempPath(), UiFile);
                using (var f = File.OpenWrite(tmp))
                    stream.CopyTo(f);
                builder.AddFromFile(tmp);
                File.Delete(tmp);
                return builder;
            }

            throw new FileNotFoundException(
                $"UI file '{UiFile}' not found as external file or embedded resource.");
        }
    }
}
