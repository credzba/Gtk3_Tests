# Python Editor using GtkSourceView in C#

A simple Python editor built with C# using GTK# and GtkSourceView, featuring a custom event loop to avoid memory leaks.

## Features

- Python syntax highlighting
- Line numbers
- Auto-indentation
- Bracket matching
- Current line highlighting
- Undo/Redo support
- File operations (New, Open, Save, Save As)
- Run Python scripts directly from the editor (non-blocking, runs in background thread)
- Right margin indicator at 80 characters
- Tab width set to 4 spaces
- **Custom GTK main loop** - Prevents memory leaks from `Gtk.Application.Invoke()` calls

## Custom Main Loop

This project includes `CustomMainLoop.cs`, a custom GTK event loop implementation that:

- Eliminates memory leaks associated with `Gtk.Application.Invoke()` in GTK#
- Provides thread-safe UI updates via `CustomMainLoop.Instance.Invoke()`
- Uses platform-specific wake mechanisms (Unix pipes on Linux, Win32 events on Windows)
- Integrates seamlessly with GLib's event loop
- Includes comprehensive debug logging via NLog

The Python editor demonstrates proper usage:
- Background thread runs Python scripts without blocking the UI
- All UI updates from background threads use `CustomMainLoop.Instance.Invoke()`
- Clean shutdown handling

## Prerequisites

### Linux

```bash
# Ubuntu/Debian
sudo apt-get install gtk-sharp3 libgtksourceview-3.0-dev

# Fedora
sudo dnf install gtk-sharp3 gtksourceview3-devel

# Arch
sudo pacman -S gtk-sharp-3 gtksourceview3
```

### Windows

Install GTK3 runtime from: https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer/releases

## Building

### On Linux (using dotnet):

```bash
dotnet restore
dotnet build
dotnet run
```

### On Windows (using dotnet):

```bash
dotnet restore
dotnet build
dotnet run
```

### On Windows (using Visual Studio):

1. Open `PythonEditor.csproj` in Visual Studio
2. Restore NuGet packages
3. Build and Run (F5)

## Usage

1. **New File**: File → New or click the New button
2. **Open File**: File → Open or click the Open button
3. **Save File**: File → Save or click the Save button
4. **Run Code**: Click the Run button (saves and executes with python3)
5. **Undo/Redo**: Edit → Undo/Redo

## Notes

- The editor uses `python3` to run scripts. Make sure Python 3 is installed and in your PATH.
- The editor provides syntax highlighting for Python out of the box via GtkSourceView.
- Tab key inserts 4 spaces instead of tab characters.

## Customization

You can modify these settings in the code:

- Font: Change `sourceView.ModifyFont()` parameter
- Tab width: Change `sourceView.TabWidth`
- Line numbers: Toggle `sourceView.ShowLineNumbers`
- Right margin: Adjust `sourceView.RightMarginPosition`

## Using CustomMainLoop in Your Own Projects

The `CustomMainLoop.cs` file can be used in any GTK# project. Replace the standard GTK Application calls:

```csharp
// Old way (can cause memory leaks):
Gtk.Application.Init();
Gtk.Application.Invoke(delegate { /* UI update */ });
Gtk.Application.Run();
Gtk.Application.Quit();

// New way (with CustomMainLoop):
CustomMainLoop.Instance.Init();
CustomMainLoop.Instance.Invoke(() => { /* UI update */ });
CustomMainLoop.Instance.Run();
CustomMainLoop.Instance.Quit();
```

**Key Benefits:**
- Thread-safe UI updates without memory leaks
- Works on both Linux and Windows
- Debug logging support (requires NLog)
- Drop-in replacement for standard GTK Application methods

## Troubleshooting

If you encounter issues with GTK on Linux, ensure you have the GTK3 development libraries installed:

```bash
# Check GTK version
pkg-config --modversion gtk+-3.0

# Check GtkSourceView version
pkg-config --modversion gtksourceview-3.0
```

For Windows, make sure the GTK runtime bin directory is in your PATH environment variable.