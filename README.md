# HorizonteNet 

**HorizonteNet** es un entorno modular avanzado desarrollado sobre **.NET 10**, diseñado para facilitar la creación de aplicaciones extensibles y distribuidas, a través de paquetes nuget
, 



## Arquitectura Modular

HorizonteNet se basa en una arquitectura de micro-servicios y módulos altamente desacoplados:

- **Core (Horizonte):** El núcleo del sistema que gestiona el contexto (`IhContext`) y el ciclo de vida de los trabajadores (`HWorkersManager`).
- **Admin UI (Horizonte.AdminUi):** Una interfaz de administración moderna y reactiva construida con **Blazor**.
- **Extensiones del Core (Horizonte.Extension):** Las extensiones proporcionan un ecosistema para trabajar con scripts en c#, notebooks, flujos de trabajos y agentes de IA.
- **Módulos (Horizonte.Modules):** Los módulos permiten la extensión de la funcionalidad de HorizonteNet, ofreciendo una variedad de funcionalidades predefinidas y personalizables.
  - **AdminUi** Panel de administración para la gestión de extensiones y módulos.
  - **Scripts** Ejecución y gestión de scripts en C# y Jupyter Notebooks.
  - **Workflows** Orquestación de flujos de trabajo personalizables.
  - **Agents** Definición y ejecución de agentes autónomos.
  - **MCPServer** Expone un servicio Model Context Protol que integra las funcionalidades de los módulos.
  
  
## Características Principales

- **Gestión de Contexto Flexible:** Sistema de configuración basado en JSON con soporte para sobrescrituras locales y de usuario, permitiendo una personalización extrema sin modificar el código base.
- **Manejo de servicios en segundo plano:** Orquestación simplificada de servicios en segundo plano y tareas programadas.
- **Administración de ensamblados** Gestiona la adquisición de paquetes nugets y su uso en la aplicación. Incluyendo código no manejado y contenidos web.
- **Extensibilidad:** Diseñado desde cero para ser ampliado mediante nuevas extensiones, módulos y workflows.
- **Multiplataforma** Disponible para linux, windows y mac.

## Empezando

### Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) o superior.
- Un entorno de desarrollo compatible (Rider, Visual Studio o VS Code).

### Instalación

1. Clona el repositorio:
   ```bash
   git clone https://github.com/franfusto/HorizonteNet.git
   ```
2. Restaura las dependencias:
   ```bash
   dotnet restore
   ```
3. Compila el proyecto:
   ```bash
   dotnet build
   ```

## 🛠️ Contribución y Estado Beta

Estamos en fase **Beta**. HorizonteNet está en desarrollo activo y agradecemos cualquier feedback, reporte de errores o propuestas de mejora.

Contacta a través de  [GitHub Issues](https://github.com/franfusto/HorizonteNet/issues) o [Discord](https://discord.com/channels/1538980325953249290/1538980326527995960).

- **Repositorio:** [https://github.com/franfusto/HorizonteNet](https://github.com/franfusto/HorizonteNet)
- **Autor:** franfusto
- **Licencia:** [MIT](LICENSE)

---

*HorizonteNet - Elevando el desarrollo modular en .NET.*
