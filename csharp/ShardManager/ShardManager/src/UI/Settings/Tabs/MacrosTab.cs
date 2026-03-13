// MacrosTab.cs
using System;
using System.Collections.Generic;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs
{
    public class MacrosTab : TabBase
    {
        public override string TabName   => "Macros";
        protected override string UiFile  => "MacrosTab.ui";

        private ListBox      _lstMacros;
        private Button       _btnNew;
        private Button       _btnDelete;
        private Button       _btnSave;
        private Button       _btnRecord;
        private Button       _btnPlay;
        private Button       _btnStop;
        private Button       _btnStopRecord;
        private CheckButton  _chkLoop;
        private Entry        _entHotKey;
        private Button       _btnHotKeySet;
        private Button       _btnHotKeyClear;
        private CheckButton  _chkPassKeyToUo;
        private TreeView     _tvActions;
        private ListStore    _actionsStore;
        private Label        _lblStatus;

        private MacroEntry _currentMacro;

        protected override void OnPopulate(Builder builder)
        {
            _lstMacros      = (ListBox)builder.GetObject("lst_macros");
            _btnNew         = (Button)builder.GetObject("btn_macro_new");
            _btnDelete      = (Button)builder.GetObject("btn_macro_delete");
            _btnSave        = (Button)builder.GetObject("btn_macro_save");
            _btnRecord      = (Button)builder.GetObject("btn_record");
            _btnPlay        = (Button)builder.GetObject("btn_play");
            _btnStop        = (Button)builder.GetObject("btn_stop");
            _btnStopRecord  = (Button)builder.GetObject("btn_stop_record");
            _chkLoop        = (CheckButton)builder.GetObject("chk_macro_loop");
            _entHotKey      = (Entry)builder.GetObject("ent_macro_hotkey");
            _btnHotKeySet   = (Button)builder.GetObject("btn_hotkey_set");
            _btnHotKeyClear = (Button)builder.GetObject("btn_hotkey_clear");
            _chkPassKeyToUo = (CheckButton)builder.GetObject("chk_macro_pass_key");
            _tvActions      = (TreeView)builder.GetObject("tv_macro_actions");
            _lblStatus      = (Label)builder.GetObject("lbl_macro_status");

            BuildActionsTree();
            LoadMacroList();
            ConnectSignals();
        }

        private void BuildActionsTree()
        {
            if (_tvActions == null) return;
            _actionsStore = new ListStore(typeof(string), typeof(string));
            _tvActions.Model = _actionsStore;
            _tvActions.AppendColumn("Action",  new CellRendererText(), "text", 0);
            _tvActions.AppendColumn("Details", new CellRendererText(), "text", 1);
        }

        private void LoadMacroList()
        {
            MacrosConfig.Instance.Load();
            RefreshMacroList();
        }

        private void RefreshMacroList()
        {
            if (_lstMacros == null) return;
            foreach (var child in _lstMacros.Children)
                _lstMacros.Remove(child);

            foreach (var macro in MacrosConfig.Instance.Macros)
            {
                var row = new ListBoxRow { Visible = true };
                row.Add(new Label(macro.Name) { Visible = true, Xalign = 0 });
                _lstMacros.Add(row);
            }
        }

        private void LoadMacroDetails(MacroEntry macro)
        {
            _currentMacro = macro;
            if (_chkLoop        != null) _chkLoop.Active       = macro.Loop;
            if (_entHotKey      != null) _entHotKey.Text       = macro.HotKey;
            if (_chkPassKeyToUo != null) _chkPassKeyToUo.Active = macro.PassKeyToUo;

            if (_actionsStore != null)
            {
                _actionsStore.Clear();
                foreach (var act in macro.Actions)
                    _actionsStore.AppendValues(act.Action, act.Details);
            }
        }

        private void ConnectSignals()
        {
            if (_lstMacros != null)
                _lstMacros.RowSelected += (o, e) => {
                    var row = e.Row;
                    if (row == null) return;
                    int idx = row.Index;
                    var macros = MacrosConfig.Instance.Macros;
                    if (idx >= 0 && idx < macros.Count)
                        LoadMacroDetails(macros[idx]);
                };

            if (_btnNew != null)
                _btnNew.Clicked += (o, e) => {
                    var entry = new MacroEntry { Name = "NewMacro" + (MacrosConfig.Instance.Macros.Count + 1) };
                    MacrosConfig.Instance.Macros.Add(entry);
                    MacrosConfig.Instance.Save();
                    RefreshMacroList();
                };

            if (_btnDelete != null)
                _btnDelete.Clicked += (o, e) => {
                    var row = _lstMacros?.SelectedRow;
                    if (row == null) return;
                    int idx = row.Index;
                    var macros = MacrosConfig.Instance.Macros;
                    if (idx >= 0 && idx < macros.Count)
                    {
                        macros.RemoveAt(idx);
                        MacrosConfig.Instance.Save();
                        _currentMacro = null;
                        RefreshMacroList();
                    }
                };

            if (_btnSave != null)
                _btnSave.Clicked += (o, e) => {
                    SaveCurrentMacro();
                    MacrosConfig.Instance.Save();
                    if (_lblStatus != null) _lblStatus.Text = "Saved.";
                };

            if (_chkLoop        != null) _chkLoop.Toggled       += (o, e) => SaveCurrentMacro();
            if (_chkPassKeyToUo != null) _chkPassKeyToUo.Toggled += (o, e) => SaveCurrentMacro();

            if (_btnHotKeySet != null)
                _btnHotKeySet.Clicked += (o, e) => { if (_currentMacro != null && _entHotKey != null) { _currentMacro.HotKey = _entHotKey.Text; MacrosConfig.Instance.Save(); } };

            if (_btnHotKeyClear != null)
                _btnHotKeyClear.Clicked += (o, e) => { if (_currentMacro != null) { _currentMacro.HotKey = ""; if (_entHotKey != null) _entHotKey.Text = ""; MacrosConfig.Instance.Save(); } };

            if (_btnRecord     != null) _btnRecord.Clicked     += (o, e) => { if (_lblStatus != null) _lblStatus.Text = "Recording..."; };
            if (_btnStop       != null) _btnStop.Clicked       += (o, e) => { if (_lblStatus != null) _lblStatus.Text = "Ready"; };
            if (_btnStopRecord != null) _btnStopRecord.Clicked += (o, e) => { if (_lblStatus != null) _lblStatus.Text = "Ready"; };
            if (_btnPlay       != null) _btnPlay.Clicked       += (o, e) => { if (_lblStatus != null) _lblStatus.Text = "Playing..."; };
        }

        private void SaveCurrentMacro()
        {
            if (_currentMacro == null) return;
            if (_chkLoop        != null) _currentMacro.Loop       = _chkLoop.Active;
            if (_chkPassKeyToUo != null) _currentMacro.PassKeyToUo = _chkPassKeyToUo.Active;
        }
    }
}
