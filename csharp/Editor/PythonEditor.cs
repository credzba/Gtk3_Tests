using System;
using Gtk;
using GtkSource;
using NLog;

namespace PythonEditor
{
    class PythonEditorWindow : Window
    {
        private GtkSource.SourceView sourceView;
        private GtkSource.Buffer sourceBuffer;
        private Label statusLabel;
        private string currentFile = null;

        public PythonEditorWindow() : base("Python Editor")
        {
            SetDefaultSize(800, 600);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent;

            // Create vertical box for layout
            VBox vbox = new VBox(false, 0);

            // Create menu bar
            MenuBar menuBar = CreateMenuBar();
            vbox.PackStart(menuBar, false, false, 0);

            // Create toolbar
            Toolbar toolbar = CreateToolbar();
            vbox.PackStart(toolbar, false, false, 0);

            // Create source view with scrolled window
            ScrolledWindow scrolled = new ScrolledWindow();
            scrolled.ShadowType = ShadowType.In;

            // Initialize GtkSourceView
            var langManager = GtkSource.LanguageManager.Default;
            var pythonLang = langManager.GetLanguage("python");

            sourceBuffer = new GtkSource.Buffer(new Gtk.TextTagTable());         
            sourceBuffer.Language = pythonLang;
            sourceBuffer.HighlightSyntax = true;
            sourceBuffer.HighlightMatchingBrackets = true;

            sourceView = new GtkSource.SourceView(sourceBuffer);
            sourceView.ShowLineNumbers = true;
            sourceView.AutoIndent = true;
            sourceView.IndentOnTab = true;
            sourceView.TabWidth = 4;
            sourceView.InsertSpacesInsteadOfTabs = true;
            sourceView.HighlightCurrentLine = true;
            sourceView.ShowRightMargin = true;
            sourceView.RightMarginPosition = 80;

            // Set a nice monospace font
            sourceView.ModifyFont(Pango.FontDescription.FromString("Monospace 10"));

            scrolled.Add(sourceView);
            vbox.PackStart(scrolled, true, true, 0);

            // Create status bar
            statusLabel = new Label("Ready");
            statusLabel.Xalign = 0;
            var statusBox = new HBox(false, 0);
            statusBox.PackStart(statusLabel, false, false, 5);
            vbox.PackStart(statusBox, false, false, 2);

            Add(vbox);
            ShowAll();

            // Set initial content
            sourceBuffer.Text = "# Python Editor Sample\n# Start coding here!\n\ndef hello_world():\n    print(\"Hello, World!\")\n\nif __name__ == \"__main__\":\n    hello_world()\n";
        }

        private MenuBar CreateMenuBar()
        {
            MenuBar menuBar = new MenuBar();

            // File menu
            Menu fileMenu = new Menu();
            MenuItem fileMenuItem = new MenuItem("File");
            fileMenuItem.Submenu = fileMenu;

            MenuItem newItem = new MenuItem("New");
            newItem.Activated += OnNew;
            fileMenu.Append(newItem);

            MenuItem openItem = new MenuItem("Open");
            openItem.Activated += OnOpen;
            fileMenu.Append(openItem);

            MenuItem saveItem = new MenuItem("Save");
            saveItem.Activated += OnSave;
            fileMenu.Append(saveItem);

            MenuItem saveAsItem = new MenuItem("Save As");
            saveAsItem.Activated += OnSaveAs;
            fileMenu.Append(saveAsItem);

            fileMenu.Append(new SeparatorMenuItem());

            MenuItem quitItem = new MenuItem("Quit");
            quitItem.Activated += OnQuit;
            fileMenu.Append(quitItem);

            menuBar.Append(fileMenuItem);

            // Edit menu
            Menu editMenu = new Menu();
            MenuItem editMenuItem = new MenuItem("Edit");
            editMenuItem.Submenu = editMenu;

            MenuItem undoItem = new MenuItem("Undo");
            undoItem.Activated += (s, e) => { if (sourceBuffer.CanUndo) sourceBuffer.Undo(); };
            editMenu.Append(undoItem);

            MenuItem redoItem = new MenuItem("Redo");
            redoItem.Activated += (s, e) => { if (sourceBuffer.CanRedo) sourceBuffer.Redo(); };
            editMenu.Append(redoItem);

            menuBar.Append(editMenuItem);

            return menuBar;
        }

        private Toolbar CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            ToolButton newButton = new ToolButton(Stock.New);
            newButton.Clicked += OnNew;
            toolbar.Insert(newButton, -1);

            ToolButton openButton = new ToolButton(Stock.Open);
            openButton.Clicked += OnOpen;
            toolbar.Insert(openButton, -1);

            ToolButton saveButton = new ToolButton(Stock.Save);
            saveButton.Clicked += OnSave;
            toolbar.Insert(saveButton, -1);

