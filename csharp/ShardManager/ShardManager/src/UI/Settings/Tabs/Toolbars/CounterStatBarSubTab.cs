// CounterStatBarSubTab.cs

using System;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Toolbars
{
    public class CounterStatBarSubTab : TabBase
    {
        public override string TabName   => "Counter / Stat Bar";
        protected override string UiFile  => "CounterStatBarSubTab.ui";

        // General
        private Button      _btnOpen;
        private Button      _btnClose;
        private CheckButton _chkLockToolBar;
        private CheckButton _chkOpenOnLogin;
        private Label       _lblPosition;

        // Item Count
        private ComboBoxText _cboSlot;
        private Entry       _entName;
        private Entry       _entGraphics;
        private Entry       _entColor;
        private CheckButton _chkShowWarning;
        private Entry       _entWarning;
        private Button      _btnGetData;
        private Button      _btnClearSlot;

        // Layout
        private ComboBoxText _cboStyle;
        private ComboBoxText _cboSize;
        private Label        _lblSlots;
        private Button       _btnSlotsPlus;
        private Button       _btnSlotsMinus;
        private CheckButton  _chkShowHits;
        private CheckButton  _chkShowStamina;
        private CheckButton  _chkShowMana;
        private CheckButton  _chkShowWeight;
        private CheckButton  _chkShow;
        private CheckButton  _chkShowTithe;

        // Opacity
        private Scale  _scaleOpacity;
        private Label  _lblOpacity;

        protected override void OnPopulate(Builder builder)
        {
            _btnOpen        = (Button)builder.GetObject("btn_counter_open");
            _btnClose       = (Button)builder.GetObject("btn_counter_close");
            _chkLockToolBar = (CheckButton)builder.GetObject("chk_lock_toolbar");
            _chkOpenOnLogin = (CheckButton)builder.GetObject("chk_open_on_login");
            _lblPosition    = (Label)builder.GetObject("lbl_position");

            _cboSlot        = (ComboBoxText)builder.GetObject("cbo_slot");
            _entName        = (Entry)builder.GetObject("ent_slot_name");
            _entGraphics    = (Entry)builder.GetObject("ent_slot_graphics");
            _entColor       = (Entry)builder.GetObject("ent_slot_color");
            _chkShowWarning = (CheckButton)builder.GetObject("chk_show_warning");
            _entWarning     = (Entry)builder.GetObject("ent_warning");
            _btnGetData     = (Button)builder.GetObject("btn_get_data");
            _btnClearSlot   = (Button)builder.GetObject("btn_clear_slot");

            _cboStyle       = (ComboBoxText)builder.GetObject("cbo_layout_style");
            _cboSize        = (ComboBoxText)builder.GetObject("cbo_layout_size");
            _lblSlots       = (Label)builder.GetObject("lbl_slots");
            _btnSlotsPlus   = (Button)builder.GetObject("btn_slots_plus");
            _btnSlotsMinus  = (Button)builder.GetObject("btn_slots_minus");
            _chkShowHits    = (CheckButton)builder.GetObject("chk_show_hits");
            _chkShowStamina = (CheckButton)builder.GetObject("chk_show_stamina");
            _chkShowMana    = (CheckButton)builder.GetObject("chk_show_mana");
            _chkShowWeight  = (CheckButton)builder.GetObject("chk_show_weight");
            _chkShow        = (CheckButton)builder.GetObject("chk_show");
            _chkShowTithe   = (CheckButton)builder.GetObject("chk_show_tithe");

            _scaleOpacity   = (Scale)builder.GetObject("scale_counter_opacity");
            _lblOpacity     = (Label)builder.GetObject("lbl_counter_opacity");

            PopulateSlots();
            PopulateStyleSize();
            LoadValues();
            ConnectSignals();
        }

        private void PopulateSlots()
        {
            if (_cboSlot == null) return;
            _cboSlot.RemoveAll();
            var cfg = ToolbarsConfig.Instance;
            for (int i = 0; i < cfg.Slots; i++)
                _cboSlot.AppendText($"Slot {i}: Empty");
            _cboSlot.Active = 0;
        }

        private void PopulateStyleSize()
        {
            if (_cboStyle != null)
            {
                _cboStyle.RemoveAll();
                foreach (var s in new[] { "Vertical", "Horizontal" })
                    _cboStyle.AppendText(s);
            }
            if (_cboSize != null)
            {
                _cboSize.RemoveAll();
                foreach (var s in new[] { "Big", "Medium", "Small" })
                    _cboSize.AppendText(s);
            }
        }

        private void LoadValues()
        {
            var cfg = ToolbarsConfig.Instance;
            cfg.Load();

            if (_chkLockToolBar != null) _chkLockToolBar.Active = cfg.LockToolBar;
            if (_chkOpenOnLogin != null) _chkOpenOnLogin.Active = cfg.OpenOnLogin;
            if (_lblPosition    != null) _lblPosition.Text      = $"X: {cfg.PositionX} - Y:{cfg.PositionY}";
            if (_lblSlots       != null) _lblSlots.Text         = cfg.Slots.ToString();

            if (_cboStyle != null) SetComboText(_cboStyle, cfg.LayoutStyle);
            if (_cboSize  != null) SetComboText(_cboSize,  cfg.LayoutSize);

            if (_chkShowHits    != null) _chkShowHits.Active    = cfg.ShowHits;
            if (_chkShowStamina != null) _chkShowStamina.Active = cfg.ShowStamina;
            if (_chkShowMana    != null) _chkShowMana.Active    = cfg.ShowMana;
            if (_chkShowWeight  != null) _chkShowWeight.Active  = cfg.ShowWeight;
            if (_chkShow        != null) _chkShow.Active        = cfg.ShowBar;
            if (_chkShowTithe   != null) _chkShowTithe.Active   = cfg.ShowTithe;

            if (_scaleOpacity != null) { _scaleOpacity.Value = cfg.CounterBarOpacity; UpdateOpacityLabel(cfg.CounterBarOpacity); }
        }

        private void SaveValues()
        {
            var cfg = ToolbarsConfig.Instance;
            if (_chkLockToolBar != null) cfg.LockToolBar   = _chkLockToolBar.Active;
            if (_chkOpenOnLogin != null) cfg.OpenOnLogin   = _chkOpenOnLogin.Active;
            if (_cboStyle       != null) cfg.LayoutStyle   = _cboStyle.ActiveText ?? "Vertical";
            if (_cboSize        != null) cfg.LayoutSize    = _cboSize.ActiveText  ?? "Big";
            if (_chkShowHits    != null) cfg.ShowHits      = _chkShowHits.Active;
            if (_chkShowStamina != null) cfg.ShowStamina   = _chkShowStamina.Active;
            if (_chkShowMana    != null) cfg.ShowMana      = _chkShowMana.Active;
            if (_chkShowWeight  != null) cfg.ShowWeight    = _chkShowWeight.Active;
            if (_chkShow        != null) cfg.ShowBar       = _chkShow.Active;
            if (_chkShowTithe   != null) cfg.ShowTithe     = _chkShowTithe.Active;
            if (_scaleOpacity   != null) cfg.CounterBarOpacity = (int)_scaleOpacity.Value;
            cfg.Save();
        }

        private void UpdateOpacityLabel(int v)
        {
            if (_lblOpacity != null) _lblOpacity.Text = $"{v}%";
        }

        private void ConnectSignals()
        {
            if (_btnOpen        != null) _btnOpen.Clicked        += (o, e) => logger.Debug("Counter bar Open clicked");
            if (_btnClose       != null) _btnClose.Clicked       += (o, e) => logger.Debug("Counter bar Close clicked");
            if (_chkLockToolBar != null) _chkLockToolBar.Toggled += (o, e) => SaveValues();
            if (_chkOpenOnLogin != null) _chkOpenOnLogin.Toggled += (o, e) => SaveValues();

            if (_btnSlotsPlus != null)
                _btnSlotsPlus.Clicked += (o, e) => {
                    ToolbarsConfig.Instance.Slots = Math.Min(20, ToolbarsConfig.Instance.Slots + 1);
                    if (_lblSlots != null) _lblSlots.Text = ToolbarsConfig.Instance.Slots.ToString();
                    PopulateSlots();
                    ToolbarsConfig.Instance.Save();
                };
            if (_btnSlotsMinus != null)
                _btnSlotsMinus.Clicked += (o, e) => {
                    ToolbarsConfig.Instance.Slots = Math.Max(1, ToolbarsConfig.Instance.Slots - 1);
                    if (_lblSlots != null) _lblSlots.Text = ToolbarsConfig.Instance.Slots.ToString();
                    PopulateSlots();
                    ToolbarsConfig.Instance.Save();
                };

            if (_cboStyle       != null) _cboStyle.Changed       += (o, e) => SaveValues();
            if (_cboSize        != null) _cboSize.Changed        += (o, e) => SaveValues();
            if (_chkShowHits    != null) _chkShowHits.Toggled    += (o, e) => SaveValues();
            if (_chkShowStamina != null) _chkShowStamina.Toggled += (o, e) => SaveValues();
            if (_chkShowMana    != null) _chkShowMana.Toggled    += (o, e) => SaveValues();
            if (_chkShowWeight  != null) _chkShowWeight.Toggled  += (o, e) => SaveValues();
            if (_chkShow        != null) _chkShow.Toggled        += (o, e) => SaveValues();
            if (_chkShowTithe   != null) _chkShowTithe.Toggled   += (o, e) => SaveValues();

            if (_scaleOpacity != null)
                _scaleOpacity.ValueChanged += (o, e) => {
                    UpdateOpacityLabel((int)_scaleOpacity.Value);
                    SaveValues();
                };

            if (_btnGetData   != null) _btnGetData.Clicked   += (o, e) => logger.Debug("Get Data clicked");
            if (_btnClearSlot != null) _btnClearSlot.Clicked += (o, e) => {
                if (_entName     != null) _entName.Text     = "Empty";
                if (_entGraphics != null) _entGraphics.Text = "0x0000";
                if (_entColor    != null) _entColor.Text    = "0x0000";
                if (_entWarning  != null) _entWarning.Text  = "0";
            };
        }
    }
}
