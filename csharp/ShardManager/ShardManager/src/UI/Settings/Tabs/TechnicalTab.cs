// TechnicalTab.cs
// Top-level Technical tab.  Contains a sub-notebook with lazy sub-tabs.

using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using UI.Settings;
using UI.Settings.Tabs.Technical;

namespace UI.Settings.Tabs
{
    public class TechnicalTab : TabBase
    {
        public override string TabName  => "Technical";
        protected override string UiFile => "TechnicalTab.ui";

        // Sub-tab instances
        private readonly List<TabBase> _subTabs = new List<TabBase>
        {
            new RazorSettingsSubTab(),
            new HelpStatusSubTab()
        };
        private readonly HashSet<int> _subLoaded = new HashSet<int>();

        private Notebook _subNotebook;

        protected override void OnPopulate(Builder builder)
        {
            _subNotebook = (Notebook)builder.GetObject("tech_subnotebook");
            if (_subNotebook == null)
            {
                logger.Error("TechnicalTab: 'tech_subnotebook' not found in UI.");
                return;
            }

            foreach (var sub in _subTabs)
            {
                var page  = sub.ContainerWidget;
                var label = new Label(sub.TabName) { Visible = true };
                _subNotebook.AppendPage(page, label);
            }

            _subNotebook.SwitchPage += OnSubSwitchPage;

            // Populate the Razor Settings sub-tab right away (it's the first / visible one)
            PopulateSubTab(0);

            // Wire the widgets that live directly in TechnicalTab.ui
            BindMainWidgets(builder);
        }

        // ── Sub-tab lazy loading ───────────────────────────────────────────

        private void OnSubSwitchPage(object sender, SwitchPageArgs args)
        {
            PopulateSubTab((int)args.PageNum);
        }

        private void PopulateSubTab(int index)
        {
            if (_subLoaded.Contains(index)) return;
            _subLoaded.Add(index);
            _subTabs[index].EnsurePopulated();
        }

        // ── Direct widgets on TechnicalTab.ui ─────────────────────────────
        // (The sub-notebook IS the content; individual fields live inside
        //  RazorSettingsSubTab.  Nothing extra to bind here for now.)

        private void BindMainWidgets(Builder builder)
        {
            // Placeholder — expand when fields outside the sub-notebook are added.
        }
    }
}
