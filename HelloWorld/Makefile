# Main Makefile - detects OS and includes platform-specific makefile

APP=GtkHelloScroll.exe
SOLUTION=HelloWorld.sln
RESOURCES=$(foreach ui,$(UI_FILES),-resource:$(ui),GtkHelloScroll.$(ui))

# All C# source files
SRC=$(wildcard *.cs)
# All UI files
UI_FILES=$(wildcard *.ui)

# Detect OS and include platform-specific makefile
ifeq ($(OS),Windows_NT)
    include Makefile.windows
else
    include Makefile.linux
endif
