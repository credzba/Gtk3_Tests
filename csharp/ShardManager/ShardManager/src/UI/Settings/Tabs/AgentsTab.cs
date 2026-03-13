// AgentsTab.cs — hosts Autoloot, Scavenger, Organizer, Vendor Buy/Sell, Dress/Arm,
//                Friends, Restock, Bandage Heal sub-tabs
using System;
using System.Collections.Generic;
using Gtk;
using UI.Settings;
using UI.Settings.Tabs.Agents;

namespace UI.Settings.Tabs
{
    public class AgentsTab : TabBase
    {
        public override string TabName   => "Agents";
        protected override string UiFile  => "AgentsTab.ui";

        private readonly List<TabBase> _subTabs = new List<TabBase>
        {
            new AutolootSubTab(),
            new ScavengerSubTab(),
            new OrganizerSubTab(),
            new VendorBuySubTab(),
            new VendorSellSubTab(),
            new DressArmSubTab(),
            new FriendsSubTab(),
            new RestockSubTab(),
            new BandageHealSubTab()
        };
        private readonly HashSet<int> _subLoaded = new HashSet<int>();
        private Notebook _subNotebook;

        protected override void OnPopulate(Builder builder)
        {
            _subNotebook = (Notebook)builder.GetObject("agents_subnotebook");
            if (_subNotebook == null) { logger.Error("AgentsTab: 'agents_subnotebook' not found."); return; }

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
