// AdvancedTab.cs
// Top-level Advanced tab — hosts Screen Shots, Video Recorder, DPS Meter sub-tabs.

using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using UI.Settings;
using UI.Settings.Tabs.Advanced;

namespace UI.Settings.Tabs
{
    public class AdvancedTab : TabBase
    {
        public override string TabName   => "Advanced";
        protected override string UiFile  => "AdvancedTab.ui";

        private readonly List<TabBase> _subTabs = new List<TabBase>
        {
            new ScreenShotsSubTab(),
            new VideoRecorderSubTab(),
            new DpsMeterSubTab()
        };
        private readonly HashSet<int> _subLoaded = new HashSet<int>();
        private Notebook _subNotebook;

        protected override void OnPopulate(Builder builder)
        {
            _subNotebook = (Notebook)builder.GetObject("adv_subnotebook");
            if (_subNotebook == null) { logger.Error("AdvancedTab: 'adv_subnotebook' not found."); return; }

            foreach (var sub in _subTabs)
                _subNotebook.AppendPage(sub.ContainerWidget, new Label(sub.TabName) { Visible = true });

            _subNotebook.SwitchPage += (o, e) => PopulateSubTab((int)e.PageNum);
            PopulateSubTab(0);
        }

        private void PopulateSubTab(int index)
        {
            if (_subLoaded.Contains(index)) return;
            _subLoaded.Add(index);
            _subTabs[index].EnsurePopulated();
        }
    }
}
