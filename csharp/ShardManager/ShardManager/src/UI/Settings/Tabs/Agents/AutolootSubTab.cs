// AutolootSubTab.cs
using System;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Agents
{
    public class AutolootSubTab : TabBase
    {
        public override string TabName   => "Autoloot";
        protected override string UiFile  => "AutolootSubTab.ui";

        private ComboBoxText _cboList;
        private Button       _btnAdd;
        private Button       _btnRemove;
        private Button       _btnClone;
        private CheckButton  _chkAllowLootHidden;
        private Entry        _entBagSerial;
        private Button       _btnSetBag;
        private CheckButton  _chkEnableAutoloot;
        private CheckButton  _chkNoOpenCorpse;
        private CheckButton  _chkAutostartOnLogin;
        private TreeView     _tvItems;
        private ListStore    _itemsStore;
        private Entry        _entDelay;
        private Button       _btnAddItem;
        private Entry        _entMaxRange;
        private Button       _btnEditProps;
        private Entry        _entAutolootLog;

        protected override void OnPopulate(Builder builder)
        {
            _cboList           = (ComboBoxText)builder.GetObject("cbo_autoloot_list");
            _btnAdd            = (Button)builder.GetObject("btn_autoloot_add");
            _btnRemove         = (Button)builder.GetObject("btn_autoloot_remove");
            _btnClone          = (Button)builder.GetObject("btn_autoloot_clone");
            _chkAllowLootHidden= (CheckButton)builder.GetObject("chk_allow_loot_hidden");
            _entBagSerial      = (Entry)builder.GetObject("ent_bag_serial");
            _btnSetBag         = (Button)builder.GetObject("btn_set_bag");
            _chkEnableAutoloot = (CheckButton)builder.GetObject("chk_enable_autoloot");
            _chkNoOpenCorpse   = (CheckButton)builder.GetObject("chk_no_open_corpse");
            _chkAutostartOnLogin = (CheckButton)builder.GetObject("chk_autostart_on_login");
            _tvItems           = (TreeView)builder.GetObject("tv_autoloot_items");
            _entDelay          = (Entry)builder.GetObject("ent_autoloot_delay");
            _btnAddItem        = (Button)builder.GetObject("btn_autoloot_add_item");
            _entMaxRange       = (Entry)builder.GetObject("ent_autoloot_max_range");
            _btnEditProps      = (Button)builder.GetObject("btn_autoloot_edit_props");
            _entAutolootLog    = (Entry)builder.GetObject("ent_autoloot_log");

            BuildItemsTree();
            LoadData();
            ConnectSignals();
        }

        private void BuildItemsTree()
        {
            if (_tvItems == null) return;
            // Columns: X (toggle), Item Name, Graphics, Color, Bag
            _itemsStore = new ListStore(typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string));
            _tvItems.Model = _itemsStore;

            var toggle = new CellRendererToggle();
            toggle.Toggled += (o, e) => {
                var path = new TreePath(e.Path); if (path != null)
                {
                    _itemsStore.GetIter(out TreeIter iter, path);
                    _itemsStore.SetValue(iter, 0, !(bool)_itemsStore.GetValue(iter, 0));
                }
            };
            _tvItems.AppendColumn("X",         toggle,                "active", 0);
            _tvItems.AppendColumn("Item Name",  new CellRendererText(), "text",   1);
            _tvItems.AppendColumn("Graphics",   new CellRendererText(), "text",   2);
            _tvItems.AppendColumn("Color",      new CellRendererText(), "text",   3);
            _tvItems.AppendColumn("Bag",        new CellRendererText(), "text",   4);
        }

        private void LoadData()
        {
            AgentsConfig.Instance.Load();
            RefreshListCombo();
        }

        private void RefreshListCombo()
        {
            if (_cboList == null) return;
            _cboList.RemoveAll();
            foreach (var lst in AgentsConfig.Instance.AutolootLists)
                _cboList.AppendText(lst.Name);
            int idx = AgentsConfig.Instance.ActiveAutolootIdx;
            _cboList.Active = (idx < AgentsConfig.Instance.AutolootLists.Count) ? idx : -1;
            LoadListDetails();
        }

        private AutolootList CurrentList()
        {
            if (_cboList == null) return null;
            int idx = _cboList.Active;
            var lists = AgentsConfig.Instance.AutolootLists;
            return (idx >= 0 && idx < lists.Count) ? lists[idx] : null;
        }

        private void LoadListDetails()
        {
            var lst = CurrentList();
            if (lst == null) { _itemsStore?.Clear(); return; }

            if (_chkEnableAutoloot  != null) _chkEnableAutoloot.Active  = lst.Enabled;
            if (_chkNoOpenCorpse    != null) _chkNoOpenCorpse.Active    = lst.NoOpenCorpse;
            if (_chkAutostartOnLogin!= null) _chkAutostartOnLogin.Active = lst.AutostartOnLogin;
            if (_chkAllowLootHidden != null) _chkAllowLootHidden.Active = lst.AllowLootingWhileHidden;
            if (_entBagSerial       != null) _entBagSerial.Text         = lst.BagSerial;
            if (_entDelay           != null) _entDelay.Text             = lst.DelayMs.ToString();
            if (_entMaxRange        != null) _entMaxRange.Text          = lst.MaxRange.ToString();

            if (_itemsStore != null)
            {
                _itemsStore.Clear();
                foreach (var item in lst.Items)
                    _itemsStore.AppendValues(item.Enabled, item.Name, item.Graphics, item.Color, item.Bag);
            }
        }

        private void SaveListDetails()
        {
            var lst = CurrentList();
            if (lst == null) return;
            if (_chkEnableAutoloot  != null) lst.Enabled              = _chkEnableAutoloot.Active;
            if (_chkNoOpenCorpse    != null) lst.NoOpenCorpse         = _chkNoOpenCorpse.Active;
            if (_chkAutostartOnLogin!= null) lst.AutostartOnLogin     = _chkAutostartOnLogin.Active;
            if (_chkAllowLootHidden != null) lst.AllowLootingWhileHidden = _chkAllowLootHidden.Active;
            if (_entBagSerial       != null) lst.BagSerial            = _entBagSerial.Text;
            if (_entDelay           != null && int.TryParse(_entDelay.Text, out int d)) lst.DelayMs = d;
            if (_entMaxRange        != null && int.TryParse(_entMaxRange.Text, out int r)) lst.MaxRange = r;
            AgentsConfig.Instance.Save();
        }

        private void ConnectSignals()
        {
            if (_cboList != null)
                _cboList.Changed += (o, e) => {
                    if (_cboList.Active >= 0) AgentsConfig.Instance.ActiveAutolootIdx = _cboList.Active;
                    LoadListDetails();
                };

            if (_btnAdd != null)
                _btnAdd.Clicked += (o, e) => {
                    var newList = new AutolootList { Name = "New List" + (AgentsConfig.Instance.AutolootLists.Count + 1) };
                    AgentsConfig.Instance.AutolootLists.Add(newList);
                    AgentsConfig.Instance.Save();
                    RefreshListCombo();
                };

            if (_btnRemove != null)
                _btnRemove.Clicked += (o, e) => {
                    int idx = _cboList?.Active ?? -1;
                    if (idx < 0) return;
                    AgentsConfig.Instance.AutolootLists.RemoveAt(idx);
                    AgentsConfig.Instance.Save();
                    RefreshListCombo();
                };

            if (_btnClone != null)
                _btnClone.Clicked += (o, e) => {
                    var src = CurrentList();
                    if (src == null) return;
                    var clone = new AutolootList { Name = src.Name + " (copy)", Enabled = src.Enabled,
                        NoOpenCorpse = src.NoOpenCorpse, AutostartOnLogin = src.AutostartOnLogin,
                        AllowLootingWhileHidden = src.AllowLootingWhileHidden, BagSerial = src.BagSerial,
                        DelayMs = src.DelayMs, MaxRange = src.MaxRange };
                    foreach (var item in src.Items)
                        clone.Items.Add(new AutolootItem { Enabled = item.Enabled, Name = item.Name,
                            Graphics = item.Graphics, Color = item.Color, Bag = item.Bag });
                    AgentsConfig.Instance.AutolootLists.Add(clone);
                    AgentsConfig.Instance.Save();
                    RefreshListCombo();
                };

            if (_chkEnableAutoloot  != null) _chkEnableAutoloot.Toggled  += (o, e) => SaveListDetails();
            if (_chkNoOpenCorpse    != null) _chkNoOpenCorpse.Toggled    += (o, e) => SaveListDetails();
            if (_chkAutostartOnLogin!= null) _chkAutostartOnLogin.Toggled += (o, e) => SaveListDetails();
            if (_chkAllowLootHidden != null) _chkAllowLootHidden.Toggled += (o, e) => SaveListDetails();

            if (_btnSetBag != null)
                _btnSetBag.Clicked += (o, e) => {
                    logger.Debug("Set Bag clicked — target selection not yet implemented");
                };

            if (_btnAddItem != null)
                _btnAddItem.Clicked += (o, e) => {
                    var lst = CurrentList();
                    if (lst == null) return;
                    lst.Items.Add(new AutolootItem { Name = "New Item" });
                    AgentsConfig.Instance.Save();
                    LoadListDetails();
                };

            if (_btnEditProps != null)
                _btnEditProps.Clicked += (o, e) => logger.Debug("Edit Props clicked");
        }
    }
}