            toolbar.Insert(new SeparatorToolItem(), -1);

            ToolButton runButton = new ToolButton(Stock.Execute);
            runButton.Label = "Run";
            runButton.Clicked += OnRun;
            toolbar.Insert(runButton, -1);

            return toolbar;
        }

        private void OnNew(object sender, EventArgs e)
        {
            if (ConfirmDiscard())
            {
                sourceBuffer.Text = "";
                currentFile = null;
                UpdateStatus("New file");
            }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (!ConfirmDiscard()) return;

            FileChooserDialog dialog = new FileChooserDialog(
                "Open File",
                this,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);

            FileFilter filter = new FileFilter();
            filter.Name = "Python files";
            filter.AddPattern("*.py");
            dialog.AddFilter(filter);

            FileFilter allFilter = new FileFilter();
            allFilter.Name = "All files";
            allFilter.AddPattern("*");
            dialog.AddFilter(allFilter);

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    currentFile = dialog.Filename;
                    sourceBuffer.Text = System.IO.File.ReadAllText(currentFile);
                    UpdateStatus($"Opened: {currentFile}");
                }
                catch (Exception ex)
                {
                    ShowError($"Error opening file: {ex.Message}");
                }
            }

            dialog.Destroy();
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                OnSaveAs(sender, e);
            }
            else
            {
                SaveFile(currentFile);
            }
        }

        private void OnSaveAs(object sender, EventArgs e)
        {
            FileChooserDialog dialog = new FileChooserDialog(
                "Save File",
                this,
                FileChooserAction.Save,
                "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept);

            dialog.DoOverwriteConfirmation = true;

            FileFilter filter = new FileFilter();
            filter.Name = "Python files";
            filter.AddPattern("*.py");
            dialog.AddFilter(filter);

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                currentFile = dialog.Filename;
                if (!currentFile.EndsWith(".py"))
                {
                    currentFile += ".py";
                }
                SaveFile(currentFile);
            }

            dialog.Destroy();
        }

        public void SaveFile(string filename)
        {
            try
            {
                System.IO.File.WriteAllText(filename, sourceBuffer.Text);
                UpdateStatus($"Saved: {filename}");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving file: {ex.Message}");
            }
        }

        private void OnRun(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                ShowError("Please save the file first before running.");
                return;
            }

            SaveFile(currentFile);

            // Run Python in a background thread to avoid blocking the UI
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "python3";
                    process.StartInfo.Arguments = currentFile;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    
                    // Update UI from background thread using CustomMainLoop.Invoke
                    CustomMainLoop.Instance.Invoke(() => 
                        UpdateStatus("Running Python script..."));
                    
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string message = "";
                    if (!string.IsNullOrEmpty(output))
                        message += "Output:\n" + output;
                    if (!string.IsNullOrEmpty(error))
                        message += "\nErrors:\n" + error;

                    if (string.IsNullOrEmpty(message))
                        message = "Program executed successfully (no output)";

                    // Update UI from background thread using CustomMainLoop.Invoke
                    CustomMainLoop.Instance.Invoke(() => 
                    {
                        ShowInfo(message);
                        UpdateStatus("Program executed");
                    });
                }
                catch (Exception ex)
                {
                    // Update UI from background thread using CustomMainLoop.Invoke
                    CustomMainLoop.Instance.Invoke(() => 
                        ShowError($"Error running program: {ex.Message}"));
                }
            });
        }

        private bool ConfirmDiscard()
        {
            if (string.IsNullOrEmpty(sourceBuffer.Text))
                return true;

            MessageDialog dialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Question,
                ButtonsType.YesNo,
                "Discard current file?");

            ResponseType response = (ResponseType)dialog.Run();
            dialog.Destroy();

            return response == ResponseType.Yes;
        }

        private void ShowError(string message)
        {
            MessageDialog dialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                message);
            dialog.Run();
            dialog.Destroy();
        }

        private void ShowInfo(string message)
        {
            MessageDialog dialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Info,
                ButtonsType.Ok,
                message);
            dialog.Run();
            dialog.Destroy();
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }

        private void OnQuit(object sender, EventArgs e)
        {
            CustomMainLoop.Instance.Quit();
        }

        private void OnDeleteEvent(object sender, DeleteEventArgs e)
        {
            CustomMainLoop.Instance.Quit();
        }

        // Public API methods
        public void SetEditorText(string text)
        {
            sourceBuffer.Text = text;
        }

        public string GetEditorText()
        {
            return sourceBuffer.Text;
        }

        public void LoadFile(string filename)
        {
            currentFile = filename;
            sourceBuffer.Text = System.IO.File.ReadAllText(filename);
            UpdateStatus($"Loaded: {filename}");
        }
    }
}