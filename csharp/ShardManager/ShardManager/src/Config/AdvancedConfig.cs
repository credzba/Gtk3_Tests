// AdvancedConfig.cs  — singleton for Advanced tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class AdvancedConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static AdvancedConfig _instance;
        private static readonly object _lock = new object();

        public static AdvancedConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new AdvancedConfig()); } }
        }

        private AdvancedConfig() { }

        // ── Screen Shots ───────────────────────────────────────────────────
        public string ScreenShotDirectory    { get; set; } = "";
        /// <summary>"jpg", "png", "bmp"</summary>
        public string ImageFormat            { get; set; } = "jpg";
        /// <summary>true = UO Only, false = Full Screen</summary>
        public bool   UoOnly                 { get; set; } = true;
        public bool   IncludeTimestamp       { get; set; } = false;
        public bool   AutoDeathScreenCapture { get; set; } = false;

        // ── Video Recorder (placeholder) ───────────────────────────────────
        // Fields to be added when that sub-tab is designed.

        // ── DPS Meter (placeholder) ────────────────────────────────────────
        // Fields to be added when that sub-tab is designed.

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Advanced");
            if (!File.Exists(path)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                ScreenShotDirectory    = root["ScreenShotDirectory"]?.ToString()  ?? "";
                ImageFormat            = root["ImageFormat"]?.ToString()           ?? "jpg";
                UoOnly                 = root["UoOnly"]                != null ? root.Value<bool>("UoOnly")                : true;
                IncludeTimestamp       = root.Value<bool>("IncludeTimestamp");
                AutoDeathScreenCapture = root.Value<bool>("AutoDeathScreenCapture");
                logger.Debug("AdvancedConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load AdvancedConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Advanced");
            try
            {
                var root = new JObject
                {
                    ["ScreenShotDirectory"]    = ScreenShotDirectory,
                    ["ImageFormat"]            = ImageFormat,
                    ["UoOnly"]                 = UoOnly,
                    ["IncludeTimestamp"]       = IncludeTimestamp,
                    ["AutoDeathScreenCapture"] = AutoDeathScreenCapture
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("AdvancedConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save AdvancedConfig"); }
        }
    }
}
