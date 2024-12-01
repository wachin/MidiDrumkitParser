#!/bin/bash

echo "Verificando si .NET 9 está instalado..."

# Verificar si el comando dotnet está disponible
if command -v dotnet &> /dev/null
then
    # Verificar la versión instalada de .NET
    INSTALLED_VERSION=$(dotnet --version)
    REQUIRED_VERSION="9.0"
    if [[ $INSTALLED_VERSION == $REQUIRED_VERSION* ]]
    then
        echo ".NET SDK $INSTALLED_VERSION ya está instalado. No es necesario instalar .NET 9."
        exit 0
    else
        echo "Se encontró .NET SDK $INSTALLED_VERSION, pero se requiere .NET 9. Procediendo con la instalación..."
    fi
else
    echo ".NET no está instalado. Procediendo con la instalación de .NET 9..."
fi

# Agregar clave GPG de Microsoft y el repositorio de paquetes
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Actualizar repositorios e instalar .NET SDK 9.0
sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Verificar instalación nuevamente
if command -v dotnet &> /dev/null
then
    echo ".NET SDK 9.0 instalado correctamente."
else
    echo "Error: No se pudo instalar .NET SDK 9.0."
    exit 1
fi
