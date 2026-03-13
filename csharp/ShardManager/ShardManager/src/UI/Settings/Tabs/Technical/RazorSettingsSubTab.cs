// RazorSettingsSubTab.cs
// "Razor Settings" sub-tab inside Technical.

using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NLog;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Technical
{
    public class RazorSettingsSubTab : TabBase
    {
        public override string TabName   => "Razor Settings";
        protected override string UiFile  => "RazorSettingsSubTab.ui";

        // ── Widget fields ──────────────────────────────────────────────────
        private CheckButton _chkSmartCpu;
        private CheckButton _chkRememberPwd;
        private CheckButton _chkShowLauncher;
        private CheckButton _chkSmartAlwaysOnTop;
        private CheckButton _chkForceGameSize;
        private Entry       _entGameWidth;
        private Entry       _entGameHeight;
        private RadioButton _rdoTaskbar;
        private RadioButton _rdoSystemTray;
        private CheckButton _chkRemoteControl;
        private ComboBoxText _cboClientPriority;

        // Profiles area
        private ComboBoxText _cboProfiles;
        private Button      _btnAdd;
        private Button      _btnDelete;
        private Button      _btnRename;
        private Button      _btnClone;
        private Label       _lblLinkedTo;
        private Button      _btnLink;
        private Button      _btnUnlink;

        // Opacity
        private Label  _lblOpacity;
        private Scale  _scaleOpacity;

        // Changelog
        private Button _btnChangelog;

        protected override void OnPopulate(Builder builder)
        {
            // General checkboxes
            _chkSmartCpu         = (CheckButton)builder.GetObject("chk_smart_cpu");
            _chkRememberPwd      = (CheckButton)builder.GetObject("chk_remember_pwd");
            _chkShowLauncher     = (CheckButton)builder.GetObject("chk_show_launcher");
            _chkSmartAlwaysOnTop = (CheckButton)builder.GetObject("chk_smart_always_on_top");
            _chkForceGameSize    = (CheckButton)builder.GetObject("chk_force_game_size");
            _entGameWidth        = (Entry)builder.GetObject("ent_game_width");
            _entGameHeight       = (Entry)builder.GetObject("ent_game_height");
            _rdoTaskbar          = (RadioButton)builder.GetObject("rdo_taskbar");
            _rdoSystemTray       = (RadioButton)builder.GetObject("rdo_system_tray");
            _chkRemoteControl    = (CheckButton)builder.GetObject("chk_remote_control");
            _cboClientPriority   = (ComboBoxText)builder.GetObject("cbo_client_priority");

            // Profiles
            _cboProfiles = (ComboBoxText)builder.GetObject("cbo_profiles");
            _btnAdd      = (Button)builder.GetObject("btn_profile_add");
            _btnDelete   = (Button)builder.GetObject("btn_profile_delete");
            _btnRename   = (Button)builder.GetObject("btn_profile_rename");
            _btnClone    = (Button)builder.GetObject("btn_profile_clone");
            _lblLinkedTo = (Label)builder.GetObject("lbl_linked_to");
            _btnLink     = (Button)builder.GetObject("btn_profile_link");
            _btnUnlink   = (Button)builder.GetObject("btn_profile_unlink");

            // Opacity
            _lblOpacity   = (Label)builder.GetObject("lbl_opacity");
            _scaleOpacity = (Scale)builder.GetObject("scale_opacity");

            // Changelog
            _btnChangelog = (Button)builder.GetObject("btn_changelog");

            LoadValues();
            ConnectSignals();
        }

        // ── Data ───────────────────────────────────────────────────────────

        private void LoadValues()
        {
            var cfg = TechnicalConfig.Instance;
            cfg.Load();

            if (_chkSmartCpu         != null) _chkSmartCpu.Active         = cfg.UseSmartCpuReduction;
            if (_chkRememberPwd      != null) _chkRememberPwd.Active      = cfg.RememberPasswords;
            if (_chkShowLauncher     != null) _chkShowLauncher.Active     = cfg.ShowLauncherWindow;
            if (_chkSmartAlwaysOnTop != null) _chkSmartAlwaysOnTop.Active = cfg.UseSmartAlwaysOnTop;
            if (_chkForceGameSize    != null) _chkForceGameSize.Active    = cfg.ForceGameSize;
            if (_entGameWidth        != null) _entGameWidth.Text           = cfg.GameSizeWidth.ToString();
            if (_entGameHeight       != null) _entGameHeight.Text          = cfg.GameSizeHeight.ToString();
            if (_chkRemoteControl    != null) _chkRemoteControl.Active    = cfg.EnableRemoteControl;

            if (_rdoTaskbar    != null) _rdoTaskbar.Active    = (cfg.ShowIn == "Taskbar");
            if (_rdoSystemTray != null) _rdoSystemTray.Active = (cfg.ShowIn == "SystemTray");

            PopulateClientPriority(cfg.DefaultClientPriority);
            PopulateProfiles();

            if (_scaleOpacity != null)
            {
                _scaleOpacity.Value = cfg.Opacity;
                UpdateOpacityLabel(cfg.Opacity);
            }
        }

        private void PopulateClientPriority(string current)
        {
            if (_cboClientPriority == null) return;
            _cboClientPriority.RemoveAll();
            foreach (var p in new[] { "Normal", "AboveNormal", "High", "BelowNormal", "Idle" })
                _cboClientPriority.AppendText(p);
            SetComboText(_cboClientPriority, current);
        }

        private void PopulateProfiles()
        {
            if (_cboProfiles == null) return;
            _cboProfiles.RemoveAll();
            var pm = ProfileManager.Instance;
            foreach (var name in pm.GetProfiles())
                _cboProfiles.AppendText(name);

            // Select active profile
            var profiles = pm.GetProfiles().ToList();
            int idx = profiles.IndexOf(pm.CurrentProfile);
            _cboProfiles.Active = idx >= 0 ? idx : 0;
        }

        private void SaveValues()
        {
            var cfg = TechnicalConfig.Instance;
            if (_chkSmartCpu         != null) cfg.UseSmartCpuReduction  = _chkSmartCpu.Active;
            if (_chkRememberPwd      != null) cfg.RememberPasswords      = _chkRememberPwd.Active;
            if (_chkShowLauncher     != null) cfg.ShowLauncherWindow     = _chkShowLauncher.Active;
            if (_chkSmartAlwaysOnTop != null) cfg.UseSmartAlwaysOnTop    = _chkSmartAlwaysOnTop.Active;
            if (_chkForceGameSize    != null) cfg.ForceGameSize           = _chkForceGameSize.Active;
            if (_entGameWidth        != null && int.TryParse(_entGameWidth.Text, out int w))   cfg.GameSizeWidth  = w;
            if (_entGameHeight       != null && int.TryParse(_entGameHeight.Text, out int h))  cfg.GameSizeHeight = h;
            if (_chkRemoteControl    != null) cfg.EnableRemoteControl     = _chkRemoteControl.Active;
            if (_cboClientPriority   != null) cfg.DefaultClientPriority   = _cboClientPriority.ActiveText ?? "Normal";
            if (_rdoSystemTray       != null) cfg.ShowIn                  = _rdoSystemTray.Active ? "SystemTray" : "Taskbar";
            if (_scaleOpacity        != null) cfg.Opacity                 = (int)_scaleOpacity.Value;
            cfg.Save();
        }

        // ── Signals ────────────────────────────────────────────────────────

        private void ConnectSignals()
        {
            // Auto-save on change
            if (_chkSmartCpu         != null) _chkSmartCpu.Toggled         += (o, e) => SaveValues();
            if (_chkRememberPwd      != null) _chkRememberPwd.Toggled      += (o, e) => SaveValues();
            if (_chkShowLauncher     != null) _chkShowLauncher.Toggled     += (o, e) => SaveValues();
            if (_chkSmartAlwaysOnTop != null) _chkSmartAlwaysOnTop.Toggled += (o, e) => SaveValues();
            if (_chkForceGameSize    != null) _chkForceGameSize.Toggled    += (o, e) => SaveValues();
            if (_chkRemoteControl    != null) _chkRemoteControl.Toggled    += (o, e) => SaveValues();
            if (_rdoTaskbar          != null) _rdoTaskbar.Toggled          += (o, e) => { if (_rdoTaskbar.Active) SaveValues(); };
            if (_rdoSystemTray       != null) _rdoSystemTray.Toggled       += (o, e) => { if (_rdoSystemTray.Active) SaveValues(); };
            if (_cboClientPriority   != null) _cboClientPriority.Changed   += (o, e) => SaveValues();

            if (_scaleOpacity != null)
                _scaleOpacity.ValueChanged += (o, e) => {
                    UpdateOpacityLabel((int)_scaleOpacity.Value);
                    SaveValues();
                };

            if (_cboProfiles != null)
                _cboProfiles.Changed += OnProfileSelected;

            if (_btnAdd     != null) _btnAdd.Clicked    += OnProfileAdd;
            if (_btnDelete  != null) _btnDelete.Clicked  += OnProfileDelete;
            if (_btnRename  != null) _btnRename.Clicked  += OnProfileRename;
            if (_btnClone   != null) _btnClone.Clicked   += OnProfileClone;
            if (_btnLink    != null) _btnLink.Clicked    += OnProfileLink;
            if (_btnUnlink  != null) _btnUnlink.Clicked  += OnProfileUnlink;

            if (_btnChangelog != null)
                _btnChangelog.Clicked += (o, e) => {
                    // Placeholder — show changelog dialog when implemented
                    logger.Debug("Changelog clicked");
                };
        }

        private void UpdateOpacityLabel(int value)
        {
            if (_lblOpacity != null)
                _lblOpacity.Text = $"Opacity: {value}%";
        }

        // ── Profile button handlers ────────────────────────────────────────

        private void OnProfileSelected(object sender, EventArgs e)
        {
            string name = _cboProfiles?.ActiveText;
            if (string.IsNullOrEmpty(name)) return;
            try
            {
                ProfileManager.Instance.SetCurrentProfile(name);
                LoadValues();   // reload all tab settings for new profile
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void OnProfileAdd(object sender, EventArgs e)
        {
            string name = PromptText("New Profile", "Enter profile name:");
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                ProfileManager.Instance.CreateProfile(name);
                ProfileManager.Instance.SetCurrentProfile(name);
                PopulateProfiles();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void OnProfileDelete(object sender, EventArgs e)
        {
            string name = _cboProfiles?.ActiveText;
            if (string.IsNullOrEmpty(name)) return;
            if (!Confirm($"Delete profile '{name}'?")) return;
            try
            {
                ProfileManager.Instance.DeleteProfile(name);
                PopulateProfiles();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void OnProfileRename(object sender, EventArgs e)
        {
            string old = _cboProfiles?.ActiveText;
            if (string.IsNullOrEmpty(old)) return;
            string newName = PromptText("Rename Profile", $"Rename '{old}' to:");
            if (string.IsNullOrWhiteSpace(newName)) return;
            try
            {
                ProfileManager.Instance.RenameProfile(old, newName);
                PopulateProfiles();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void OnProfileClone(object sender, EventArgs e)
        {
            string src = _cboProfiles?.ActiveText;
            if (string.IsNullOrEmpty(src)) return;
            string newName = PromptText("Clone Profile", $"Clone '{src}' as:");
            if (string.IsNullOrWhiteSpace(newName)) return;
            try
            {
                ProfileManager.Instance.CloneProfile(src, newName);
                PopulateProfiles();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void OnProfileLink(object sender, EventArgs e)
        {
            // Link to login/character — placeholder until Server.shards integration is wired
            logger.Debug("Profile Link clicked (not yet implemented)");
        }

        private void OnProfileUnlink(object sender, EventArgs e)
        {
            logger.Debug("Profile UnLink clicked (not yet implemented)");
        }

        // ── Dialogs ────────────────────────────────────────────────────────

        private string PromptText(string title, string prompt)
        {
            var dlg = new Dialog(title, null, DialogFlags.Modal,
                "OK", ResponseType.Ok, "Cancel", ResponseType.Cancel);
            dlg.ContentArea.Add(new Label(prompt) { Visible = true });
            var entry = new Entry { Visible = true };
            dlg.ContentArea.Add(entry);
            dlg.ShowAll();
            string result = dlg.Run() == (int)ResponseType.Ok ? entry.Text : null;
            dlg.Destroy();
            return result;
        }

        private bool Confirm(string message)
        {
            var dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                ButtonsType.YesNo, message);
            bool yes = dlg.Run() == (int)ResponseType.Yes;
            dlg.Destroy();
            return yes;
        }

        private void ShowError(string message)
        {
            var dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Error,
                ButtonsType.Ok, message);
            dlg.Run();
            dlg.Destroy();
        }
    }
}
