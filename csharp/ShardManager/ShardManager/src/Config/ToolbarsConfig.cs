// ToolbarsConfig.cs  — singleton for Toolbars tab settings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class ToolbarsConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static ToolbarsConfig _instance;
        private static readonly object _lock = new object();

        public static ToolbarsConfig Instance
        {
            get { lock (_lock) { return _instance ?? (_instance = new ToolbarsConfig()); } }
        }

        private ToolbarsConfig() { }

        // ── Counter / Stat Bar ─────────────────────────────────────────────
        public bool   CounterBarOpen       { get; set; } = false;
        public bool   LockToolBar          { get; set; } = false;
        public bool   OpenOnLogin          { get; set; } = false;
        public int    PositionX            { get; set; } = 10;
        public int    PositionY            { get; set; } = 10;
        public int    Slots                { get; set; } = 2;
        /// <summary>"Vertical" or "Horizontal"</summary>
        public string LayoutStyle          { get; set; } = "Vertical";
        /// <summary>"Big", "Medium", "Small"</summary>
        public string LayoutSize           { get; set; } = "Big";
        public bool   ShowHits             { get; set; } = true;
        public bool   ShowStamina          { get; set; } = true;
        public bool   ShowMana             { get; set; } = true;
        public bool   ShowWeight           { get; set; } = true;
        public bool   ShowBar              { get; set; } = false;
        public bool   ShowTithe            { get; set; } = false;
        public int    CounterBarOpacity    { get; set; } = 100;

        // ── Spell Grid (placeholder) ───────────────────────────────────────
        // Fields to be added when that sub-tab is designed.

        // ── Persistence ────────────────────────────────────────────────────

        public void Load()
        {
            var path = ProfileManager.Instance.GetConfigPath("Toolbars");
            if (!File.Exists(path)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                CounterBarOpen    = root.Value<bool>("CounterBarOpen");
                LockToolBar       = root.Value<bool>("LockToolBar");
                OpenOnLogin       = root.Value<bool>("OpenOnLogin");
                PositionX         = root["PositionX"] != null ? root.Value<int>("PositionX") : 10;
                PositionY         = root["PositionY"] != null ? root.Value<int>("PositionY") : 10;
                Slots             = root["Slots"]     != null ? root.Value<int>("Slots")     : 2;
                LayoutStyle       = root["LayoutStyle"]?.ToString() ?? "Vertical";
                LayoutSize        = root["LayoutSize"]?.ToString()  ?? "Big";
                ShowHits          = root["ShowHits"]  != null ? root.Value<bool>("ShowHits")  : true;
                ShowStamina       = root["ShowStamina"] != null ? root.Value<bool>("ShowStamina") : true;
                ShowMana          = root["ShowMana"]  != null ? root.Value<bool>("ShowMana")  : true;
                ShowWeight        = root["ShowWeight"] != null ? root.Value<bool>("ShowWeight") : true;
                ShowBar           = root.Value<bool>("ShowBar");
                ShowTithe         = root.Value<bool>("ShowTithe");
                CounterBarOpacity = root["CounterBarOpacity"] != null ? root.Value<int>("CounterBarOpacity") : 100;
                logger.Debug("ToolbarsConfig loaded");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to load ToolbarsConfig"); }
        }

        public void Save()
        {
            var path = ProfileManager.Instance.GetConfigPath("Toolbars");
            try
            {
                var root = new JObject
                {
                    ["CounterBarOpen"]    = CounterBarOpen,
                    ["LockToolBar"]       = LockToolBar,
                    ["OpenOnLogin"]       = OpenOnLogin,
                    ["PositionX"]         = PositionX,
                    ["PositionY"]         = PositionY,
                    ["Slots"]             = Slots,
                    ["LayoutStyle"]       = LayoutStyle,
                    ["LayoutSize"]        = LayoutSize,
                    ["ShowHits"]          = ShowHits,
                    ["ShowStamina"]       = ShowStamina,
                    ["ShowMana"]          = ShowMana,
                    ["ShowWeight"]        = ShowWeight,
                    ["ShowBar"]           = ShowBar,
                    ["ShowTithe"]         = ShowTithe,
                    ["CounterBarOpacity"] = CounterBarOpacity
                };
                File.WriteAllText(path, root.ToString(Formatting.Indented));
                logger.Debug("ToolbarsConfig saved");
            }
            catch (Exception ex) { logger.Error(ex, "Failed to save ToolbarsConfig"); }
        }
    }
}
