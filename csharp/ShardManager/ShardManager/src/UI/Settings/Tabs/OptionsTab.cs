// OptionsTab.cs
using System;
using Gtk;
using Config;
using UI.Settings;

namespace UI.Settings.Tabs
{
    public class OptionsTab : TabBase
    {
        public override string TabName   => "Options";
        protected override string UiFile  => "OptionsTab.ui";

        // Overrides
        private Button       _btnRazorHue;
        private Button       _btnWarningHue;
        private Button       _btnSpeechHue;
        private CheckButton  _chkOverrideSpeech;
        private Button       _btnLastTargetHighlight;
        private CheckButton  _chkOverrideSpellHues;
        private Button       _btnBeneficialHue;
        private Button       _btnHarmfulHue;
        private Button       _btnNeutralHue;
        private Entry        _entSpellFormat;
        private ComboBoxText _cboRazorMessages;
        private Entry        _entMapPath;

        // Targets
        private CheckButton _chkSmartLastTarget;
        private CheckButton _chkRangeCheckLastTarget;
        private Entry       _entTargetTiles;
        private CheckButton _chkShowTargetFlag;

        // Containers
        private Entry       _entCorpseTiles;
        private CheckButton _chkAutoSearchContainers;
        private CheckButton _chkIgnorePouches;

        // Stealth
        private CheckButton _chkCountStealthSteps;
        private CheckButton _chkBlockRunStealthed;

        // Misc
        private CheckButton _chkBlockDismount;
        private CheckButton _chkAutoStackOre;
        private CheckButton _chkAutoOpenDoors;
        private CheckButton _chkDisableIfHidden;

        // Queues
        private CheckButton _chkShowActionQueue;
        private CheckButton _chkAutoQueueDelay;
        private Entry       _entObjectDelay;
        private CheckButton _chkQueueLastTarget;

        // Visual
        private CheckButton _chkShowNamesNewMobiles;
        private CheckButton _chkShowNamesNewCorpses;
        private CheckButton _chkShowHealthAbove;
        private Entry       _entHealthFormat;
        private CheckButton _chkShowPartyManaStam;

        // Spells/Potions
        private CheckButton _chkAutoUnReequip;
        private CheckButton _chkAutoUnequipCasting;
        private CheckButton _chkUsePacketsDruid;

        // Status Window
        private CheckButton _chkPreAosStatus;

        protected override void OnPopulate(Builder builder)
        {
            _btnRazorHue            = (Button)builder.GetObject("btn_razor_hue");
            _btnWarningHue          = (Button)builder.GetObject("btn_warning_hue");
            _btnSpeechHue           = (Button)builder.GetObject("btn_speech_hue");
            _chkOverrideSpeech      = (CheckButton)builder.GetObject("chk_override_speech");
            _btnLastTargetHighlight = (Button)builder.GetObject("btn_last_target_highlight");
            _chkOverrideSpellHues   = (CheckButton)builder.GetObject("chk_override_spell_hues");
            _btnBeneficialHue       = (Button)builder.GetObject("btn_beneficial_hue");
            _btnHarmfulHue          = (Button)builder.GetObject("btn_harmful_hue");
            _btnNeutralHue          = (Button)builder.GetObject("btn_neutral_hue");
            _entSpellFormat         = (Entry)builder.GetObject("ent_spell_format");
            _cboRazorMessages       = (ComboBoxText)builder.GetObject("cbo_razor_messages");
            _entMapPath             = (Entry)builder.GetObject("ent_map_path");

            _chkSmartLastTarget      = (CheckButton)builder.GetObject("chk_smart_last_target");
            _chkRangeCheckLastTarget = (CheckButton)builder.GetObject("chk_range_check_last_target");
            _entTargetTiles          = (Entry)builder.GetObject("ent_target_tiles");
            _chkShowTargetFlag       = (CheckButton)builder.GetObject("chk_show_target_flag");

            _entCorpseTiles          = (Entry)builder.GetObject("ent_corpse_tiles");
            _chkAutoSearchContainers = (CheckButton)builder.GetObject("chk_auto_search_containers");
            _chkIgnorePouches        = (CheckButton)builder.GetObject("chk_ignore_pouches");

            _chkCountStealthSteps    = (CheckButton)builder.GetObject("chk_count_stealth_steps");
            _chkBlockRunStealthed    = (CheckButton)builder.GetObject("chk_block_run_stealthed");

            _chkBlockDismount        = (CheckButton)builder.GetObject("chk_block_dismount");
            _chkAutoStackOre         = (CheckButton)builder.GetObject("chk_auto_stack_ore");
            _chkAutoOpenDoors        = (CheckButton)builder.GetObject("chk_auto_open_doors");
            _chkDisableIfHidden      = (CheckButton)builder.GetObject("chk_disable_if_hidden");

            _chkShowActionQueue      = (CheckButton)builder.GetObject("chk_show_action_queue");
            _chkAutoQueueDelay       = (CheckButton)builder.GetObject("chk_auto_queue_delay");
            _entObjectDelay          = (Entry)builder.GetObject("ent_object_delay");
            _chkQueueLastTarget      = (CheckButton)builder.GetObject("chk_queue_last_target");

            _chkShowNamesNewMobiles  = (CheckButton)builder.GetObject("chk_show_names_new_mobiles");
            _chkShowNamesNewCorpses  = (CheckButton)builder.GetObject("chk_show_names_new_corpses");
            _chkShowHealthAbove      = (CheckButton)builder.GetObject("chk_show_health_above");
            _entHealthFormat         = (Entry)builder.GetObject("ent_health_format");
            _chkShowPartyManaStam    = (CheckButton)builder.GetObject("chk_show_party_mana_stam");

            _chkAutoUnReequip        = (CheckButton)builder.GetObject("chk_auto_unreequip");
            _chkAutoUnequipCasting   = (CheckButton)builder.GetObject("chk_auto_unequip_casting");
            _chkUsePacketsDruid      = (CheckButton)builder.GetObject("chk_use_packets_druid");

            _chkPreAosStatus         = (CheckButton)builder.GetObject("chk_pre_aos_status");

            PopulateRazorMessages();
            LoadValues();
            ConnectSignals();
        }

