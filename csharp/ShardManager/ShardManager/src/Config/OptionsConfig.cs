// OptionsConfig.cs  — singleton for Options tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class OptionsConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static OptionsConfig _instance;
        private static readonly object _lock = new object();

        public static OptionsConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new OptionsConfig()); } }
        }

        private OptionsConfig() { }

        // ── Overrides ──────────────────────────────────────────────────────
        public int    RazorMessageHue      { get; set; } = 0;
        public int    WarningMessageHue    { get; set; } = 0x0026;  // red
        public int    SpeechHue            { get; set; } = 0;
        public bool   OverrideSpeechHue    { get; set; } = false;
        public int    LastTargetHighlight  { get; set; } = 0;
        public bool   OverrideSpellHues    { get; set; } = false;
        public int    BeneficialHue        { get; set; } = 0x0059;  // blue
        public int    HarmfulHue           { get; set; } = 0x0026;  // red
        public int    NeutralHue           { get; set; } = 0x03B2;  // gray
        public string SpellFormat          { get; set; } = "{power} {spell}";
        /// <summary>"Show All", "Show Errors", "Hide All"</summary>
        public string RazorMessages        { get; set; } = "Show All";
        public string MapIntegrationPath   { get; set; } = "";

        // ── Targets ────────────────────────────────────────────────────────
        public bool UseSmartLastTarget          { get; set; } = false;
        public bool RangeCheckLastTarget        { get; set; } = false;
        public int  TargetTiles                 { get; set; } = 12;
        public bool ShowTargetFlagOnSingleClick { get; set; } = false;

        // ── Containers ─────────────────────────────────────────────────────
        public int  OpenNewCorpsesWithinTiles   { get; set; } = 2;
        public bool AutoSearchNewContainers     { get; set; } = false;
        public bool IgnorePouches               { get; set; } = true;

        // ── Stealth ────────────────────────────────────────────────────────
        public bool CountStealthSteps           { get; set; } = false;
        public bool BlockRunIfStealthed         { get; set; } = false;

        // ── Miscellaneous ──────────────────────────────────────────────────
        public bool BlockDismountInWarMode      { get; set; } = false;
        public bool AutoStackOreAtFeet          { get; set; } = false;
        public bool AutomaticallyOpenDoors      { get; set; } = false;
        public bool DisableIfHidden             { get; set; } = false;

        // ── Queues ─────────────────────────────────────────────────────────
        public bool ShowActionQueueStatus       { get; set; } = false;
        public bool AutoQueueObjectDelayActions { get; set; } = false;
        public int  ObjectDelayMs               { get; set; } = 600;
        public bool QueueLastTargetAndSelf      { get; set; } = false;

        // ── Visual ─────────────────────────────────────────────────────────
        public bool   ShowNamesNewMobiles       { get; set; } = false;
        public bool   ShowNamesNewCorpses       { get; set; } = false;
        public bool   ShowHealthAboveMobiles    { get; set; } = false;
        public string HealthFormat              { get; set; } = "{0}%";
        public bool   ShowPartyManaStam         { get; set; } = false;

        // ── Spells / Potions ───────────────────────────────────────────────
        public bool AutoUnReequipForPotions     { get; set; } = false;
        public bool AutoUnequipBeforeCasting    { get; set; } = false;
        public bool UsePacketsDruidCleric       { get; set; } = false;

        // ── Status Window ──────────────────────────────────────────────────
        public bool UsePreAosStatusWindow       { get; set; } = false;

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Options");
            if (!File.Exists(path)) return;
            try
            {
                var r = JObject.Parse(File.ReadAllText(path));
                RazorMessageHue             = r["RazorMessageHue"]            != null ? r.Value<int>("RazorMessageHue")            : 0;
                WarningMessageHue           = r["WarningMessageHue"]          != null ? r.Value<int>("WarningMessageHue")          : 0x0026;
                SpeechHue                   = r["SpeechHue"]                  != null ? r.Value<int>("SpeechHue")                  : 0;
                OverrideSpeechHue           = r.Value<bool>("OverrideSpeechHue");
                LastTargetHighlight         = r["LastTargetHighlight"]        != null ? r.Value<int>("LastTargetHighlight")        : 0;
                OverrideSpellHues           = r.Value<bool>("OverrideSpellHues");
                BeneficialHue              = r["BeneficialHue"]              != null ? r.Value<int>("BeneficialHue")              : 0x0059;
                HarmfulHue                 = r["HarmfulHue"]                 != null ? r.Value<int>("HarmfulHue")                 : 0x0026;
                NeutralHue                 = r["NeutralHue"]                 != null ? r.Value<int>("NeutralHue")                 : 0x03B2;
                SpellFormat                = r["SpellFormat"]?.ToString()    ?? "{power} {spell}";
                RazorMessages              = r["RazorMessages"]?.ToString()  ?? "Show All";
                MapIntegrationPath         = r["MapIntegrationPath"]?.ToString() ?? "";
                UseSmartLastTarget          = r.Value<bool>("UseSmartLastTarget");
                RangeCheckLastTarget        = r.Value<bool>("RangeCheckLastTarget");
                TargetTiles                 = r["TargetTiles"]                != null ? r.Value<int>("TargetTiles")                : 12;
                ShowTargetFlagOnSingleClick = r.Value<bool>("ShowTargetFlagOnSingleClick");
                OpenNewCorpsesWithinTiles   = r["OpenNewCorpsesWithinTiles"]  != null ? r.Value<int>("OpenNewCorpsesWithinTiles")  : 2;
                AutoSearchNewContainers     = r.Value<bool>("AutoSearchNewContainers");
                IgnorePouches               = r["IgnorePouches"]              != null ? r.Value<bool>("IgnorePouches")             : true;
                CountStealthSteps           = r.Value<bool>("CountStealthSteps");
                BlockRunIfStealthed         = r.Value<bool>("BlockRunIfStealthed");
                BlockDismountInWarMode      = r.Value<bool>("BlockDismountInWarMode");
                AutoStackOreAtFeet          = r.Value<bool>("AutoStackOreAtFeet");
                AutomaticallyOpenDoors      = r.Value<bool>("AutomaticallyOpenDoors");
                DisableIfHidden             = r.Value<bool>("DisableIfHidden");
                ShowActionQueueStatus       = r.Value<bool>("ShowActionQueueStatus");
                AutoQueueObjectDelayActions = r.Value<bool>("AutoQueueObjectDelayActions");
                ObjectDelayMs               = r["ObjectDelayMs"]              != null ? r.Value<int>("ObjectDelayMs")              : 600;
                QueueLastTargetAndSelf      = r.Value<bool>("QueueLastTargetAndSelf");
                ShowNamesNewMobiles         = r.Value<bool>("ShowNamesNewMobiles");
                ShowNamesNewCorpses         = r.Value<bool>("ShowNamesNewCorpses");
                ShowHealthAboveMobiles      = r.Value<bool>("ShowHealthAboveMobiles");
                HealthFormat                = r["HealthFormat"]?.ToString()   ?? "{0}%";
                ShowPartyManaStam           = r.Value<bool>("ShowPartyManaStam");
                AutoUnReequipForPotions     = r.Value<bool>("AutoUnReequipForPotions");
                AutoUnequipBeforeCasting    = r.Value<bool>("AutoUnequipBeforeCasting");
                UsePacketsDruidCleric       = r.Value<bool>("UsePacketsDruidCleric");
                UsePreAosStatusWindow       = r.Value<bool>("UsePreAosStatusWindow");
                logger.Debug("OptionsConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load OptionsConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Options");
            try
            {
                var root = new JObject
                {
                    ["RazorMessageHue"]             = RazorMessageHue,
                    ["WarningMessageHue"]           = WarningMessageHue,
                    ["SpeechHue"]                   = SpeechHue,
                    ["OverrideSpeechHue"]           = OverrideSpeechHue,
                    ["LastTargetHighlight"]         = LastTargetHighlight,
                    ["OverrideSpellHues"]           = OverrideSpellHues,
                    ["BeneficialHue"]               = BeneficialHue,
                    ["HarmfulHue"]                  = HarmfulHue,
                    ["NeutralHue"]                  = NeutralHue,
                    ["SpellFormat"]                 = SpellFormat,
                    ["RazorMessages"]               = RazorMessages,
                    ["MapIntegrationPath"]          = MapIntegrationPath,
                    ["UseSmartLastTarget"]          = UseSmartLastTarget,
                    ["RangeCheckLastTarget"]        = RangeCheckLastTarget,
                    ["TargetTiles"]                 = TargetTiles,
                    ["ShowTargetFlagOnSingleClick"] = ShowTargetFlagOnSingleClick,
                    ["OpenNewCorpsesWithinTiles"]   = OpenNewCorpsesWithinTiles,
                    ["AutoSearchNewContainers"]     = AutoSearchNewContainers,
                    ["IgnorePouches"]               = IgnorePouches,
                    ["CountStealthSteps"]           = CountStealthSteps,
                    ["BlockRunIfStealthed"]         = BlockRunIfStealthed,
                    ["BlockDismountInWarMode"]      = BlockDismountInWarMode,
                    ["AutoStackOreAtFeet"]          = AutoStackOreAtFeet,
                    ["AutomaticallyOpenDoors"]      = AutomaticallyOpenDoors,
                    ["DisableIfHidden"]             = DisableIfHidden,
                    ["ShowActionQueueStatus"]       = ShowActionQueueStatus,
                    ["AutoQueueObjectDelayActions"] = AutoQueueObjectDelayActions,
                    ["ObjectDelayMs"]               = ObjectDelayMs,
                    ["QueueLastTargetAndSelf"]      = QueueLastTargetAndSelf,
                    ["ShowNamesNewMobiles"]         = ShowNamesNewMobiles,
                    ["ShowNamesNewCorpses"]         = ShowNamesNewCorpses,
                    ["ShowHealthAboveMobiles"]      = ShowHealthAboveMobiles,
                    ["HealthFormat"]                = HealthFormat,
                    ["ShowPartyManaStam"]           = ShowPartyManaStam,
                    ["AutoUnReequipForPotions"]     = AutoUnReequipForPotions,
                    ["AutoUnequipBeforeCasting"]    = AutoUnequipBeforeCasting,
                    ["UsePacketsDruidCleric"]       = UsePacketsDruidCleric,
                    ["UsePreAosStatusWindow"]       = UsePreAosStatusWindow
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("OptionsConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save OptionsConfig"); }
        }
    }
}
