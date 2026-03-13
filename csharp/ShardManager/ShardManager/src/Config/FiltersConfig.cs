// FiltersConfig.cs  — singleton for Filters tab settings
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class FiltersConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static FiltersConfig _instance;
        private static readonly object _lock = new object();

        public static FiltersConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new FiltersConfig()); } }
        }

        private FiltersConfig() { }

        // ── Virtual sub-tab ────────────────────────────────────────────────
        public bool RepeatingSysMessages { get; set; } = false;
        public bool SnoopingMessages     { get; set; } = false;
        public bool PoisonMessages       { get; set; } = false;
        public bool MonsterMessages      { get; set; } = false;

        // More Filters — keyed by display name
        public Dictionary<string, bool> MoreFilters { get; private set; } = new Dictionary<string, bool>
        {
            ["Death Status"]       = false,
            ["Light Levels"]       = false,
            ["Seasons"]            = false,
            ["Bards' Music"]       = false,
            ["Animal Sounds"]      = false,
            ["Bird Sounds"]        = false,
            ["Spirit Speak Sound"] = false,
            ["Spell Fizzle Sound"] = false,
            ["Backpack Sound"]     = false,
            ["Cyclop/Titan Sounds"]= false,
            ["Dragon Sounds"]      = false,
            ["Divine Fury"]        = false,
            ["Gate Sound"]         = false,
            ["Detector Sound"]     = false,
            ["Drag"]               = false,
            ["LockPick"]           = false
        };

        // Journal filter
        public string       JournalFilterText { get; set; } = "";
        public List<string> JournalFilters    { get; private set; } = new List<string>();

        // ── Targetting sub-tab (placeholder) ──────────────────────────────
        // Fields to be added when UI design is provided.

        // ── Misc sub-tab (placeholder) ─────────────────────────────────────
        // Fields to be added when UI design is provided.

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Filters");
            if (!File.Exists(path)) return;
            try
            {
                var r = JObject.Parse(File.ReadAllText(path));
                RepeatingSysMessages = r.Value<bool>("RepeatingSysMessages");
                SnoopingMessages     = r.Value<bool>("SnoopingMessages");
                PoisonMessages       = r.Value<bool>("PoisonMessages");
                MonsterMessages      = r.Value<bool>("MonsterMessages");
                JournalFilterText    = r["JournalFilterText"]?.ToString() ?? "";

                var mf = r["MoreFilters"] as JObject;
                if (mf != null)
                    foreach (var key in new List<string>(MoreFilters.Keys))
                        if (mf[key] != null)
                            MoreFilters[key] = mf.Value<bool>(key);

                var jf = r["JournalFilters"] as JArray;
                if (jf != null)
                {
                    JournalFilters.Clear();
                    foreach (var item in jf) JournalFilters.Add(item.ToString());
                }
                logger.Debug("FiltersConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load FiltersConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Filters");
            try
            {
                var mfObj = new JObject();
                foreach (var kv in MoreFilters) mfObj[kv.Key] = kv.Value;

                var root = new JObject
                {
                    ["RepeatingSysMessages"] = RepeatingSysMessages,
                    ["SnoopingMessages"]     = SnoopingMessages,
                    ["PoisonMessages"]       = PoisonMessages,
                    ["MonsterMessages"]      = MonsterMessages,
                    ["JournalFilterText"]    = JournalFilterText,
                    ["MoreFilters"]          = mfObj,
                    ["JournalFilters"]       = new JArray(JournalFilters)
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("FiltersConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save FiltersConfig"); }
        }
    }
}
