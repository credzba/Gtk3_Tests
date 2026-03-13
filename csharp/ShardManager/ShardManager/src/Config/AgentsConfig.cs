// AgentsConfig.cs  — singleton for Agents tab settings (Autoloot etc.)
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class AutolootItem
    {
        [JsonProperty("Enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("Name")]
        public string Name { get; set; } = "";

        [JsonProperty("Graphics")]
        public string Graphics { get; set; } = "0x0000";

        [JsonProperty("Color")]
        public string Color { get; set; } = "0x0000";

        [JsonProperty("Bag")]
        public string Bag { get; set; } = "";
    }

    public class AutolootList
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = "New List";

        [JsonProperty("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonProperty("NoOpenCorpse")]
        public bool NoOpenCorpse { get; set; } = false;

        [JsonProperty("AutostartOnLogin")]
        public bool AutostartOnLogin { get; set; } = false;

        [JsonProperty("AllowLootingWhileHidden")]
        public bool AllowLootingWhileHidden { get; set; } = false;

        [JsonProperty("BagSerial")]
        public string BagSerial { get; set; } = "0x00000000";

        [JsonProperty("DelayMs")]
        public int DelayMs { get; set; } = 0;

        [JsonProperty("MaxRange")]
        public int MaxRange { get; set; } = 2;

        [JsonProperty("Items")]
        public List<AutolootItem> Items { get; set; } = new List<AutolootItem>();
    }

    public class AgentsConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static AgentsConfig _instance;
        private static readonly object _lock = new object();

        public static AgentsConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new AgentsConfig()); } }
        }

        private AgentsConfig() { }

        // ── Autoloot ───────────────────────────────────────────────────────
        public List<AutolootList> AutolootLists    { get; private set; } = new List<AutolootList>();
        public int                ActiveAutolootIdx { get; set; } = 0;

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Agents");
            if (!File.Exists(path)) return;
            try
            {
                var r = JObject.Parse(File.ReadAllText(path));
                ActiveAutolootIdx = r["ActiveAutolootIdx"] != null ? r.Value<int>("ActiveAutolootIdx") : 0;

                var arr = r["AutolootLists"] as JArray;
                AutolootLists.Clear();
                if (arr != null)
                    foreach (var item in arr)
                        AutolootLists.Add(item.ToObject<AutolootList>());

                logger.Debug("AgentsConfig loaded — {0} autoloot lists", AutolootLists.Count);
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load AgentsConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Agents");
            try
            {
                var root = new JObject
                {
                    ["ActiveAutolootIdx"] = ActiveAutolootIdx,
                    ["AutolootLists"]     = JArray.FromObject(AutolootLists)
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("AgentsConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save AgentsConfig"); }
        }
    }
}