        private void PopulateRazorMessages()
        {
            if (_cboRazorMessages == null) return;
            _cboRazorMessages.RemoveAll();
            foreach (var opt in new[] { "Show All", "Show Errors", "Hide All" })
                _cboRazorMessages.AppendText(opt);
        }

        private void LoadValues()
        {
            var cfg = OptionsConfig.Instance;
            cfg.Load();

            if (_entSpellFormat      != null) _entSpellFormat.Text       = cfg.SpellFormat;
            if (_cboRazorMessages    != null) SetComboText(_cboRazorMessages, cfg.RazorMessages);
            if (_entMapPath          != null) _entMapPath.Text            = cfg.MapIntegrationPath;
            if (_chkOverrideSpeech   != null) _chkOverrideSpeech.Active   = cfg.OverrideSpeechHue;
            if (_chkOverrideSpellHues!= null) _chkOverrideSpellHues.Active= cfg.OverrideSpellHues;

            if (_chkSmartLastTarget      != null) _chkSmartLastTarget.Active      = cfg.UseSmartLastTarget;
            if (_chkRangeCheckLastTarget != null) _chkRangeCheckLastTarget.Active = cfg.RangeCheckLastTarget;
            if (_entTargetTiles          != null) _entTargetTiles.Text             = cfg.TargetTiles.ToString();
            if (_chkShowTargetFlag       != null) _chkShowTargetFlag.Active        = cfg.ShowTargetFlagOnSingleClick;

            if (_entCorpseTiles          != null) _entCorpseTiles.Text             = cfg.OpenNewCorpsesWithinTiles.ToString();
            if (_chkAutoSearchContainers != null) _chkAutoSearchContainers.Active  = cfg.AutoSearchNewContainers;
            if (_chkIgnorePouches        != null) _chkIgnorePouches.Active         = cfg.IgnorePouches;

            if (_chkCountStealthSteps != null) _chkCountStealthSteps.Active = cfg.CountStealthSteps;
            if (_chkBlockRunStealthed != null) _chkBlockRunStealthed.Active  = cfg.BlockRunIfStealthed;

            if (_chkBlockDismount   != null) _chkBlockDismount.Active   = cfg.BlockDismountInWarMode;
            if (_chkAutoStackOre    != null) _chkAutoStackOre.Active    = cfg.AutoStackOreAtFeet;
            if (_chkAutoOpenDoors   != null) _chkAutoOpenDoors.Active   = cfg.AutomaticallyOpenDoors;
            if (_chkDisableIfHidden != null) _chkDisableIfHidden.Active = cfg.DisableIfHidden;

            if (_chkShowActionQueue != null) _chkShowActionQueue.Active = cfg.ShowActionQueueStatus;
            if (_chkAutoQueueDelay  != null) _chkAutoQueueDelay.Active  = cfg.AutoQueueObjectDelayActions;
            if (_entObjectDelay     != null) _entObjectDelay.Text        = cfg.ObjectDelayMs.ToString();
            if (_chkQueueLastTarget != null) _chkQueueLastTarget.Active  = cfg.QueueLastTargetAndSelf;

            if (_chkShowNamesNewMobiles != null) _chkShowNamesNewMobiles.Active = cfg.ShowNamesNewMobiles;
            if (_chkShowNamesNewCorpses != null) _chkShowNamesNewCorpses.Active = cfg.ShowNamesNewCorpses;
            if (_chkShowHealthAbove     != null) _chkShowHealthAbove.Active     = cfg.ShowHealthAboveMobiles;
            if (_entHealthFormat        != null) _entHealthFormat.Text           = cfg.HealthFormat;
            if (_chkShowPartyManaStam   != null) _chkShowPartyManaStam.Active   = cfg.ShowPartyManaStam;

            if (_chkAutoUnReequip      != null) _chkAutoUnReequip.Active      = cfg.AutoUnReequipForPotions;
            if (_chkAutoUnequipCasting != null) _chkAutoUnequipCasting.Active = cfg.AutoUnequipBeforeCasting;
            if (_chkUsePacketsDruid    != null) _chkUsePacketsDruid.Active    = cfg.UsePacketsDruidCleric;

            if (_chkPreAosStatus != null) _chkPreAosStatus.Active = cfg.UsePreAosStatusWindow;
        }

