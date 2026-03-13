// SettingsWindow.cs
// Main settings dialog.  Hosts a Notebook whose pages are populated lazily.

using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using UI.Settings.Tabs;

namespace UI.Settings
{
    public class SettingsWindow
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Gtk.Window  _window;
        private Notebook    _notebook;

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
        {
            BuildWindow();
        }

        private void BuildWindow()
        {
            _window = new Gtk.Window("Settings")
            {
                DefaultWidth  = 680,
                DefaultHeight = 440,
                WindowPosition = WindowPosition.Center
            };
            _window.DeleteEvent += (o, e) => { CustomMainLoop.Instance.Quit(); e.RetVal = true; };

            _notebook = new Notebook { Visible = true };

            foreach (var tab in _tabs)
            {
                var page  = tab.ContainerWidget;
                var label = new Label(tab.TabName) { Visible = true };
                _notebook.AppendPage(page, label);
            }

            _notebook.SwitchPage += OnSwitchPage;

            _window.Add(_notebook);

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

        public void Show()
        {
            _window.ShowAll();
            _window.Present();
        }

        public void Hide()
        {
            _window.Hide();
        }
    }
}
