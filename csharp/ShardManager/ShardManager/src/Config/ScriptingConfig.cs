// ScriptingConfig.cs  — singleton for Scripting tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class ScriptingConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static ScriptingConfig _instance;
        private static readonly object _lock = new object();

        public static ScriptingConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new ScriptingConfig()); } }
        }

        private ScriptingConfig() { }

        // ── Script Info ────────────────────────────────────────────────────
        public bool LoopMode             { get; set; } = false;
        public bool WaitBeforeInterrupt  { get; set; } = false;
        public bool AutoStartAtLogin     { get; set; } = false;
        public bool Preload              { get; set; } = false;

        // ── Error/Message settings ─────────────────────────────────────────
        public bool LogScriptError          { get; set; } = false;
        public bool ShowScriptErrorMessage  { get; set; } = true;
        public bool ShowCharClientMessages  { get; set; } = false;

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Scripting");
            if (!File.Exists(path)) return;
            try
            {
                var r = JObject.Parse(File.ReadAllText(path));
                LoopMode            = r.Value<bool>("LoopMode");
                WaitBeforeInterrupt = r.Value<bool>("WaitBeforeInterrupt");
                AutoStartAtLogin    = r.Value<bool>("AutoStartAtLogin");
                Preload             = r.Value<bool>("Preload");
                LogScriptError      = r.Value<bool>("LogScriptError");
                ShowScriptErrorMessage = r["ShowScriptErrorMessage"] != null ? r.Value<bool>("ShowScriptErrorMessage") : true;
                ShowCharClientMessages = r.Value<bool>("ShowCharClientMessages");
                logger.Debug("ScriptingConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load ScriptingConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Scripting");
            try
            {
                var root = new JObject
                {
                    ["LoopMode"]               = LoopMode,
                    ["WaitBeforeInterrupt"]    = WaitBeforeInterrupt,
                    ["AutoStartAtLogin"]       = AutoStartAtLogin,
                    ["Preload"]                = Preload,
                    ["LogScriptError"]         = LogScriptError,
                    ["ShowScriptErrorMessage"] = ShowScriptErrorMessage,
                    ["ShowCharClientMessages"] = ShowCharClientMessages
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("ScriptingConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save ScriptingConfig"); }
        }
    }
}
