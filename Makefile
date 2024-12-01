# Variables
PROJECT_NAME = MIDI_Drumkit_Parser

# Comandos principales
all: build

build:
	dotnet build -c Release

run: build
	dotnet run --project $(PROJECT_NAME)

clean:
	dotnet clean

install:
	./install.sh


