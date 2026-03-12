// ShardSelector.cs
using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using NLog;
using Config;

namespace UI
{
    public class ShardSelector : GtkWindowBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Data
        private ShardsFile shardsFile;
        private string shardsFilePath;
        private bool suppressChangeEvents = false;

        // Widget references
        private ComboBoxText shardCombo;
        private Label clientLocationLabel;
        private Label uoFolderLabel;
        private Label cuoClientLabel;
        private Entry serverAddressEntry;
        private Entry portEntry;
        private CheckButton patchEncCheck;
        private CheckButton osiEncCheck;

        public ShardSelector(string shardsFilePath)
            : base("ShardSelector.ui", "ShardManager.ShardSelector.ui")
        {
            this.shardsFilePath = shardsFilePath;
            shardsFile = ShardsFile.Load(shardsFilePath);

            // Populate shard combo and display initial selection
            PopulateShardCombo();
            SelectInitialShard();
        }

        protected override void Setup(Builder builder)
        {
            // Get widget references from builder
            shardCombo             = (ComboBoxText) builder.GetObject("shardCombo");
            clientLocationLabel    = (Label)        builder.GetObject("clientLocationLabel");
            uoFolderLabel          = (Label)        builder.GetObject("uoFolderLabel");
            cuoClientLabel         = (Label)        builder.GetObject("cuoClientLabel");
            serverAddressEntry     = (Entry)        builder.GetObject("serverAddressEntry");
            portEntry              = (Entry)        builder.GetObject("portEntry");
            patchEncCheck          = (CheckButton)  builder.GetObject("patchEncCheck");
            osiEncCheck            = (CheckButton)  builder.GetObject("osiEncCheck");

            var addButton               = (Button) builder.GetObject("addButton");
            var removeButton            = (Button) builder.GetObject("removeButton");
            var clientLocationPickerBtn = (Button) builder.GetObject("clientLocationPickerBtn");
            var uoFolderPickerBtn       = (Button) builder.GetObject("uoFolderPickerBtn");
            var cuoClientPickerBtn      = (Button) builder.GetObject("cuoClientPickerBtn");
            var launchButton            = (Button) builder.GetObject("launchButton");
            var launchCuoButton         = (Button) builder.GetObject("launchCuoButton");
            var checkUpdateButton       = (Button) builder.GetObject("checkUpdateButton");
            var exitButton              = (Button) builder.GetObject("exitButton");

            // Wire signals
            window.Destroyed                  += delegate { CustomMainLoop.Instance.Quit(); };
            shardCombo.Changed                += OnShardComboChanged;
            serverAddressEntry.Changed        += OnServerAddressChanged;
            portEntry.Changed                 += OnPortChanged;
            patchEncCheck.Toggled             += OnPatchEncToggled;
            osiEncCheck.Toggled               += OnOsiEncToggled;
            addButton.Clicked                 += OnAddClicked;
            removeButton.Clicked              += OnRemoveClicked;
            clientLocationPickerBtn.Clicked   += OnClientLocationPickerClicked;
            uoFolderPickerBtn.Clicked         += OnUoFolderPickerClicked;
            cuoClientPickerBtn.Clicked        += OnCuoClientPickerClicked;
            launchButton.Clicked              += OnLaunchClicked;
            launchCuoButton.Clicked           += OnLaunchCuoClicked;
            checkUpdateButton.Clicked         += OnCheckUpdateClicked;
            exitButton.Clicked                += delegate { CustomMainLoop.Instance.Quit(); };
        }

        // ── Data helpers ────────────────────────────────────────────────────────

        private void PopulateShardCombo()
        {
            suppressChangeEvents = true;
            shardCombo.RemoveAll();
            foreach (string name in shardsFile.Shards.Keys)
                shardCombo.AppendText(name);
            suppressChangeEvents = false;
        }

        private void SelectInitialShard()
        {
            // Prefer SelectedShard key, fall back to Selected flag, then first entry
            int idx = 0, i = 0;
            foreach (var kvp in shardsFile.Shards)
            {
                if (!string.IsNullOrEmpty(shardsFile.SelectedShard))
                {
                    if (kvp.Key == shardsFile.SelectedShard) { idx = i; break; }
                }
                else if (kvp.Value.Selected) { idx = i; break; }
                i++;
            }
            suppressChangeEvents = true;
            shardCombo.Active = idx;
            suppressChangeEvents = false;
            LoadShardIntoUI(CurrentShardName());
        }

        private string CurrentShardName() => shardCombo.ActiveText;

        private ShardEntry CurrentShard()
        {
            string name = CurrentShardName();
            if (name == null) return null;
            ShardEntry entry;
            shardsFile.Shards.TryGetValue(name, out entry);
            return entry;
        }

