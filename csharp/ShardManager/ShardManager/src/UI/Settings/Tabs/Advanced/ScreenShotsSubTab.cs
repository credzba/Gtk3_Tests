// ScreenShotsSubTab.cs  — Screen Shots sub-tab inside Advanced

using System;
using System.IO;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Advanced
{
    public class ScreenShotsSubTab : TabBase
    {
        public override string TabName   => "Screen Shots";
        protected override string UiFile  => "ScreenShotsSubTab.ui";

        private Entry       _entDirectory;
        private Button      _btnTakeNow;
        private ComboBoxText _cboFormat;
        private RadioButton _rdoUoOnly;
        private RadioButton _rdoFullScreen;
        private CheckButton _chkTimestamp;
        private CheckButton _chkAutoDeath;

        protected override void OnPopulate(Builder builder)
        {
            _entDirectory  = (Entry)builder.GetObject("ent_screenshot_dir");
            _btnTakeNow    = (Button)builder.GetObject("btn_take_now");
            _cboFormat     = (ComboBoxText)builder.GetObject("cbo_image_format");
            _rdoUoOnly     = (RadioButton)builder.GetObject("rdo_uo_only");
            _rdoFullScreen = (RadioButton)builder.GetObject("rdo_full_screen");
            _chkTimestamp  = (CheckButton)builder.GetObject("chk_include_timestamp");
            _chkAutoDeath  = (CheckButton)builder.GetObject("chk_auto_death");

            PopulateFormats();
            LoadValues();
            ConnectSignals();
        }

        private void PopulateFormats()
        {
            if (_cboFormat == null) return;
            _cboFormat.RemoveAll();
            foreach (var fmt in new[] { "jpg", "png", "bmp" })
                _cboFormat.AppendText(fmt);
        }

        private void LoadValues()
        {
            var cfg = AdvancedConfig.Instance;
            cfg.Load();

            if (_entDirectory  != null) _entDirectory.Text          = cfg.ScreenShotDirectory;
            if (_rdoUoOnly     != null) _rdoUoOnly.Active           = cfg.UoOnly;
            if (_rdoFullScreen != null) _rdoFullScreen.Active       = !cfg.UoOnly;
            if (_chkTimestamp  != null) _chkTimestamp.Active        = cfg.IncludeTimestamp;
            if (_chkAutoDeath  != null) _chkAutoDeath.Active        = cfg.AutoDeathScreenCapture;

            if (_cboFormat != null)
            {
                SetComboText(_cboFormat, cfg.ImageFormat);
            }
        }

        private void SaveValues()
        {
            var cfg = AdvancedConfig.Instance;
            if (_entDirectory  != null) cfg.ScreenShotDirectory    = _entDirectory.Text;
            if (_cboFormat     != null) cfg.ImageFormat             = _cboFormat.ActiveText ?? "jpg";
            if (_rdoUoOnly     != null) cfg.UoOnly                  = _rdoUoOnly.Active;
            if (_chkTimestamp  != null) cfg.IncludeTimestamp        = _chkTimestamp.Active;
            if (_chkAutoDeath  != null) cfg.AutoDeathScreenCapture  = _chkAutoDeath.Active;
            cfg.Save();
        }

        private void ConnectSignals()
        {
            if (_entDirectory  != null) _entDirectory.Changed   += (o, e) => SaveValues();
            if (_cboFormat     != null) _cboFormat.Changed      += (o, e) => SaveValues();
            if (_rdoUoOnly     != null) _rdoUoOnly.Toggled      += (o, e) => { if (_rdoUoOnly.Active) SaveValues(); };
            if (_rdoFullScreen != null) _rdoFullScreen.Toggled  += (o, e) => { if (_rdoFullScreen.Active) SaveValues(); };
            if (_chkTimestamp  != null) _chkTimestamp.Toggled   += (o, e) => SaveValues();
            if (_chkAutoDeath  != null) _chkAutoDeath.Toggled   += (o, e) => SaveValues();

            if (_btnTakeNow != null)
                _btnTakeNow.Clicked += (o, e) => logger.Debug("Take Screen Shot Now clicked (not yet implemented)");
        }
    }
}
