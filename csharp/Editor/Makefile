# Main Makefile - detects OS and includes platform-specific makefile

APP=PythonEditor.exe
SOLUTION=PythonEditor.sln

# All C# source files
SRC=$(wildcard *.cs)

# Detect OS and include platform-specific makefile
ifeq ($(OS),Windows_NT)
    include Makefile.windows
else
    include Makefile.linux
endif
