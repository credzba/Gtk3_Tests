# Main Makefile - detects OS and includes platform-specific makefile

APP=ShardManager.exe
SOLUTION=ShardManager.sln


# All C# source files under src/
SRC=$(wildcard src/*.cs) $(wildcard src/Config/*.cs) $(wildcard src/UI/*.cs)
# All UI files under src/UI/
UI_FILES=$(wildcard src/UI/*.ui)

# Detect OS and include platform-specific makefile
ifeq ($(OS),Windows_NT)
    include Makefile.windows
else
    include Makefile.linux
endif
