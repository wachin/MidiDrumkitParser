# Variables
PROJECT_NAME = MIDI_Drumkit_Parser

# Comandos principales
all: build

build:
	dotnet build $(PROJECT_NAME).csproj -c Release

run: build
	dotnet run --project $(PROJECT_NAME).csproj

clean:
	dotnet clean $(PROJECT_NAME).csproj

install:
	./install.sh
