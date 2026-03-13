// SkillsConfig.cs  — singleton for Skills tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class SkillsConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static SkillsConfig _instance;
        private static readonly object _lock = new object();

        public static SkillsConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new SkillsConfig()); } }
        }

        private SkillsConfig() { }

        // ── Properties ─────────────────────────────────────────────────────
        public bool   DisplayChanges { get; set; } = false;
        /// <summary>"Up", "Down", "Locked"</summary>
        public string SetAllLocks   { get; set; } = "";

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Skills");
            if (!File.Exists(path)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                DisplayChanges = root.Value<bool>("DisplayChanges");
                SetAllLocks    = root["SetAllLocks"]?.ToString() ?? "";
                logger.Debug("SkillsConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load SkillsConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Skills");
            try
            {
                var root = new JObject
                {
                    ["DisplayChanges"] = DisplayChanges,
                    ["SetAllLocks"]    = SetAllLocks
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("SkillsConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save SkillsConfig"); }
        }
    }
}
