#!/bin/sh
set -e

USER_ID=${PUID:-1000}
GROUP_ID=${PGID:-1000}

# Ajustar GID del grupo horizonte
CURRENT_GID=$(id -g horizonte 2>/dev/null || echo "")
if [ "$CURRENT_GID" != "$GROUP_ID" ]; then
    groupmod -o -g "$GROUP_ID" horizonte 2>/dev/null || groupadd -o -g "$GROUP_ID" horizonte
fi

# Ajustar UID del usuario horizonte
CURRENT_UID=$(id -u horizonte 2>/dev/null || echo "")
if [ "$CURRENT_UID" != "$USER_ID" ]; then
    usermod -o -u "$USER_ID" -g "$GROUP_ID" horizonte 2>/dev/null
fi

# Inicializar /home/horizonte con los archivos base si ConsoleApp no existe en el volumen
if [ ! -f /home/horizonte/ConsoleApp ] && [ -d /app ]; then
    echo "Inicializando /home/horizonte con los archivos base de la aplicación..."
    mkdir -p /home/horizonte
    cp -a /app/. /home/horizonte/
fi

# Asegurar propiedad y permisos de ejecución en /home/horizonte
chown -R horizonte:horizonte /home/horizonte
if [ -f /home/horizonte/ConsoleApp ]; then
    chmod +x /home/horizonte/ConsoleApp
fi

# Asegurar que el directorio de trabajo sea /home/horizonte
cd /home/horizonte

# Ejecutar el comando especificado como el usuario horizonte
exec gosu horizonte "$@"
