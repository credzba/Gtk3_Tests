# Makefile for hello_world GTK3 Rust application
# Cross-platform wrapper for cargo commands

.PHONY: all build release run clean test package help

# Default target
all: build

# Development build
build:
	cargo build

# Release build (optimized)
release:
	cargo build --release

# Run the application
run:
	cargo run

# Run tests
test:
	cargo test

# Clean build artifacts
clean:
	cargo clean
	rm -rf dist
	rm -f *.tar.gz *.zip

# Package for distribution (platform-specific)
package: release
ifeq ($(OS),Windows_NT)
	$(MAKE) -f Makefile.windows package
else
	$(MAKE) -f Makefile.linux package
endif

# Install locally
install: release
	cargo install --path .

# Help target
help:
	@echo "Available targets:"
	@echo "  make build    - Build debug version"
	@echo "  make release  - Build optimized release version"
	@echo "  make run      - Run the application"
	@echo "  make test     - Run tests"
	@echo "  make clean    - Remove build artifacts"
	@echo "  make package  - Create distribution package"
	@echo "  make install  - Install to ~/.cargo/bin"
	@echo "  make help     - Show this help message"