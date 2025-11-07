# Horizonte.Net — Biblioteca Núcleo

Horizonte es el núcleo del ecosistema Horizonte.Net: un conjunto de librerías y utilidades para construir aplicaciones y servicios .NET 8 modernos (Blazor, servicios en segundo plano, integraciones y agentes asistidos por IA).

Este paquete proporciona las primitivas base (entidades, servicios, extensiones, configuraciones y workers) sobre las que se apoyan los demás proyectos del repositorio:
- Horizonte.Extension.AspNetCore: extensiones para ASP.NET Core (middleware y helpers web).
- Horizonte.Ai.Agent: agente y widgets de chat/IA integrables en apps Blazor.
- Horizonte.AdminUi: interfaz de administración y configuración.
- Samples: proyectos de ejemplo que muestran integraciones (AEMET, Google, notificaciones, Razor, etc.).

## Características clave
- Base sólida para apps .NET 8 con enfoque modular.
- Patrones consistentes para Servicios, Workers y Settings.
- Integración con Blazor para construir widgets reutilizables.
- Extensiones listas para ASP.NET Core.
- Ejemplos reales que aceleran el arranque de nuevos proyectos.

## Requisitos
- .NET 8 SDK (y runtime correspondiente)
- Sistema operativo: Windows, Linux o macOS

## Instalación (NuGet)
Para usar la biblioteca núcleo en tu proyecto:

- dotnet add package Horizonte

También puedes referenciar el proyecto directamente desde el repositorio si trabajas en modo monorepo.

## Inicio rápido
1. Crea un proyecto .NET 8 (por ejemplo, una app de consola o un servicio web).
2. Instala el paquete “Horizonte”.
3. Registra y utiliza los servicios básicos proporcionados por la librería.

Ejemplo mínimo (pseudocódigo):
- using Horizonte;
- // Configura servicios, carga Settings y arranca tus Workers

Para ver un uso real, revisa los proyectos en la carpeta Samples o las extensiones en Horizonte.Extension.AspNetCore.

## Estructura del repositorio (resumen)
- Horizonte/ … Biblioteca núcleo (este README)
- Horizonte.Extension.AspNetCore/ … Extensiones para ASP.NET Core
- Horizonte.Ai.Agent/ … Agente, componentes y widgets IA (Blazor)
- Horizonte.AdminUi/ … UI de administración (Blazor)
- Launchers/ … Ejecutables para Linux/Windows/ARM64
- Samples/ … Proyectos de ejemplo (Aemet, Gmaps, Gcalendar, RazorApp, etc.)

## Ejecutar ejemplos y demos
- Requisitos: .NET 8 SDK instalado.
- Abre la solución HorizonteNet.sln con Rider/VS o usa la CLI.
- Elige un proyecto dentro de Samples o un Launcher y ejecútalo:
  - dotnet run --project Launchers/LinuxApp
  - dotnet run --project Horizonte.Samples.RazorApp

Algunos samples requieren claves/API propias (p.ej., AEMET o Google). Consulta el README específico de cada sample y ajusta su configuración (horizonte.json o appsettings.json donde aplique).

## Conceptos del núcleo
- Services: lógica de negocio y utilidades reutilizables.
- Settings: configuración tipada para carga desde archivos/variables de entorno.
- Workers: tareas en segundo plano y procesos largos.
- Extensions/Helpers: métodos de extensión y utilidades comunes.

## Contribución
Las contribuciones son bienvenidas. Sugerencias, issues y PRs ayudan a mejorar el proyecto. Por favor:
- Abre un issue describiendo la mejora o el bug.
- Acompaña los PRs con una breve explicación y, si aplica, ejemplos.

## Soporte y enlaces
- Repositorio principal: https://github.com/franfusto/HorizonteNet
- Paquete NuGet: “Horizonte”

## Licencia
MIT