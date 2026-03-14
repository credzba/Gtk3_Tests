// SettingsWindow.cs
// Main settings dialog.  Hosts a Notebook whose pages are populated lazily.

using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using UI.Settings.Tabs;

namespace UI.Settings
{
    public class SettingsWindow : GtkWindowBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Notebook _notebook;

        // One entry per top-level tab, in display order
        private readonly List<TabBase> _tabs = new List<TabBase>
        {
            new OptionsTab(),
            new FiltersTab(),
            new ScriptingTab(),
            new MacrosTab(),
            new AgentsTab(),
            new ToolbarsTab(),
            new SkillsTab(),
            new HotKeysTab(),
            new AdvancedTab(),
            new TechnicalTab()
        };

        // Track which pages have been populated
        private readonly HashSet<int> _loaded = new HashSet<int>();

        // ── Construction ───────────────────────────────────────────────────

        public SettingsWindow()
            : base("SettingsWindow.ui", "ShardManager.SettingsWindow.ui", "wnd_settings")
        {
        }

        // ── GtkWindowBase ──────────────────────────────────────────────────

        protected override void Setup(Builder builder)
        {
            _notebook = (Notebook)builder.GetObject("notebook_settings");

            window.DeleteEvent += (o, e) => { CustomMainLoop.Instance.Quit(); e.RetVal = true; };

            foreach (var tab in _tabs)
            {
                var page  = tab.ContainerWidget;
                var label = new Label(tab.TabName) { Visible = true };
                _notebook.AppendPage(page, label);
            }

            _notebook.SwitchPage += OnSwitchPage;

            // Populate the first visible tab immediately
            PopulateTab(0);
        }

        // ── Lazy population ────────────────────────────────────────────────

        private void OnSwitchPage(object sender, SwitchPageArgs args)
        {
            PopulateTab((int)args.PageNum);
        }

        private void PopulateTab(int index)
        {
            if (_loaded.Contains(index)) return;
            _loaded.Add(index);
            _tabs[index].EnsurePopulated();
        }

        // ── Public API ─────────────────────────────────────────────────────

        public new void Show()
        {
            window.ShowAll();
            window.Present();
        }

        public void Hide()
        {
            window.Hide();
        }
    }
}
