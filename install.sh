#!/bin/bash

echo "Instalando dependencias necesarias para .NET 9..."

# Agregar clave GPG de Microsoft y el repositorio de paquetes
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Actualizar repositorios e instalar .NET SDK 9.0
sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Verificar instalación
if ! command -v dotnet &> /dev/null
then
    echo "Error: .NET SDK no se instaló correctamente."
    exit 1
fi

echo ".NET SDK 9.0 instalado correctamente. Puede compilar el proyecto con 'dotnet build'."

