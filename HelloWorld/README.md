# HelloWorld - GTK# Scrolling Window Example

A basic GTK# 3.0 application demonstrating window management, scrolling containers, and VSCode debugging integration.

## Files

### Source Code
- **'Program.cs'** - Application entry point, initializes GTK and creates main window
- **'MainWindow.cs'** - Main window implementation with scrolled window and text view
- **'GtkWindowBase.cs'** - Base class for GTK windows with common functionality
- **'MainWindow.ui'** - Glade UI definition file for the main window layout

### Build Files
- **'Makefile'** - Build automation with targets for debug, release, and debugging
- **'GtkHelloScroll.csproj'** - C# project file for IDE integration
- **'HelloWorld.sln'** - Visual Studio solution file

## Building
'''bash
# Release build
make

# Debug build with symbols
make debug

# Run with Mono debugger agent (for VSCode)
make debug-run
'''

## Debugging in VSCode

This project is configured for F5 debugging:

1. Open the project folder in VSCode
2. Ensure '.vscode/tasks.json' and '.vscode/launch.json' are configured (see parent directory)
3. Press **F5** to build and start debugging
4. The debugger will automatically attach when the application prints "DEBUGGER_READY"

### How it works

The 'debug-run' Makefile target:
- Compiles with debug symbols ('-debug:portable')
- Starts Mono with debugger agent on port 55555
- Prints "DEBUGGER_READY" signal for VSCode
- VSCode detects the signal and attaches the debugger

## Running
'''bash
# After building
mono GtkHelloScroll.exe
'''

## Requirements

- Mono 6.8+
- GTK# 3.0 ('gtk-sharp-3.0')
- VSCode with Mono Debug extension (for debugging)

