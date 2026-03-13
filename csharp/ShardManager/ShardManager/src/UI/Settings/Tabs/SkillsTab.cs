// SkillsTab.cs

using System;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs
{
    public class SkillsTab : TabBase
    {
        public override string TabName   => "Skills";
        protected override string UiFile  => "SkillsTab.ui";

        private TreeView    _tree;
        private ListStore   _store;

        private Button      _btnResetPlusMinus;
        private ComboBoxText _cboSetAllLocks;
        private Button      _btnCopySelected;
        private Button      _btnCopyAll;
        private CheckButton _chkDisplayChanges;
        private Label       _lblBaseTotal;

        protected override void OnPopulate(Builder builder)
        {
            _tree              = (TreeView)builder.GetObject("tv_skills");
            _btnResetPlusMinus = (Button)builder.GetObject("btn_reset_plusminus");
            _cboSetAllLocks    = (ComboBoxText)builder.GetObject("cbo_set_all_locks");
            _btnCopySelected   = (Button)builder.GetObject("btn_copy_selected");
            _btnCopyAll        = (Button)builder.GetObject("btn_copy_all");
            _chkDisplayChanges = (CheckButton)builder.GetObject("chk_display_changes");
            _lblBaseTotal      = (Label)builder.GetObject("lbl_base_total");

            BuildTree();
            PopulateLockOptions();
            LoadValues();
            ConnectSignals();
        }

        private void BuildTree()
        {
            if (_tree == null) return;

            // Columns: Skill Name (string), Value (double), Base (double), +/- (string)
            _store = new ListStore(typeof(string), typeof(double), typeof(double), typeof(string));
            _tree.Model = _store;

            _tree.AppendColumn("Skill Name", new CellRendererText(), "text", 0);
            _tree.AppendColumn("Value",      new CellRendererText(), "text", 1);
            _tree.AppendColumn("Base",       new CellRendererText(), "text", 2);
            _tree.AppendColumn("+/-",        new CellRendererText(), "text", 3);

            // Data will be populated from the game client when integrated.
        }

        private void PopulateLockOptions()
        {
            if (_cboSetAllLocks == null) return;
            _cboSetAllLocks.RemoveAll();
            foreach (var opt in new[] { "", "Up", "Down", "Locked" })
                _cboSetAllLocks.AppendText(opt);
        }

        private void LoadValues()
        {
            var cfg = SkillsConfig.Instance;
            cfg.Load();

            if (_chkDisplayChanges != null) _chkDisplayChanges.Active = cfg.DisplayChanges;
            if (_cboSetAllLocks    != null)
            {
                SetComboText(_cboSetAllLocks, cfg.SetAllLocks);
            }
            UpdateBaseTotal(0.0);
        }

        private void UpdateBaseTotal(double val)
        {
            if (_lblBaseTotal != null)
                _lblBaseTotal.Text = $"Base Total:  {val:F1}";
        }

        private void ConnectSignals()
        {
            if (_chkDisplayChanges != null)
                _chkDisplayChanges.Toggled += (o, e) => {
                    SkillsConfig.Instance.DisplayChanges = _chkDisplayChanges.Active;
                    SkillsConfig.Instance.Save();
                };

            if (_cboSetAllLocks != null)
                _cboSetAllLocks.Changed += (o, e) => {
                    SkillsConfig.Instance.SetAllLocks = _cboSetAllLocks.ActiveText ?? "";
                    SkillsConfig.Instance.Save();
                };

            if (_btnResetPlusMinus != null)
                _btnResetPlusMinus.Clicked += (o, e) => logger.Debug("Reset +/- clicked");

            if (_btnCopySelected != null)
                _btnCopySelected.Clicked += (o, e) => logger.Debug("Copy Selected clicked");

            if (_btnCopyAll != null)
                _btnCopyAll.Clicked += (o, e) => logger.Debug("Copy All clicked");
        }
    }
}
