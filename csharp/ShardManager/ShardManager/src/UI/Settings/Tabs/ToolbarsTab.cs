// ToolbarsTab.cs — hosts Counter/Stat Bar and Spell Grid sub-tabs

using System;
using System.Collections.Generic;
using Gtk;
using UI.Settings;
using UI.Settings.Tabs.Toolbars;

namespace UI.Settings.Tabs
{
    public class ToolbarsTab : TabBase
    {
        public override string TabName   => "Toolbars";
        protected override string UiFile  => "ToolbarsTab.ui";

        private readonly List<TabBase> _subTabs = new List<TabBase>
        {
            new CounterStatBarSubTab(),
            new SpellGridSubTab()
        };
        private readonly HashSet<int> _subLoaded = new HashSet<int>();
        private Notebook _subNotebook;

        protected override void OnPopulate(Builder builder)
        {
            _subNotebook = (Notebook)builder.GetObject("toolbars_subnotebook");
            if (_subNotebook == null) { logger.Error("ToolbarsTab: 'toolbars_subnotebook' not found."); return; }

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
