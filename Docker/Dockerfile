# Imagen base con el runtime de ASP.NET Core (.NET 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Instalar gosu para permitir el ajuste dinámico de UID/GID manteniendo la ejecución bajo el usuario horizonte
RUN apt-get update && \
    apt-get install -y --no-install-recommends gosu && \
    rm -rf /var/lib/apt/lists/*

# Reconfigurar el usuario/grupo 'app' predeterminado (UID/GID 1000) a 'horizonte' con home /home/horizonte
RUN if getent group app >/dev/null 2>&1; then \
        groupmod -n horizonte app; \
    elif ! getent group 1000 >/dev/null 2>&1 && ! getent group horizonte >/dev/null 2>&1; then \
        groupadd -g 1000 horizonte; \
    fi && \
    if id -u app >/dev/null 2>&1; then \
        usermod -l horizonte -d /home/horizonte -m -s /bin/bash app; \
    elif ! id -u 1000 >/dev/null 2>&1 && ! id -u horizonte >/dev/null 2>&1; then \
        useradd -m -u 1000 -g horizonte -s /bin/bash horizonte; \
    fi

# Establecer directorio de trabajo predeterminado
WORKDIR /home/horizonte

# Exponer el puerto 5000
EXPOSE 5000

# Variables de entorno recomendadas
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_HTTP_PORTS=5000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    PUID=1000 \
    PGID=1000

# Copiar los binarios y configuración de la aplicación a una ruta base inmutable (/app)
COPY --chown=horizonte:horizonte AppFiles/ /app/
COPY --chown=horizonte:horizonte horizonte.json /app/
COPY --chown=horizonte:horizonte packages/ /app/packages/

# Asegurar permisos de ejecución en los binarios base
RUN chmod -R u+x /app

# Copiar script de entrypoint
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Punto de entrada con entrypoint que ajusta UID/GID y ejecuta la app como horizonte
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
CMD ["./ConsoleApp"]
