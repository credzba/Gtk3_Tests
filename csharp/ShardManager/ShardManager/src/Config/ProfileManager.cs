// ProfileManager.cs
// Singleton that manages profile directories alongside Server.shards.
// Profiles are subdirectories of  <shardsDir>/Profiles/
// Each profile directory holds per-tab JSON config files.

using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace Config
{
    public class ProfileManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static ProfileManager _instance;
        private static readonly object _lock = new object();

        private string _baseDir;   // directory that contains Server.shards
        private string _currentProfile = "Default";

        // ── Singleton access ───────────────────────────────────────────────

        public static ProfileManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        throw new InvalidOperationException(
                            "ProfileManager not initialized. Call Init() first.");
                    return _instance;
                }
            }
        }

        public static ProfileManager Init(string shardsFilePath)
        {
            lock (_lock)
            {
                var pm = new ProfileManager();
                pm._baseDir = Path.GetDirectoryName(Path.GetFullPath(shardsFilePath));
                pm.EnsureDefaultProfile();
                _instance = pm;
                logger.Info("ProfileManager initialized. Base: {0}", pm._baseDir);
                return _instance;
            }
        }

        private ProfileManager() { }

        // ── Paths ──────────────────────────────────────────────────────────

        public string BaseDir        => _baseDir;
        public string ProfilesDir    => Path.Combine(_baseDir, "Profiles");
        public string CurrentProfile => _currentProfile;

        public string CurrentProfileDir =>
            Path.Combine(ProfilesDir, _currentProfile);

        /// <summary>Returns the full path for a tab's JSON file in the active profile.</summary>
        public string GetConfigPath(string tabName) =>
            Path.Combine(CurrentProfileDir, tabName + ".json");

        // ── Profile management ─────────────────────────────────────────────

        public IEnumerable<string> GetProfiles()
        {
            if (!Directory.Exists(ProfilesDir))
                return new[] { "Default" };

            var names = new List<string>();
            foreach (var dir in Directory.GetDirectories(ProfilesDir))
                names.Add(Path.GetFileName(dir));

            if (names.Count == 0) names.Add("Default");
            return names;
        }

        public void SetCurrentProfile(string name)
        {
            if (!Directory.Exists(Path.Combine(ProfilesDir, name)))
                throw new ArgumentException($"Profile '{name}' does not exist.");
            _currentProfile = name;
            logger.Info("Active profile: {0}", name);
        }

        public void CreateProfile(string name)
        {
            var dir = Path.Combine(ProfilesDir, name);
            if (Directory.Exists(dir))
                throw new InvalidOperationException($"Profile '{name}' already exists.");
            Directory.CreateDirectory(dir);
            logger.Info("Created profile: {0}", name);
        }

        public void DeleteProfile(string name)
        {
            if (string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot delete the Default profile.");
            var dir = Path.Combine(ProfilesDir, name);
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
            if (_currentProfile == name)
                _currentProfile = "Default";
            logger.Info("Deleted profile: {0}", name);
        }

        public void RenameProfile(string oldName, string newName)
        {
            var oldDir = Path.Combine(ProfilesDir, oldName);
            var newDir = Path.Combine(ProfilesDir, newName);
            Directory.Move(oldDir, newDir);
            if (_currentProfile == oldName)
                _currentProfile = newName;
            logger.Info("Renamed profile '{0}' -> '{1}'", oldName, newName);
        }

        public void CloneProfile(string sourceName, string newName)
        {
            var srcDir  = Path.Combine(ProfilesDir, sourceName);
            var destDir = Path.Combine(ProfilesDir, newName);
            CopyDirectory(srcDir, destDir);
            logger.Info("Cloned profile '{0}' -> '{1}'", sourceName, newName);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void EnsureDefaultProfile()
        {
            var defaultDir = Path.Combine(ProfilesDir, "Default");
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: false);
        }
    }
}