        private void SaveValues()
        {
            var cfg = OptionsConfig.Instance;
            if (_entSpellFormat          != null) cfg.SpellFormat                 = _entSpellFormat.Text;
            if (_cboRazorMessages        != null) cfg.RazorMessages               = _cboRazorMessages.ActiveText ?? "Show All";
            if (_entMapPath              != null) cfg.MapIntegrationPath           = _entMapPath.Text;
            if (_chkOverrideSpeech       != null) cfg.OverrideSpeechHue           = _chkOverrideSpeech.Active;
            if (_chkOverrideSpellHues    != null) cfg.OverrideSpellHues           = _chkOverrideSpellHues.Active;
            if (_chkSmartLastTarget      != null) cfg.UseSmartLastTarget          = _chkSmartLastTarget.Active;
            if (_chkRangeCheckLastTarget != null) cfg.RangeCheckLastTarget        = _chkRangeCheckLastTarget.Active;
            if (_entTargetTiles          != null && int.TryParse(_entTargetTiles.Text, out int t)) cfg.TargetTiles = t;
            if (_chkShowTargetFlag       != null) cfg.ShowTargetFlagOnSingleClick  = _chkShowTargetFlag.Active;
            if (_entCorpseTiles          != null && int.TryParse(_entCorpseTiles.Text, out int ct)) cfg.OpenNewCorpsesWithinTiles = ct;
            if (_chkAutoSearchContainers != null) cfg.AutoSearchNewContainers     = _chkAutoSearchContainers.Active;
            if (_chkIgnorePouches        != null) cfg.IgnorePouches               = _chkIgnorePouches.Active;
            if (_chkCountStealthSteps    != null) cfg.CountStealthSteps           = _chkCountStealthSteps.Active;
            if (_chkBlockRunStealthed    != null) cfg.BlockRunIfStealthed         = _chkBlockRunStealthed.Active;
            if (_chkBlockDismount        != null) cfg.BlockDismountInWarMode      = _chkBlockDismount.Active;
            if (_chkAutoStackOre         != null) cfg.AutoStackOreAtFeet          = _chkAutoStackOre.Active;
            if (_chkAutoOpenDoors        != null) cfg.AutomaticallyOpenDoors      = _chkAutoOpenDoors.Active;
            if (_chkDisableIfHidden      != null) cfg.DisableIfHidden             = _chkDisableIfHidden.Active;
            if (_chkShowActionQueue      != null) cfg.ShowActionQueueStatus       = _chkShowActionQueue.Active;
            if (_chkAutoQueueDelay       != null) cfg.AutoQueueObjectDelayActions = _chkAutoQueueDelay.Active;
            if (_entObjectDelay          != null && int.TryParse(_entObjectDelay.Text, out int od)) cfg.ObjectDelayMs = od;
            if (_chkQueueLastTarget      != null) cfg.QueueLastTargetAndSelf      = _chkQueueLastTarget.Active;
            if (_chkShowNamesNewMobiles  != null) cfg.ShowNamesNewMobiles         = _chkShowNamesNewMobiles.Active;
            if (_chkShowNamesNewCorpses  != null) cfg.ShowNamesNewCorpses         = _chkShowNamesNewCorpses.Active;
            if (_chkShowHealthAbove      != null) cfg.ShowHealthAboveMobiles      = _chkShowHealthAbove.Active;
            if (_entHealthFormat         != null) cfg.HealthFormat                = _entHealthFormat.Text;
            if (_chkShowPartyManaStam    != null) cfg.ShowPartyManaStam           = _chkShowPartyManaStam.Active;
            if (_chkAutoUnReequip        != null) cfg.AutoUnReequipForPotions     = _chkAutoUnReequip.Active;
            if (_chkAutoUnequipCasting   != null) cfg.AutoUnequipBeforeCasting    = _chkAutoUnequipCasting.Active;
            if (_chkUsePacketsDruid      != null) cfg.UsePacketsDruidCleric       = _chkUsePacketsDruid.Active;
            if (_chkPreAosStatus         != null) cfg.UsePreAosStatusWindow       = _chkPreAosStatus.Active;
            cfg.Save();
        }

