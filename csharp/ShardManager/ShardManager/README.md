# ShardManager

A GTK3 / GtkSharp settings and server-selector front-end for
[RazorEnhanced](https://github.com/RazorEnhanced/RazorEnhanced), targeting
**.NET Framework 4.7** on Windows (developed under MSYS2).

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Architecture Overview](#architecture-overview)
3. [Config Layer](#config-layer)
4. [UI Layer](#ui-layer)
   - [GtkWindowBase](#gtkwindowbase)
   - [TabBase and Lazy Population](#tabbase-and-lazy-population)
   - [Sub-tabs](#sub-tabs)
5. [Custom Event Loop](#custom-event-loop)
6. [Glade / UI File Conventions](#glade--ui-file-conventions)
7. [Profile System](#profile-system)
8. [Data Model (Server.shards)](#data-model-servershards)
9. [Building (MSYS2 / MSBuild)](#building-msys2--msbuild)
10. [Known GTK / GtkSharp Quirks](#known-gtk--gtksharp-quirks)

---

## Project Structure

```
ShardManager/
├── ShardManager.csproj
├── src/
│   ├── Program.cs                        # Entry point
│   ├── Config/
│   │   ├── ServerData.cs                 # ServerConfig singleton + data model
│   │   ├── ProfileManager.cs             # Profile directory management singleton
│   │   ├── TechnicalConfig.cs            # Technical tab settings singleton
│   │   ├── OptionsConfig.cs
│   │   ├── ToolbarsConfig.cs
│   │   ├── AdvancedConfig.cs
│   │   ├── HotKeysConfig.cs
│   │   ├── SkillsConfig.cs
│   │   ├── FiltersConfig.cs
│   │   ├── ScriptingConfig.cs
│   │   ├── MacrosConfig.cs
│   │   └── AgentsConfig.cs
│   └── UI/
│       ├── CustomMainLoop.cs             # Custom GTK event loop with invoke queue
│       ├── GtkWindowBase.cs              # Abstract base for all top-level windows
│       ├── ShardSelector.cs              # Server selection / main window
│       └── Settings/
│           ├── SettingsWindow.cs         # Settings host window + notebook
│           ├── TabBase.cs                # Abstract base for all tab panels
│           └── Tabs/
│               ├── OptionsTab.cs
│               ├── FiltersTab.cs
│               ├── ScriptingTab.cs
│               ├── MacrosTab.cs
│               ├── AgentsTab.cs
│               ├── ToolbarsTab.cs
│               ├── SkillsTab.cs
│               ├── HotKeysTab.cs
│               ├── AdvancedTab.cs
│               ├── TechnicalTab.cs
│               ├── Technical/
│               │   ├── RazorSettingsSubTab.cs
│               │   └── HelpStatusSubTab.cs
│               ├── Toolbars/
│               │   ├── CounterStatBarSubTab.cs
│               │   └── SpellGridSubTab.cs
│               ├── Agents/
│               │   ├── AutolootSubTab.cs
│               │   ├── ScavengerSubTab.cs
│               │   └── ...
│               ├── Scripting/
│               │   ├── PythonSubTab.cs
│               │   ├── UosSubTab.cs
│               │   └── CSharpSubTab.cs
│               ├── Filters/
│               │   ├── VirtualSubTab.cs
│               │   ├── TargettingSubTab.cs
│               │   └── MiscSubTab.cs
│               └── Advanced/
│                   ├── VideoRecorderSubTab.cs
│                   ├── DpsMeterSubTab.cs
│                   └── ScreenShotsSubTab.cs
└── deployment/
    ├── Server.shards                     # Server / login / character data (JSON)
    ├── Profiles/
    │   └── Default/                      # Per-tab JSON config files
    │       ├── Technical.json
    │       ├── Options.json
    │       └── ...
    ├── SettingsWindow.ui                 # Real settings window
    ├── ShardSelector.ui                  # Real server selector window
    ├── OptionsTab.ui                     # Tab content (GtkWindow wrapper for Glade)
    ├── TechnicalTab.ui
    └── ...                               # One .ui per tab and sub-tab
```

---

## Architecture Overview

```
Program.Main
  └─ ProfileManager.Init()           // locate Profiles/ directory
  └─ new ShardSelector(shardsFile)   // loads ServerConfig singleton
        └─ GtkWindowBase()           // loads .ui, calls Setup()
  └─ mainWindow.Run()                // enters CustomMainLoop

  On "Launch" button:
  └─ new SettingsWindow()            // creates notebook, populates tab 0 eagerly
  └─ ShardSelector.window.Hide()
  └─ settingsWindow.Show()

  On notebook SwitchPage:
  └─ TabBase.EnsurePopulated()       // lazy: loads .ui, strips wrapper, packs root
        └─ OnPopulate(builder)       // subclass binds widgets and signals
```

The application has **two top-level windows**: the server selector (`ShardSelector`)
and the settings window (`SettingsWindow`). Switching from selector to settings
hides the former rather than destroying it.

---

## Config Layer

### Singleton Pattern

Every config class follows the same pattern — private constructor, thread-safe
lazy `Instance` property, explicit `Load()` / `Save()` methods:

```csharp
public class TechnicalConfig
{
    private static TechnicalConfig _instance;
    private static readonly object _lock = new object();

    public static TechnicalConfig Instance
    {
        get { lock (_lock) { return _instance ?? (_instance = new TechnicalConfig()); } }
    }

    private TechnicalConfig() { }

    public void Load() { /* reads from ProfileManager.Instance.GetConfigPath("Technical") */ }
    public void Save() { /* writes to same path */ }
}
```

`ServerConfig` is the exception — it must be loaded with an explicit file path, so
`Instance` throws if `Load(path)` has not been called first.

### Config Hierarchy

```
ServerConfig          — Server.shards (global, outside profiles)
ProfileManager        — manages Profiles/ directory (singleton)
TechnicalConfig  \
OptionsConfig     |
ToolbarsConfig    |— one per tab, stored in Profiles/<active>/TabName.json
AdvancedConfig    |
...              /
```

Each tab config calls `ProfileManager.Instance.GetConfigPath("TabName")` to
resolve its file path, so switching the active profile transparently redirects
all subsequent `Load()` / `Save()` calls.

---

## UI Layer

### GtkWindowBase

`GtkWindowBase` is the abstract base for every top-level GTK window. It:

1. Wires `AppDomain.AssemblyResolve` so native GTK DLLs are found next to the exe.
2. Initialises GTK via `CustomMainLoop.Instance.Init()` (idempotent).
3. Loads the `.ui` file — first from the executable directory, then from
   embedded resources — via `Gtk.Builder`.
4. Retrieves the named `GtkWindow` object and calls the abstract `Setup(builder)`
   for the subclass to bind widgets.

```
GtkWindowBase  (abstract)
├── ShardSelector        — server/login selector, main application entry point
└── SettingsWindow       — tabbed settings host
```

### TabBase and Lazy Population

`TabBase` is the abstract base for every settings tab panel. Each tab:

- Owns a `Box` container widget (`ContainerWidget`) that is added to the
  `SettingsWindow` notebook immediately — but empty.
- Loads its `.ui` file and builds its content only on **first activation**
  (`EnsurePopulated()`), driven by the notebook's `SwitchPage` signal.
- Handles the **Glade wrapper window** automatically (see below).

```csharp
public abstract class TabBase
{
    public abstract string TabName  { get; }
    protected abstract string UiFile { get; }
    protected abstract void OnPopulate(Builder builder);

    public Widget ContainerWidget { get; }   // always present; content packed in lazily
    public void EnsurePopulated() { ... }    // called by SettingsWindow.SwitchPage
}
```

Inheritance tree:

```
TabBase  (abstract)
├── OptionsTab
├── FiltersTab
├── ScriptingTab
├── MacrosTab
├── AgentsTab
├── ToolbarsTab        — also manages a sub-notebook; populates sub-tab 0 eagerly
├── SkillsTab
├── HotKeysTab
├── AdvancedTab        — same sub-notebook pattern
└── TechnicalTab       — same sub-notebook pattern

Sub-tabs also derive from TabBase:
├── RazorSettingsSubTab
├── HelpStatusSubTab
├── CounterStatBarSubTab
├── SpellGridSubTab
├── AutolootSubTab
└── ... (all agent, scripting, filter, advanced sub-tabs)
```

**SettingsWindow** wires lazy loading like this:

```csharp
_notebook.SwitchPage += (s, e) => PopulateTab((int)e.PageNum);
PopulateTab(0);   // populate the first visible tab immediately in Setup()

void PopulateTab(int index)
{
    if (_loaded.Contains(index)) return;
    _loaded.Add(index);
    _tabs[index].EnsurePopulated();
}
```

Tabs that contain a sub-notebook (`TechnicalTab`, `ToolbarsTab`, `AdvancedTab`,
`AgentsTab`) replicate the same pattern one level deeper for their sub-tabs.

### Sub-tabs

A tab that hosts sub-tabs:

1. Declares `List<TabBase> _subTabs` in its `OnPopulate()`.
2. Gets its sub-notebook from the builder (`tech_subnotebook`, etc.).
3. Appends each sub-tab's `ContainerWidget` as a notebook page.
4. Wires `SwitchPage` to `PopulateSubTab()` and immediately populates sub-tab 0.

Sub-tabs are identical to top-level tabs from `TabBase`'s perspective — the same
`UiFile` / `OnPopulate` contract applies.

---

## Custom Event Loop

GTK's `Gtk.Application.Run()` is replaced by `CustomMainLoop`, a singleton that:

- Runs its own `while (running)` loop calling `GLib.MainContext.Iteration(true)`.
- Maintains a thread-safe `Queue<Action>` so background threads can marshal work
  to the UI thread via `CustomMainLoop.Instance.Invoke(action)`.
- Uses a **platform-specific wake mechanism** (`IWakeMechanism`) to interrupt the
  blocking `Iteration()` call when a new action is enqueued.

```
IWakeMechanism
├── UnixPipeWake      — self-pipe pattern; GLib.IOChannel watch fires on pipe read
└── WindowsEventWake  — ManualResetEvent polled via GLib.Timeout every 10 ms
```

The platform is detected at construction time from `Environment.OSVersion.Platform`.

Usage pattern (replaces standard GTK calls):

| Standard GTK              | CustomMainLoop equivalent             |
|---------------------------|---------------------------------------|
| `Gtk.Application.Init()`  | `CustomMainLoop.Instance.Init()`      |
| `Gtk.Application.Run()`   | `CustomMainLoop.Instance.Run()`       |
| `Gtk.Application.Quit()`  | `CustomMainLoop.Instance.Quit()`      |
| `Gtk.Application.Invoke`  | `CustomMainLoop.Instance.Invoke(...)` |

`Init()` is idempotent and is called from `GtkWindowBase`'s constructor, so it is
always safe to call before the first window is shown.

---

## Glade / UI File Conventions

UI files are edited in **Glade** and live in `deployment/`. Every tab and sub-tab
`.ui` file follows these rules so Glade and the runtime both work:

### 1. GtkWindow wrapper (Glade compatibility)

Glade requires a top-level `GtkWindow` to navigate the widget tree. Tab content
files therefore wrap their real root widget in a `GtkWindow`:

```xml
<interface>
  <requires lib="gtk+" version="3.0"/>
  <object class="GtkWindow" id="wnd_options_tab">
    <property name="visible">False</property>   <!-- MUST be False -->
    <property name="title">OptionsTab</property>
    <child>
      <object class="GtkBox" id="tab_root">     <!-- real root; always id="tab_root" -->
        ...
      </object>
    </child>
  </object>
</interface>
```

At runtime, `TabBase.EnsurePopulated()` strips the wrapper automatically:

```csharp
if (root.Parent is Gtk.Window wrapperWin)
{
    wrapperWin.Hide();    // release GDK resources before unparenting
    root.Unparent();      // detach real content from the wrapper
}
_container.PackStart(root, true, true, 0);
```

The wrapper is **never destroyed** by application code — the builder manages its
lifetime and frees it when garbage collected.

### 2. `visible=False` on the wrapper window

The wrapper `GtkWindow` **must** have `<property name="visible">False</property>`.
If it is `True`, GtkBuilder maps it to the screen the moment `AddFromFile` runs,
causing a blank window to flash before `Hide()` is called.  Real windows
(`SettingsWindow.ui`, `ShardSelector.ui`) keep `visible=True`.

### 3. `tab_root` id convention

Every tab and sub-tab `.ui` file must have exactly one widget with
`id="tab_root"`. `TabBase` looks up this ID to extract the content.

### 4. GtkAdjustment placement

`GtkAdjustment` objects referenced by `GtkScale` widgets must be declared as
**top-level `<object>` elements** inside `<interface>`, not nested inside a
widget's `<child>` block:

```xml
<interface>
  <requires lib="gtk+" version="3.0"/>
  <!-- Correct: top-level object -->
  <object class="GtkAdjustment" id="adj_opacity">
    <property name="lower">10</property>
    <property name="upper">100</property>
    <property name="value">100</property>
    <property name="step-increment">1</property>
    <property name="page-increment">10</property>
  </object>
  <object class="GtkWindow" id="wnd_razor_settings_sub_tab">
    ...
    <object class="GtkScale" id="scale_opacity">
      <property name="adjustment">adj_opacity</property>  <!-- reference by id -->
    </object>
  </object>
</interface>
```

Placing a `GtkAdjustment` inside a `<child>` block of a `GtkWindow` causes an
`AccessViolationException` on GtkSharp / .NET Framework / Windows (see
[Known Quirks](#known-gtk--gtksharp-quirks)).

---

## Profile System

Profiles allow per-character or per-shard configuration sets.

```
<dir containing Server.shards>/
└── Profiles/
    ├── Default/
    │   ├── Technical.json
    │   ├── Options.json
    │   └── ...
    └── PvP/
        ├── Technical.json
        └── ...
```

- **Discovery**: profile list = subdirectory names under `Profiles/`. No manifest
  file is needed.
- **Default profile**: always created on first run by `ProfileManager.Init()`.
- **Active profile**: stored in `ProfileManager.CurrentProfile` (runtime only;
  the active profile for a character is stored in `Server.shards` via
  `CharacterEntry.Profile`).
- **Path resolution**: all tab configs call
  `ProfileManager.Instance.GetConfigPath("TabName")` which returns
  `Profiles/<active>/TabName.json`.

`ProfileManager` operations (create, delete, rename, clone) manipulate directories
directly; no JSON manifest is read or written.

---

## Data Model (Server.shards)

`Server.shards` is a JSON file containing connection info, login credentials, and
character profiles:

```json
{
  "AutoLogin": false,
  "ShowLauncher": true,
  "SelectedServer": "Atlantic",
  "Servers": {
    "Atlantic": {
      "Host": "login.ultimaonline.com",
      "Port": 2593,
      "PatchEnc": true,
      "OSIEnc": false,
      "ClientPath": "C:/UO/client.exe",
      "ClientFolder": "C:/UO",
      "CUOClient": "",
      "SelectedLogin": "myuser@email.com",
      "Logins": [
        {
          "Username": "myuser@email.com",
          "Password": "<encrypted>",
          "Shards": [
            {
              "Name": "Atlantic",
              "Characters": [
                { "Name": "MyWarrior", "Profile": "Default" },
                { "Name": "MyMage",    "Profile": "PvP" }
              ]
            }
          ]
        }
      ]
    }
  }
}
```

Class hierarchy (all in `Config` namespace):

```
ServerConfig          (singleton; loaded from Server.shards)
└── Dictionary<string, ServerEntry>
    └── ServerEntry
        └── List<LoginEntry>
            └── LoginEntry
                └── List<ShardEntry>
                    └── ShardEntry
                        └── List<CharacterEntry>
                            └── CharacterEntry { Name, Profile }
```

---

## Building (MSYS2 / MSBuild)

A `Makefile.windows` is provided for MSYS2 shell builds.

**Key points:**

- `msbuild` is a shell alias in MSYS2, not a PATH binary. The Makefile uses the
  full path:
  ```makefile
  MSBUILD = /c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe
  ```

- MSYS2 converts leading `/` in arguments to filesystem paths (e.g. `/t:Build`
  becomes `t:Build`).  All MSBuild switches use the `-` prefix:
  ```makefile
  "$(MSBUILD)" ShardManager.csproj -t:Build -p:Configuration=Release
  ```

- To print the Unix-style output path after a build:
  ```makefile
  @echo "Unix path: $(shell cygpath -u '$(RELEASE_DIR)\$(APP)')"
  ```

---

## Known GTK / GtkSharp Quirks

### AccessViolationException from GtkAdjustment in child blocks

**Symptom:** `AccessViolationException` in `Gtk.Builder.AddFromFile` when a
`.ui` file contains `GtkAdjustment` as a nested object inside a container's
`<child>` block.

**Cause:** On .NET Framework / Windows, GtkSharp's XML parser triggers
undefined behaviour when `GtkAdjustment` (a non-widget `GObject`) appears in a
position the GTK type system treats as a widget slot.

**Fix:** Declare `GtkAdjustment` as a top-level `<object>` in `<interface>` and
reference it by ID string in the scale's `adjustment` property (see
[GtkAdjustment placement](#4-gtkadjustment-placement) above).

### Wrapper window flash

**Symptom:** Each tab shows a blank top-level window briefly when first activated.

**Cause:** GtkBuilder processes `<property name="visible">True</property>` on the
GtkWindow wrapper synchronously inside `AddFromFile`, mapping the window to the
screen before application code can hide it.

**Fix:** Set `<property name="visible">False</property>` on every wrapper
GtkWindow in tab `.ui` files.

### GTK_IS_WIDGET assertion failures after Destroy()

**Symptom:** `gtk_widget_get_preferred_height: assertion 'GTK_IS_WIDGET' failed`
followed by `AccessViolationException` when calling `gtk_widget_destroy` on the
wrapper window after unparenting its child.

**Cause:** Destroying a realized `GtkWindow` (even after `Unparent`) can
invalidate GDK sub-windows that extracted widgets still reference internally.

**Fix:** Call `Hide()` on the wrapper before `Unparent()` to release its GDK
resources, and **do not call `Destroy()`** — let the builder's GC finalizer
manage the wrapper's lifetime.
