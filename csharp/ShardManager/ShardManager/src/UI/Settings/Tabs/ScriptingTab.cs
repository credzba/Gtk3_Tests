// ScriptingTab.cs — hosts Python, UOS, C# sub-tabs
using System;
using System.Collections.Generic;
using Gtk;
using UI.Settings;
using UI.Settings.Tabs.Scripting;

namespace UI.Settings.Tabs
{
    public class ScriptingTab : TabBase
    {
        public override string TabName   => "Scripting";
        protected override string UiFile  => "ScriptingTab.ui";

        private readonly List<TabBase> _subTabs = new List<TabBase>
        {
            new PythonSubTab(),
            new UosSubTab(),
            new CSharpSubTab()
        };
        private readonly HashSet<int> _subLoaded = new HashSet<int>();
        private Notebook _subNotebook;

        protected override void OnPopulate(Builder builder)
        {
            _subNotebook = (Notebook)builder.GetObject("scripting_subnotebook");
            if (_subNotebook == null) { logger.Error("ScriptingTab: 'scripting_subnotebook' not found."); return; }

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
