// VirtualSubTab.cs
using System;
using System.Collections.Generic;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs.Filters
{
    public class VirtualSubTab : TabBase
    {
        public override string TabName   => "Virtual";
        protected override string UiFile  => "VirtualSubTab.ui";

        private CheckButton  _chkRepeatingSys;
        private CheckButton  _chkSnooping;
        private CheckButton  _chkPoison;
        private CheckButton  _chkMonster;

        // More Filters — one CheckButton per entry, built dynamically
        private Box          _boxMoreFilters;
        private readonly List<(string Key, CheckButton Chk)> _moreFilterChks = new List<(string, CheckButton)>();

        // Journal filter
        private Entry        _entJournalFilterText;
        private Button       _btnJournalAdd;
        private ListBox      _lstJournalFilters;

        protected override void OnPopulate(Builder builder)
        {
            _chkRepeatingSys       = (CheckButton)builder.GetObject("chk_repeating_sys");
            _chkSnooping           = (CheckButton)builder.GetObject("chk_snooping");
            _chkPoison             = (CheckButton)builder.GetObject("chk_poison");
            _chkMonster            = (CheckButton)builder.GetObject("chk_monster");
            _boxMoreFilters        = (Box)builder.GetObject("box_more_filters");
            _entJournalFilterText  = (Entry)builder.GetObject("ent_journal_filter_text");
            _btnJournalAdd         = (Button)builder.GetObject("btn_journal_add");
            _lstJournalFilters     = (ListBox)builder.GetObject("lst_journal_filters");

            BuildMoreFilters();
            LoadValues();
            ConnectSignals();
        }

        private void BuildMoreFilters()
        {
            if (_boxMoreFilters == null) return;
            foreach (var child in _boxMoreFilters.Children)
                _boxMoreFilters.Remove(child);

            _moreFilterChks.Clear();
            foreach (var key in FiltersConfig.Instance.MoreFilters.Keys)
            {
                var chk = new CheckButton(key) { Visible = true };
                _boxMoreFilters.PackStart(chk, false, false, 0);
                _moreFilterChks.Add((key, chk));
                chk.Toggled += (o, e) => SaveValues();
            }
        }

        private void LoadValues()
        {
            var cfg = FiltersConfig.Instance;
            cfg.Load();

            if (_chkRepeatingSys != null) _chkRepeatingSys.Active = cfg.RepeatingSysMessages;
            if (_chkSnooping     != null) _chkSnooping.Active     = cfg.SnoopingMessages;
            if (_chkPoison       != null) _chkPoison.Active       = cfg.PoisonMessages;
            if (_chkMonster      != null) _chkMonster.Active      = cfg.MonsterMessages;

            foreach (var (key, chk) in _moreFilterChks)
                if (cfg.MoreFilters.TryGetValue(key, out bool val))
                    chk.Active = val;

            if (_entJournalFilterText != null) _entJournalFilterText.Text = cfg.JournalFilterText;
            RefreshJournalList();
        }

        private void SaveValues()
        {
            var cfg = FiltersConfig.Instance;
            if (_chkRepeatingSys != null) cfg.RepeatingSysMessages = _chkRepeatingSys.Active;
            if (_chkSnooping     != null) cfg.SnoopingMessages     = _chkSnooping.Active;
            if (_chkPoison       != null) cfg.PoisonMessages       = _chkPoison.Active;
            if (_chkMonster      != null) cfg.MonsterMessages      = _chkMonster.Active;

            foreach (var (key, chk) in _moreFilterChks)
                cfg.MoreFilters[key] = chk.Active;

            if (_entJournalFilterText != null) cfg.JournalFilterText = _entJournalFilterText.Text;
            cfg.Save();
        }

        private void RefreshJournalList()
        {
            if (_lstJournalFilters == null) return;
            foreach (var child in _lstJournalFilters.Children)
                _lstJournalFilters.Remove(child);
            foreach (var f in FiltersConfig.Instance.JournalFilters)
            {
                var row = new ListBoxRow { Visible = true };
                row.Add(new Label(f) { Visible = true, Xalign = 0 });
                _lstJournalFilters.Add(row);
            }
        }

        private void ConnectSignals()
        {
            void S(CheckButton c) { if (c != null) c.Toggled += (o, e) => SaveValues(); }
            S(_chkRepeatingSys); S(_chkSnooping); S(_chkPoison); S(_chkMonster);

            if (_entJournalFilterText != null)
                _entJournalFilterText.Changed += (o, e) => SaveValues();

            if (_btnJournalAdd != null)
                _btnJournalAdd.Clicked += (o, e) => {
                    var text = _entJournalFilterText?.Text?.Trim();
                    if (string.IsNullOrEmpty(text)) return;
                    FiltersConfig.Instance.JournalFilters.Add(text);
                    FiltersConfig.Instance.Save();
                    RefreshJournalList();
                    if (_entJournalFilterText != null) _entJournalFilterText.Text = "";
                };
        }
    }
}
