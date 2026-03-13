# Main Makefile - detects OS and includes platform-specific makefile

APP=ShardManager.exe
SOLUTION=ShardManager.sln


# All C# source files under src/ (recursive via find)
SRC=$(shell find ShardManager/src -name '*.cs')
# All deployment UI files
UI_FILES=$(wildcard ShardManager/deployment/*.ui)

# Detect OS and include platform-specific makefile
# Check both the Windows env var (cmd/PowerShell) and uname -o (MSYS2/Cygwin)
UNAME_O := $(shell uname -o 2>/dev/null)
ifeq ($(OS),Windows_NT)
    include Makefile.windows
else ifeq ($(UNAME_O),Msys)
    include Makefile.windows
else ifeq ($(UNAME_O),Cygwin)
    include Makefile.windows
else
    include Makefile.linux
endif
