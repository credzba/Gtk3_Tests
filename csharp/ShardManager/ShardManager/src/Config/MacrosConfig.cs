// MacrosConfig.cs  — singleton for Macros tab settings
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class MacroEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Loop")]
        public bool Loop { get; set; } = false;

        [JsonProperty("HotKey")]
        public string HotKey { get; set; } = "";

        [JsonProperty("PassKeyToUo")]
        public bool PassKeyToUo { get; set; } = true;

        // Actions are stored as a list of {Action, Details} pairs
        [JsonProperty("Actions")]
        public List<MacroAction> Actions { get; set; } = new List<MacroAction>();
    }

    public class MacroAction
    {
        [JsonProperty("Action")]
        public string Action { get; set; } = "";

        [JsonProperty("Details")]
        public string Details { get; set; } = "";
    }

    public class MacrosConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static MacrosConfig _instance;
        private static readonly object _lock = new object();

        public static MacrosConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new MacrosConfig()); } }
        }

        private MacrosConfig() { }

        public List<MacroEntry> Macros { get; private set; } = new List<MacroEntry>();

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Macros");
            if (!File.Exists(path)) return;
            try
            {
                var r = JObject.Parse(File.ReadAllText(path));
                var arr = r["Macros"] as JArray;
                Macros.Clear();
                if (arr != null)
                    foreach (var item in arr)
                        Macros.Add(item.ToObject<MacroEntry>());
                logger.Debug("MacrosConfig loaded — {0} macros", Macros.Count);
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load MacrosConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Macros");
            try
            {
                var root = new JObject { ["Macros"] = JArray.FromObject(Macros) };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("MacrosConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save MacrosConfig"); }
        }
    }
}
