APP=GtkHelloScroll.exe
#
# Detect OS
ifeq ($(OS),Windows_NT)
    # Windows paths
    CP=copy
    RM=del /Q
else
    # Linux/Unix
    CP=cp
    RM=rm -f
endif
#
# All C# source files
SRC=$(wildcard *.cs)
# All UI files
UI_FILES=$(wildcard *.ui)
# Embedded resources for embed target
RESOURCES=$(foreach ui,$(UI_FILES),-resource:$(ui),GtkHelloScroll.$(ui))

# Default target: regular build (release)
all: $(APP)

# Build with external UI files (development/release)
$(APP): $(SRC) $(UI_FILES)
	mcs -sdk:4.5 -pkg:gtk-sharp-3.0 -out:$(APP) $(SRC)
	@for ui in $(UI_FILES); do \
		src=$$(readlink -f "$$ui"); \
		dst=$$(readlink -f "./$$ui"); \
		if [ "$$src" != "$$dst" ]; then \
			cp "$$ui" .; \
		fi \
	done
	@echo "Build complete."

# Embedded build (single EXE with UI resources)
embed: $(SRC) $(UI_FILES)
	mcs -sdk:4.5 -pkg:gtk-sharp-3.0 $(RESOURCES) -out:$(APP) $(SRC)
	@echo "Embedded build complete."

# Debug build (external UI, with -debug)
# Detect OS
ifeq ($(OS),Windows_NT)
    # Windows paths
    CP=copy
    RM=del /Q
else
    # Linux/Unix
    CP=cp
    RM=rm -f
endif

# ... then in your targets:
debug: $(SRC) $(UI_FILES)
	mcs -debug -sdk:4.5 -pkg:gtk-sharp-3.0 -out:$(APP) $(SRC)
ifeq ($(OS),Windows_NT)
	@for %%f in ($(UI_FILES)) do copy %%f .
else
	@for ui in $(UI_FILES); do \
		src=$$(readlink -f "$$ui"); \
		dst=$$(readlink -f "./$$ui"); \
		if [ "$$src" != "$$dst" ]; then \
			cp "$$ui" .; \
		fi \
	done
endif
	@echo "Debug build complete."
	@echo "Debug build complete. Run with:"
	@echo "  mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 $(APP)"

# Debug run (builds, then runs with debugger agent)
debug-run: debug
	@mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555,suspend=y $(APP) & \
	MONO_PID=$$! ; \
	sleep 0.5 ; \
	echo "DEBUGGER_READY" ; \
	wait $$MONO_PID

# Clean build artifacts
clean:
	rm -f $(APP) $(APP).mdb

.PHONY: all embed debug debug-run clean