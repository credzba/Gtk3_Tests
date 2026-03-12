# Main Makefile - detects OS and includes platform-specific makefile

APP=ShardManager.exe
SOLUTION=ShardManager.sln


# All C# source files under src/
SRC=$(wildcard ShardManager/src/*.cs) $(wildcard ShardManager/src/Config/*.cs) $(wildcard ShardManager/src/UI/*.cs)
# All UI files under ShardManager/src/UI/
UI_FILES=$(wildcard ShardManager/src/UI/*.ui)

# Detect OS and include platform-specific makefile
ifeq ($(OS),Windows_NT)
    include Makefile.windows
else
    include Makefile.linux
endif