        private void LoadShardIntoUI(string shardName)
        {
            if (shardName == null) return;
            ShardEntry entry;
            if (!shardsFile.Shards.TryGetValue(shardName, out entry)) return;

            suppressChangeEvents = true;
            clientLocationLabel.Text = entry.ClientPath   ?? "";
            uoFolderLabel.Text       = entry.ClientFolder ?? "";
            cuoClientLabel.Text      = entry.CUOClient    ?? "";
            serverAddressEntry.Text  = entry.Host         ?? "";
            portEntry.Text           = entry.Port.ToString();
            patchEncCheck.Active     = entry.PatchEnc;
            osiEncCheck.Active       = entry.OSIEnc;
            suppressChangeEvents = false;
        }

        // ── Signal handlers ─────────────────────────────────────────────────────

        private void OnShardComboChanged(object sender, EventArgs e)
        {
            if (suppressChangeEvents) return;
            string name = CurrentShardName();
            logger.Debug("Shard changed to: {0}", name);
            shardsFile.SelectedShard = name ?? "";
            shardsFile.Save(shardsFilePath);
            LoadShardIntoUI(name);
        }

        private void OnServerAddressChanged(object sender, EventArgs e)
        {
            if (suppressChangeEvents) return;
            ShardEntry entry = CurrentShard();
            if (entry == null) return;
            entry.Host = serverAddressEntry.Text;
            shardsFile.Save(shardsFilePath);
        }

        private void OnPortChanged(object sender, EventArgs e)
        {
            if (suppressChangeEvents) return;
            ShardEntry entry = CurrentShard();
            if (entry == null) return;
            int port;
            if (int.TryParse(portEntry.Text, out port))
            {
                entry.Port = port;
                shardsFile.Save(shardsFilePath);
            }
        }

        private void OnPatchEncToggled(object sender, EventArgs e)
        {
            if (suppressChangeEvents) return;
            ShardEntry entry = CurrentShard();
            if (entry == null) return;
            entry.PatchEnc = patchEncCheck.Active;
            shardsFile.Save(shardsFilePath);
        }

        private void OnOsiEncToggled(object sender, EventArgs e)
        {
            if (suppressChangeEvents) return;
            ShardEntry entry = CurrentShard();
            if (entry == null) return;
            entry.OSIEnc = osiEncCheck.Active;
            shardsFile.Save(shardsFilePath);
        }

        private void OnClientLocationPickerClicked(object sender, EventArgs e)
        {
            string chosen = RunFileChooser(
                "Select client.exe",
                FileChooserAction.Open,
                currentPath: CurrentShard()?.ClientPath,
                filterName: "UO Client (client.exe)",
                filterPatterns: new[] { "client.exe", "Client.exe", "CLIENT.EXE" });

            if (chosen == null) return;

            ShardEntry entry = CurrentShard();
            entry.ClientPath = chosen;
            clientLocationLabel.Text = chosen;

            // Auto-set UO Folder to the directory of the selected client.exe
            string dir = Path.GetDirectoryName(chosen);
            if (string.IsNullOrEmpty(entry.ClientFolder) ||
                entry.ClientFolder == Path.GetDirectoryName(entry.ClientPath))
            {
                entry.ClientFolder = dir;
                uoFolderLabel.Text = dir;
            }

            shardsFile.Save(shardsFilePath);
        }

        private void OnUoFolderPickerClicked(object sender, EventArgs e)
        {
            string startDir = CurrentShard()?.ClientFolder;
            if (string.IsNullOrEmpty(startDir))
                startDir = Path.GetDirectoryName(CurrentShard()?.ClientPath ?? "");

            string chosen = RunFileChooser(
                "Select UO Folder",
                FileChooserAction.SelectFolder,
                currentPath: startDir,
                filterName: null,
                filterPatterns: null);

            if (chosen == null) return;

            ShardEntry entry = CurrentShard();
            entry.ClientFolder = chosen;
            uoFolderLabel.Text = chosen;
            shardsFile.Save(shardsFilePath);
        }

