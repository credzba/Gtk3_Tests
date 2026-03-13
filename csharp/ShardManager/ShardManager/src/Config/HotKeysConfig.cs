// HotKeysConfig.cs  — singleton for Hot Keys tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class HotKeysConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static HotKeysConfig _instance;
        private static readonly object _lock = new object();

        public static HotKeysConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new HotKeysConfig()); } }
        }

        private HotKeysConfig() { }

        // ── Properties ─────────────────────────────────────────────────────
        public bool   Enabled    { get; set; } = true;
        public string OnOffKey   { get; set; } = "";
        public string MasterKey  { get; set; } = "";
        public string ModifyKey  { get; set; } = "";
        public bool   PassKeyToUo { get; set; } = false;

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("HotKeys");
            if (!File.Exists(path)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                Enabled     = root["Enabled"]    != null ? root.Value<bool>("Enabled") : true;
                OnOffKey    = root["OnOffKey"]?.ToString()  ?? "";
                MasterKey   = root["MasterKey"]?.ToString() ?? "";
                ModifyKey   = root["ModifyKey"]?.ToString() ?? "";
                PassKeyToUo = root.Value<bool>("PassKeyToUo");
                logger.Debug("HotKeysConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load HotKeysConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("HotKeys");
            try
            {
                var root = new JObject
                {
                    ["Enabled"]     = Enabled,
                    ["OnOffKey"]    = OnOffKey,
                    ["MasterKey"]   = MasterKey,
                    ["ModifyKey"]   = ModifyKey,
                    ["PassKeyToUo"] = PassKeyToUo
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("HotKeysConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save HotKeysConfig"); }
        }
    }
}
