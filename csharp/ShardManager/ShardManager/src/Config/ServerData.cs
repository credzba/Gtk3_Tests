// ServerData.cs - Data model for Server.shards JSON
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Config
{
    public class CharacterEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Profile")]
        public string Profile { get; set; }
    }

    public class ShardEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Characters")]
        public List<CharacterEntry> Characters { get; set; }

        public ShardEntry()
        {
            Characters = new List<CharacterEntry>();
        }
    }

    public class LoginEntry
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }  // encrypted

        [JsonProperty("Shards")]
        public List<ShardEntry> Shards { get; set; }

        public LoginEntry()
        {
            Shards = new List<ShardEntry>();
        }
    }

    public class ServerEntry
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

        [JsonProperty("SelectedLogin")]
        public string SelectedLogin { get; set; }

        [JsonProperty("Logins")]
        public List<LoginEntry> Logins { get; set; }

        public ServerEntry()
        {
            Logins = new List<LoginEntry>();
        }
    }

    public class ServerConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static ServerConfig instance;
        private static readonly object instanceLock = new object();

        private string filePath;

        public static ServerConfig Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                        throw new InvalidOperationException("ServerConfig not loaded. Call Load() first.");
                    return instance;
                }
            }
        }

        public bool AutoLogin { get; set; }
        public bool ShowLauncher { get; set; }
        public string SelectedServer { get; set; }
        public Dictionary<string, ServerEntry> Servers { get; private set; }

        private ServerConfig()
        {
            AutoLogin = false;
            Servers = new Dictionary<string, ServerEntry>();
        }

        public static ServerConfig Load(string path)
        {
            lock (instanceLock)
            {
                logger.Info("Loading server config: {0}", path);
                var cfg = new ServerConfig();
                cfg.filePath = path;
                cfg.LoadFromFile(path);
                instance = cfg;
                return instance;
            }
        }

        private void LoadFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                JObject root = JObject.Parse(json);

                AutoLogin      = root["AutoLogin"]    != null && (bool)root["AutoLogin"];
                ShowLauncher   = root["ShowLauncher"] != null && (bool)root["ShowLauncher"];
                SelectedServer = root["SelectedServer"]?.ToString() ?? "";

                JObject serversObj = root["Servers"] as JObject;
                if (serversObj != null)
                {
                    foreach (JProperty prop in serversObj.Properties())
                        Servers[prop.Name] = prop.Value.ToObject<ServerEntry>();
                }

                logger.Info("Loaded {0} servers", Servers.Count);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load server config");
            }
        }

        public void Save()
        {
            logger.Debug("Saving server config: {0}", filePath);
            try
            {
                JObject root = new JObject();
                root["AutoLogin"]      = AutoLogin;
                root["ShowLauncher"]   = ShowLauncher;
                root["SelectedServer"] = SelectedServer ?? "";

                JObject serversObj = new JObject();
                foreach (var kvp in Servers)
                    serversObj[kvp.Key] = JObject.FromObject(kvp.Value);
                root["Servers"] = serversObj;

                File.WriteAllText(filePath, root.ToString(Formatting.Indented));
                logger.Debug("Server config saved");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save server config");
            }
        }
    }
}
