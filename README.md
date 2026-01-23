# GTK3 Tests

A collection of GTK# 3.0 (C#/Mono) and GTK-rs (Rust) example applications and experiments for Linux, with multi-platform CI support.

## Repository Structure

`Gtk3_Tests/`
`├── csharp/ # C# GTK# 3.0 examples (Mono)`
`│ └── HelloWorld/ # Basic GTK# window with debugging setup`
`│ ├──  ...`
`├── rust/ # Rust GTK-rs examples`
`│ └── hello_world/ # Basic GTK-rs application`
`│ ├── src/`
`│ │ ├── main.rs`
`│ │ ├── main_window.rs`
`│ │ └── main_window.ui`
`│ └── ...`
`├── LICENSE`
`└── README.md`

## Themes

The application supports GTK themes. You can change the theme by setting the 'GTK_THEME' environment variable before running the application.
The default themes are 'Adwaita', 'HighContrast' and 'HighContrastInverted'.

New themes can be added in the {executabledirectory}/share/themes/ directory if you set the environment variable GTK_DATA_PREFIX=./  or in whatever standard places your operating system supports.


## Language-Specific Examples

### [C# GTK# Examples](./csharp/)
C# applications using GTK# 3.0 with Mono runtime:

- **[HelloWorld](./csharp/HelloWorld/)** - A simple GTK# application demonstrating basic window creation with scrolling UI and VSCode debugging setup.

### [Rust GTK-rs Examples](./rust/)
Rust applications using GTK-rs:

- **[hello_world](./rust/hello_world/)** - A basic GTK application written in Rust using GTK-rs with cross-platform build support.

## Requirements

### C# Examples
- **Mono 6.8+** with development tools
- **GTK# 3.0** libraries
- **VSCode** (optional, for debugging)
- **Make** (for using the provided Makefiles)

### Rust Examples
- **Rust 1.70+** and Cargo
- **GTK development libraries**:
  - **Linux**: `libgtk-3-dev`, `gtk-3-dev` or equivalent
  - **Windows**: MSVC build tools + GTK bundle
  - **macOS**: GTK via Homebrew (`brew install gtk+3`)
- **Make** (for cross-platform builds using Makefiles)

## Building

## Using Makefiles (Cross-Platform)
Each project has cross-platform Makefiles for consistent builds:

#### **C# Project:**

```
`cd csharp/HelloWorld`
`make -f Makefile.linux      # Linux build`
`make -f Makefile.windows    # Windows build (with Mono)`
`make                        # Uses system-appropriate Makefile
```

`

#### Rust Project:

```
`cd rust/hello_world`
`make -f Makefile.linux      # Linux build`
`make -f Makefile.windows    # Windows cross-compile setup`
`make                        # Default build
```

`

#### Direct Compilation

C# with Mono:

```
cd csharp/HelloWorld
mcs -out:GtkHelloScroll.exe -target:winexe -r:gtk-sharp-3.0 *.cs
```

Rust with Cargo:

```
cd rust/hello_world
cargo build                 # Debug build
cargo build --release       # Release build
cargo run                   # Build and run
```

#### Continuous Integration

This repository uses GitHub Actions for automated testing:

##### C# Workflow (.github/workflows/csharp-build.yml)

​    Builds C# projects on Ubuntu, Windows, and macOS

​    Runs mcs compilation tests

​    Validates Mono/GTK# compatibility across platforms

##### Rust Workflow (.github/workflows/rust-build.yml)

​    Builds Rust projects on Ubuntu, Windows, and macOS

​    Runs cargo test for each project

​    Checks formatting with cargo fmt --check

​    Runs lints with cargo clippy

#### Debugging

C# with VSCode
Projects include VSCode workspace configurations:

```
cd csharp/HelloWorld
code HelloWorld.code-workspace
```

Rust with VSCode
Install the Rust Analyzer extension and use the provided workspace:

```
cd rust/hello_world
code hello_world.code-workspace
```

#### License

See LICENSE for terms.

#### Contributing

​    Place C# examples in the csharp/ directory
​    Place Rust examples in the rust/ directory
​    Include appropriate Makefile.linux and Makefile.windows for cross-platform builds
​    Add VSCode workspace files for easy development setup
​    Update this README with your project details
​    Ensure GitHub Actions workflows pass for your changes
