// HotKeysTab.cs

using System;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs
{
    public class HotKeysTab : TabBase
    {
        public override string TabName   => "Hot Keys";
        protected override string UiFile  => "HotKeysTab.ui";

        private TreeView    _tree;
        private TreeStore   _treeStore;

        // Right-panel widgets
        private Label       _lblStatus;
        private Button      _btnEnable;
        private Button      _btnDisable;
        private Label       _lblOnOffKey;

        private Entry       _entMasterKey;
        private Button      _btnMasterSet;
        private Button      _btnMasterClear;

        private Entry       _entModifyKey;
        private CheckButton _chkPassKeyToUo;
        private Button      _btnModifySet;
        private Button      _btnModifyClear;

        protected override void OnPopulate(Builder builder)
        {
            _tree         = (TreeView)builder.GetObject("tv_hotkeys");
            _lblStatus    = (Label)builder.GetObject("lbl_status");
            _btnEnable    = (Button)builder.GetObject("btn_enable");
            _btnDisable   = (Button)builder.GetObject("btn_disable");
            _lblOnOffKey  = (Label)builder.GetObject("lbl_onoff_key");

            _entMasterKey   = (Entry)builder.GetObject("ent_master_key");
            _btnMasterSet   = (Button)builder.GetObject("btn_master_set");
            _btnMasterClear = (Button)builder.GetObject("btn_master_clear");

            _entModifyKey   = (Entry)builder.GetObject("ent_modify_key");
            _chkPassKeyToUo = (CheckButton)builder.GetObject("chk_pass_key_to_uo");
            _btnModifySet   = (Button)builder.GetObject("btn_modify_set");
            _btnModifyClear = (Button)builder.GetObject("btn_modify_clear");

            BuildTree();
            LoadValues();
            ConnectSignals();
        }

        private void BuildTree()
        {
            if (_tree == null) return;

            _treeStore = new TreeStore(typeof(string));
            _tree.Model = _treeStore;
            _tree.AppendColumn("HotKeys", new CellRendererText(), "text", 0);
            _tree.HeadersVisible = false;

            // Static categories matching the screenshot
            foreach (var cat in new[]
                { "General", "Actions", "Agents", "Combat", "Skills",
                  "Spells", "Target", "Script", "Virtue", "Macro" })
            {
                _treeStore.AppendValues(cat);
            }
            _tree.ExpandAll();
        }

        private void LoadValues()
        {
            var cfg = HotKeysConfig.Instance;
            cfg.Load();

            UpdateStatusLabel(cfg.Enabled);
            if (_entMasterKey   != null) _entMasterKey.Text   = cfg.MasterKey;
            if (_entModifyKey   != null) _entModifyKey.Text   = cfg.ModifyKey;
            if (_chkPassKeyToUo != null) _chkPassKeyToUo.Active = cfg.PassKeyToUo;
        }

        private void UpdateStatusLabel(bool enabled)
        {
            if (_lblStatus != null)
                _lblStatus.Text = "Status: " + (enabled ? "Enabled" : "Disabled");
        }

        private void SaveValues()
        {
            var cfg = HotKeysConfig.Instance;
            if (_chkPassKeyToUo != null) cfg.PassKeyToUo = _chkPassKeyToUo.Active;
            cfg.Save();
        }

        private void ConnectSignals()
        {
            if (_btnEnable  != null) _btnEnable.Clicked  += (o, e) => { HotKeysConfig.Instance.Enabled = true;  HotKeysConfig.Instance.Save(); UpdateStatusLabel(true); };
            if (_btnDisable != null) _btnDisable.Clicked += (o, e) => { HotKeysConfig.Instance.Enabled = false; HotKeysConfig.Instance.Save(); UpdateStatusLabel(false); };

            if (_btnMasterSet   != null) _btnMasterSet.Clicked   += (o, e) => { if (_entMasterKey != null) { HotKeysConfig.Instance.MasterKey = _entMasterKey.Text; HotKeysConfig.Instance.Save(); } };
            if (_btnMasterClear != null) _btnMasterClear.Clicked += (o, e) => { if (_entMasterKey != null) { _entMasterKey.Text = ""; HotKeysConfig.Instance.MasterKey = ""; HotKeysConfig.Instance.Save(); } };

            if (_btnModifySet   != null) _btnModifySet.Clicked   += (o, e) => { if (_entModifyKey != null) { HotKeysConfig.Instance.ModifyKey = _entModifyKey.Text; HotKeysConfig.Instance.Save(); } };
            if (_btnModifyClear != null) _btnModifyClear.Clicked += (o, e) => { if (_entModifyKey != null) { _entModifyKey.Text = ""; HotKeysConfig.Instance.ModifyKey = ""; HotKeysConfig.Instance.Save(); } };

            if (_chkPassKeyToUo != null) _chkPassKeyToUo.Toggled += (o, e) => SaveValues();
        }
    }
}
