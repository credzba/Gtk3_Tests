// ShardData.cs - Data model for UO_Copilot.shards JSON
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class ShardEntry
    {
        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ClientPath")]
        public string ClientPath { get; set; }

        [JsonProperty("CUOClient")]
        public string CUOClient { get; set; }

        [JsonProperty("ClientFolder")]
        public string ClientFolder { get; set; }

        [JsonProperty("Host")]
        public string Host { get; set; }

        [JsonProperty("Port")]
        public int Port { get; set; }

        [JsonProperty("PatchEnc")]
        public bool PatchEnc { get; set; }

        [JsonProperty("OSIEnc")]
        public bool OSIEnc { get; set; }

        [JsonProperty("Selected")]
        public bool Selected { get; set; }

        [JsonProperty("StartClientType")]
        public int StartClientType { get; set; }
    }

    public class ShardsFile
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Preserved in file but not shown in UI
        public bool AllowBeta { get; set; }
        public bool ShowLauncher { get; set; }

        // Key of the currently displayed shard
        public string SelectedShard { get; set; }

        // Key = shard name as it appears in the JSON object
        public Dictionary<string, ShardEntry> Shards { get; set; }

        public ShardsFile()
        {
            Shards = new Dictionary<string, ShardEntry>();
        }

        public static ShardsFile Load(string filePath)
        {
            logger.Info("Loading shards file: {0}", filePath);
            try
            {
                string json = File.ReadAllText(filePath);
                JObject root = JObject.Parse(json);

                ShardsFile result = new ShardsFile();
                result.AllowBeta    = root["AllowBeta"]    != null && (bool)root["AllowBeta"];
                result.ShowLauncher = root["ShowLauncher"] != null && (bool)root["ShowLauncher"];
                result.SelectedShard = root["SelectedShard"]?.ToString() ?? "";

                JObject shardsObj = root["Shards"] as JObject;
                if (shardsObj != null)
                {
                    foreach (JProperty prop in shardsObj.Properties())
                        result.Shards[prop.Name] = prop.Value.ToObject<ShardEntry>();
                }

                logger.Info("Loaded {0} shards", result.Shards.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load shards file");
                return new ShardsFile();
            }
        }

        public void Save(string filePath)
        {
            logger.Debug("Saving shards file: {0}", filePath);
            try
            {
                JObject root = new JObject();
                root["AllowBeta"]     = AllowBeta;
                root["ShowLauncher"]  = ShowLauncher;
                root["SelectedShard"] = SelectedShard ?? "";

                JObject shardsObj = new JObject();
                foreach (var kvp in Shards)
                    shardsObj[kvp.Key] = JObject.FromObject(kvp.Value);
                root["Shards"] = shardsObj;

                File.WriteAllText(filePath, root.ToString(Formatting.Indented));
                logger.Debug("Shards file saved");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save shards file");
            }
        }
    }
}