        private void OnCuoClientPickerClicked(object sender, EventArgs e)
        {
            string chosen = RunFileChooser(
                "Select CUO Client",
                FileChooserAction.Open,
                currentPath: CurrentShard()?.CUOClient,
                filterName: "CUO Executables (CUO.exe, ClassicUO.exe)",
                filterPatterns: new[] { "CUO.exe", "cuo.exe", "ClassicUO.exe", "classicuo.exe" });

            if (chosen == null) return;

            ShardEntry entry = CurrentShard();
            entry.CUOClient = chosen;
            cuoClientLabel.Text = chosen;
            shardsFile.Save(shardsFilePath);
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            string newName = RunInputDialog("Add Shard", "Enter new shard name:");
            if (string.IsNullOrWhiteSpace(newName)) return;

            if (shardsFile.Shards.ContainsKey(newName))
            {
                ShowMessage(MessageType.Warning, $"A shard named '{newName}' already exists.");
                return;
            }

            shardsFile.Shards[newName] = new ShardEntry
            {
                Description     = newName,
                ClientPath      = "",
                CUOClient       = "",
                ClientFolder    = "",
                Host            = "",
                Port            = 2593,
                PatchEnc        = true,
                OSIEnc          = false,
                Selected        = false,
                StartClientType = 0
            };
            shardsFile.Save(shardsFilePath);

            PopulateShardCombo();

            int idx = 0;
            foreach (string k in shardsFile.Shards.Keys)
            {
                if (k == newName) break;
                idx++;
            }
            suppressChangeEvents = true;
            shardCombo.Active = idx;
            suppressChangeEvents = false;
            LoadShardIntoUI(newName);
        }

        private void OnRemoveClicked(object sender, EventArgs e)
        {
            string name = CurrentShardName();
            if (name == null) return;

            var confirm = new MessageDialog(window, DialogFlags.Modal, MessageType.Question,
                ButtonsType.YesNo, $"Remove shard '{name}'?");
            confirm.Title = "Confirm Remove";
            var resp = (ResponseType)confirm.Run();
            confirm.Destroy();
            if (resp != ResponseType.Yes) return;

            shardsFile.Shards.Remove(name);
            shardsFile.Save(shardsFilePath);
            PopulateShardCombo();

            if (shardCombo.Model.IterNChildren() > 0)
            {
                suppressChangeEvents = true;
                shardCombo.Active = 0;
                suppressChangeEvents = false;
                LoadShardIntoUI(CurrentShardName());
            }
            else
            {
                suppressChangeEvents = true;
                clientLocationLabel.Text = "";
                uoFolderLabel.Text       = "";
                cuoClientLabel.Text      = "";
                serverAddressEntry.Text  = "";
                portEntry.Text           = "";
                patchEncCheck.Active     = false;
                osiEncCheck.Active       = false;
                suppressChangeEvents = false;
            }
        }

        private void OnLaunchClicked(object sender, EventArgs e)
        {
            logger.Info("Launch clicked (stub)");
            // TODO: implement launch
        }

        private void OnLaunchCuoClicked(object sender, EventArgs e)
        {
            logger.Info("Launch CUO clicked (stub)");
            // TODO: implement launch CUO
        }

        private void OnCheckUpdateClicked(object sender, EventArgs e)
        {
            logger.Info("Check Update clicked (stub)");
            // TODO: implement update check
        }

        // ── File chooser / dialog helpers ────────────────────────────────────────

        private string RunFileChooser(string title, FileChooserAction action,
                                      string currentPath, string filterName, string[] filterPatterns)
        {
            var acceptLabel = action == FileChooserAction.SelectFolder ? "Select" : "Open";
            var dialog = new FileChooserDialog(title, window, action,
                "Cancel", ResponseType.Cancel,
                acceptLabel, ResponseType.Accept);

            if (!string.IsNullOrEmpty(currentPath))
            {
                string dir = action == FileChooserAction.SelectFolder
                    ? currentPath
                    : Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    dialog.SetCurrentFolder(dir);
            }

            if (filterName != null && filterPatterns != null && action != FileChooserAction.SelectFolder)
            {
                var filter = new FileFilter { Name = filterName };
                foreach (string p in filterPatterns)
                    filter.AddPattern(p);
                dialog.AddFilter(filter);
            }

            string result = null;
            if ((ResponseType)dialog.Run() == ResponseType.Accept)
                result = dialog.Filename;
            dialog.Destroy();
            return result;
        }

        private string RunInputDialog(string title, string prompt)
        {
            var dlg = new Dialog(title, window,
                DialogFlags.Modal | DialogFlags.DestroyWithParent,
                "OK", ResponseType.Ok,
                "Cancel", ResponseType.Cancel);
            dlg.ContentArea.PackStart(new Label(prompt), false, false, 4);
            var entry = new Entry { ActivatesDefault = true };
            dlg.ContentArea.PackStart(entry, false, false, 4);
            dlg.DefaultResponse = Gtk.ResponseType.Ok;
            dlg.ShowAll();

            string result = null;
            if ((ResponseType)dlg.Run() == ResponseType.Ok)
                result = entry.Text.Trim();
            dlg.Destroy();
            return result;
        }

        private void ShowMessage(MessageType msgType, string text)
        {
            var md = new MessageDialog(window, DialogFlags.Modal, msgType, ButtonsType.Ok, text);
            md.Run();
            md.Destroy();
        }
    }
}