        private void ConnectSignals()
        {
            void S(CheckButton c) { if (c != null) c.Toggled += (o, e) => SaveValues(); }
            S(_chkOverrideSpeech); S(_chkOverrideSpellHues); S(_chkSmartLastTarget);
            S(_chkRangeCheckLastTarget); S(_chkShowTargetFlag); S(_chkAutoSearchContainers);
            S(_chkIgnorePouches); S(_chkCountStealthSteps); S(_chkBlockRunStealthed);
            S(_chkBlockDismount); S(_chkAutoStackOre); S(_chkAutoOpenDoors); S(_chkDisableIfHidden);
            S(_chkShowActionQueue); S(_chkAutoQueueDelay); S(_chkQueueLastTarget);
            S(_chkShowNamesNewMobiles); S(_chkShowNamesNewCorpses); S(_chkShowHealthAbove);
            S(_chkShowPartyManaStam); S(_chkAutoUnReequip); S(_chkAutoUnequipCasting);
            S(_chkUsePacketsDruid); S(_chkPreAosStatus);

            if (_cboRazorMessages != null) _cboRazorMessages.Changed += (o, e) => SaveValues();
            if (_entSpellFormat   != null) _entSpellFormat.Changed   += (o, e) => SaveValues();
            if (_entObjectDelay   != null) _entObjectDelay.Changed   += (o, e) => SaveValues();
            if (_entHealthFormat  != null) _entHealthFormat.Changed  += (o, e) => SaveValues();
            if (_entMapPath       != null) _entMapPath.Changed       += (o, e) => SaveValues();

            // Hue buttons — show a simple hex input dialog
            void HueBtn(Button btn, string label, Func<int> get, Action<int> set) {
                if (btn == null) return;
                btn.Clicked += (o, e) => {
                    var dlg = new Dialog(label, null, DialogFlags.Modal, "OK", ResponseType.Ok, "Cancel", ResponseType.Cancel);
                    var entry = new Entry { Text = $"0x{get():X4}", Visible = true };
                    dlg.ContentArea.Add(new Label(label) { Visible = true });
                    dlg.ContentArea.Add(entry);
                    dlg.ShowAll();
                    if (dlg.Run() == (int)ResponseType.Ok)
                    {
                        string val = entry.Text.Replace("0x", "").Replace("0X", "");
                        if (int.TryParse(val, System.Globalization.NumberStyles.HexNumber, null, out int hue))
                        { set(hue); SaveValues(); }
                    }
                    dlg.Destroy();
                };
            }
            var cfg = OptionsConfig.Instance;
            HueBtn(_btnRazorHue,            "Razor Message Hue",   () => cfg.RazorMessageHue,   v => cfg.RazorMessageHue   = v);
            HueBtn(_btnWarningHue,          "Warning Message Hue", () => cfg.WarningMessageHue, v => cfg.WarningMessageHue = v);
            HueBtn(_btnSpeechHue,           "Speech Hue",          () => cfg.SpeechHue,         v => cfg.SpeechHue         = v);
            HueBtn(_btnLastTargetHighlight, "Last Target Highlight",() => cfg.LastTargetHighlight,v => cfg.LastTargetHighlight = v);
            HueBtn(_btnBeneficialHue,       "Beneficial Hue",      () => cfg.BeneficialHue,    v => cfg.BeneficialHue     = v);
            HueBtn(_btnHarmfulHue,          "Harmful Hue",         () => cfg.HarmfulHue,       v => cfg.HarmfulHue        = v);
            HueBtn(_btnNeutralHue,          "Neutral Hue",         () => cfg.NeutralHue,       v => cfg.NeutralHue        = v);
        }
    }
}
