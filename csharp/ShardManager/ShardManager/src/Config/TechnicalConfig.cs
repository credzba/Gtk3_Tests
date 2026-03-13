// TechnicalConfig.cs  — singleton for Technical tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class TechnicalConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static TechnicalConfig _instance;
        private static readonly object _lock = new object();

        public static TechnicalConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new TechnicalConfig()); } }
        }

        private TechnicalConfig() { }

        // ── Properties ─────────────────────────────────────────────────────

        public bool   UseSmartCpuReduction  { get; set; } = false;
        public bool   RememberPasswords     { get; set; } = false;
        public bool   ShowLauncherWindow    { get; set; } = true;
        public bool   UseSmartAlwaysOnTop   { get; set; } = false;
        public bool   ForceGameSize         { get; set; } = false;
        public int    GameSizeWidth         { get; set; } = 800;
        public int    GameSizeHeight        { get; set; } = 600;
        /// <summary>"Taskbar" or "SystemTray"</summary>
        public string ShowIn                { get; set; } = "Taskbar";
        public bool   EnableRemoteControl   { get; set; } = false;
        /// <summary>"Normal", "High", "AboveNormal", etc.</summary>
        public string DefaultClientPriority { get; set; } = "Normal";
        public int    Opacity               { get; set; } = 100;

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Technical");
            if (!File.Exists(path)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                UseSmartCpuReduction  = root.Value<bool>("UseSmartCpuReduction");
                RememberPasswords     = root.Value<bool>("RememberPasswords");
                ShowLauncherWindow    = root["ShowLauncherWindow"]   != null ? root.Value<bool>("ShowLauncherWindow")   : true;
                UseSmartAlwaysOnTop   = root.Value<bool>("UseSmartAlwaysOnTop");
                ForceGameSize         = root.Value<bool>("ForceGameSize");
                GameSizeWidth         = root["GameSizeWidth"]        != null ? root.Value<int>("GameSizeWidth")         : 800;
                GameSizeHeight        = root["GameSizeHeight"]       != null ? root.Value<int>("GameSizeHeight")        : 600;
                ShowIn                = root["ShowIn"]?.ToString()   ?? "Taskbar";
                EnableRemoteControl   = root.Value<bool>("EnableRemoteControl");
                DefaultClientPriority = root["DefaultClientPriority"]?.ToString() ?? "Normal";
                Opacity               = root["Opacity"]              != null ? root.Value<int>("Opacity")               : 100;
                logger.Debug("TechnicalConfig loaded from {0}", path);
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load TechnicalConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Technical");
            try
            {
                var root = new JObject
                {
                    ["UseSmartCpuReduction"]  = UseSmartCpuReduction,
                    ["RememberPasswords"]     = RememberPasswords,
                    ["ShowLauncherWindow"]    = ShowLauncherWindow,
                    ["UseSmartAlwaysOnTop"]   = UseSmartAlwaysOnTop,
                    ["ForceGameSize"]         = ForceGameSize,
                    ["GameSizeWidth"]         = GameSizeWidth,
                    ["GameSizeHeight"]        = GameSizeHeight,
                    ["ShowIn"]                = ShowIn,
                    ["EnableRemoteControl"]   = EnableRemoteControl,
                    ["DefaultClientPriority"] = DefaultClientPriority,
                    ["Opacity"]               = Opacity
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("TechnicalConfig saved to {0}", path);
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save TechnicalConfig"); }
        }
    }
}
