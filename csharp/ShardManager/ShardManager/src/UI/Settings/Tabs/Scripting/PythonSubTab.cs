// PythonSubTab.cs
using System;
using System.Collections.Generic;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Scripting
{
    public class PythonSubTab : TabBase
    {
        public override string TabName   => "Python";
        protected override string UiFile  => "PythonSubTab.ui";

        private TreeView     _tvScripts;
        private ListStore    _scriptsStore;

        // Script Info
        private CheckButton  _chkLoopMode;
        private CheckButton  _chkWaitBeforeInterrupt;
        private CheckButton  _chkAutoStartAtLogin;
        private CheckButton  _chkPreload;

        // Script Operation
        private Button       _btnAdd;
        private Button       _btnRemove;
        private Button       _btnDown;
        private Button       _btnTo;
        private Button       _btnUp;
        private Button       _btnNew;
        private Button       _btnEdit;
        private Button       _btnStop;
        private Button       _btnReload;
        private Button       _btnPlay;

        // Search + error options
        private Entry        _entSearch;
        private CheckButton  _chkLogScriptError;
        private CheckButton  _chkShowScriptErrorMessage;
        private CheckButton  _chkShowCharClientMessages;

        // Inspect
        private Button       _btnInspect;
        private Button       _btnInspectGump;

        protected override void OnPopulate(Builder builder)
        {
            _tvScripts               = (TreeView)builder.GetObject("tv_scripts");
            _chkLoopMode             = (CheckButton)builder.GetObject("chk_loop_mode");
            _chkWaitBeforeInterrupt  = (CheckButton)builder.GetObject("chk_wait_before_interrupt");
            _chkAutoStartAtLogin     = (CheckButton)builder.GetObject("chk_autostart_at_login");
            _chkPreload              = (CheckButton)builder.GetObject("chk_preload");
            _btnAdd                  = (Button)builder.GetObject("btn_script_add");
            _btnRemove               = (Button)builder.GetObject("btn_script_remove");
            _btnDown                 = (Button)builder.GetObject("btn_script_down");
            _btnTo                   = (Button)builder.GetObject("btn_script_to");
            _btnUp                   = (Button)builder.GetObject("btn_script_up");
            _btnNew                  = (Button)builder.GetObject("btn_script_new");
            _btnEdit                 = (Button)builder.GetObject("btn_script_edit");
            _btnStop                 = (Button)builder.GetObject("btn_script_stop");
            _btnReload               = (Button)builder.GetObject("btn_script_reload");
            _btnPlay                 = (Button)builder.GetObject("btn_script_play");
            _entSearch               = (Entry)builder.GetObject("ent_script_search");
            _chkLogScriptError       = (CheckButton)builder.GetObject("chk_log_script_error");
            _chkShowScriptErrorMessage = (CheckButton)builder.GetObject("chk_show_script_error_message");
            _chkShowCharClientMessages = (CheckButton)builder.GetObject("chk_show_char_client_messages");
            _btnInspect              = (Button)builder.GetObject("btn_inspect");
            _btnInspectGump          = (Button)builder.GetObject("btn_inspect_gump");

            BuildScriptsTree();
            LoadValues();
            ConnectSignals();
        }

        private void BuildScriptsTree()
        {
            if (_tvScripts == null) return;
            _scriptsStore = new ListStore(typeof(int), typeof(string), typeof(string));
            _tvScripts.Model = _scriptsStore;
            _tvScripts.AppendColumn("#",        new CellRendererText(), "text", 0);
            _tvScripts.AppendColumn("Filename", new CellRendererText(), "text", 1);
            _tvScripts.AppendColumn("Status",   new CellRendererText(), "text", 2);
        }

        private void LoadValues()
        {
            var cfg = ScriptingConfig.Instance;
            cfg.Load();
            if (_chkLoopMode              != null) _chkLoopMode.Active              = cfg.LoopMode;
            if (_chkWaitBeforeInterrupt   != null) _chkWaitBeforeInterrupt.Active   = cfg.WaitBeforeInterrupt;
            if (_chkAutoStartAtLogin      != null) _chkAutoStartAtLogin.Active      = cfg.AutoStartAtLogin;
            if (_chkPreload               != null) _chkPreload.Active               = cfg.Preload;
            if (_chkLogScriptError        != null) _chkLogScriptError.Active        = cfg.LogScriptError;
            if (_chkShowScriptErrorMessage!= null) _chkShowScriptErrorMessage.Active= cfg.ShowScriptErrorMessage;
            if (_chkShowCharClientMessages!= null) _chkShowCharClientMessages.Active= cfg.ShowCharClientMessages;
        }

        private void SaveValues()
        {
            var cfg = ScriptingConfig.Instance;
            if (_chkLoopMode              != null) cfg.LoopMode              = _chkLoopMode.Active;
            if (_chkWaitBeforeInterrupt   != null) cfg.WaitBeforeInterrupt   = _chkWaitBeforeInterrupt.Active;
            if (_chkAutoStartAtLogin      != null) cfg.AutoStartAtLogin      = _chkAutoStartAtLogin.Active;
            if (_chkPreload               != null) cfg.Preload               = _chkPreload.Active;
            if (_chkLogScriptError        != null) cfg.LogScriptError        = _chkLogScriptError.Active;
            if (_chkShowScriptErrorMessage!= null) cfg.ShowScriptErrorMessage= _chkShowScriptErrorMessage.Active;
            if (_chkShowCharClientMessages!= null) cfg.ShowCharClientMessages= _chkShowCharClientMessages.Active;
            cfg.Save();
        }

        private void ConnectSignals()
        {
            void S(CheckButton c) { if (c != null) c.Toggled += (o, e) => SaveValues(); }
            S(_chkLoopMode); S(_chkWaitBeforeInterrupt); S(_chkAutoStartAtLogin);
            S(_chkPreload); S(_chkLogScriptError); S(_chkShowScriptErrorMessage);
            S(_chkShowCharClientMessages);

            if (_btnInspect     != null) _btnInspect.Clicked     += (o, e) => logger.Debug("Inspect clicked");
            if (_btnInspectGump != null) _btnInspectGump.Clicked += (o, e) => logger.Debug("Inspect Gump clicked");
            if (_btnStop        != null) _btnStop.Clicked        += (o, e) => logger.Debug("Script Stop clicked");
            if (_btnReload      != null) _btnReload.Clicked      += (o, e) => logger.Debug("Script Reload clicked");
            if (_btnPlay        != null) _btnPlay.Clicked        += (o, e) => logger.Debug("Script Play clicked");
            if (_btnAdd         != null) _btnAdd.Clicked         += (o, e) => logger.Debug("Script Add clicked");
            if (_btnRemove      != null) _btnRemove.Clicked      += (o, e) => logger.Debug("Script Remove clicked");
            if (_btnNew         != null) _btnNew.Clicked         += (o, e) => logger.Debug("Script New clicked");
            if (_btnEdit        != null) _btnEdit.Clicked        += (o, e) => logger.Debug("Script Edit clicked");
            if (_btnUp          != null) _btnUp.Clicked          += (o, e) => logger.Debug("Script Up clicked");
            if (_btnDown        != null) _btnDown.Clicked        += (o, e) => logger.Debug("Script Down clicked");
            if (_btnTo          != null) _btnTo.Clicked          += (o, e) => logger.Debug("Script To clicked");
        }
    }
}
